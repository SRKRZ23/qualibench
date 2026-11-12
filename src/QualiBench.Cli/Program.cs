using System.Diagnostics;
using QualiBench.Core;
using QualiBench.Core.Metrics;

double? failOnNoise = null;
var filtered = new List<string>();
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--fail-on-noise" && i + 1 < args.Length)
    {
        failOnNoise = double.Parse(args[i + 1]);
        i++;
    }
    else filtered.Add(args[i]);
}
args = filtered.ToArray();

string mode = args.Length > 0 ? args[0] : "help";

switch (mode)
{
    case "generate":
    {
        int items       = args.Length > 1 ? int.Parse(args[1]) : 100_000;
        int annotators  = args.Length > 2 ? int.Parse(args[2]) : 10;
        int classes     = args.Length > 3 ? int.Parse(args[3]) : 8;
        double noise    = args.Length > 4 ? double.Parse(args[4]) : 0.10;
        string outPath  = args.Length > 5 ? args[5] : "synthetic.csv";

        Console.WriteLine($"Generating {items} items x {annotators} annotators, {classes} classes, {noise:P0} noise...");
        var sw = Stopwatch.StartNew();
        var (m, truth) = SyntheticData.Generate(items, annotators, classes, noiseRate: noise, seed: 7);
        m.SaveCsv(outPath);
        File.WriteAllLines("truth.csv", truth.Select(x => x.ToString()));
        sw.Stop();
        Console.WriteLine($"Wrote {outPath} and truth.csv in {sw.ElapsedMilliseconds} ms");
        break;
    }

    case "audit":
    {
        string path = args.Length > 1 ? args[1] : "synthetic.csv";
        Console.WriteLine($"Loading {path}...");
        var m = AnnotationMatrix.LoadCsv(path);
        Console.WriteLine($"  Items={m.Items:N0}  Annotators={m.Annotators}  Labels=[{m.MinLabel}..{m.MaxLabel}]");

        var sw = Stopwatch.StartNew();

        var rel = AnnotatorReliability.Compute(m);
        Console.WriteLine("\n== Annotator reliability (agreement with consensus) ==");
        for (int a = 0; a < rel.Length; a++)
            Console.WriteLine($"  A{a,2}: {(double.IsNaN(rel[a]) ? "n/a" : rel[a].ToString("P2"))}");

        var alpha = KrippendorffsAlpha.Nominal(m);
        Console.WriteLine($"\n== Krippendorff's alpha (nominal) ==");
        Console.WriteLine($"  alpha = {alpha:F4}  (>=0.80 good, 0.67-0.80 tentative)");

        var kappas = CohensKappa.Pairwise(m);
        int show = Math.Min(5, m.Annotators);
        Console.WriteLine($"\n== Cohen's kappa (first {show}x{show}) ==");
        for (int i = 0; i < show; i++)
        {
            for (int j = 0; j < show; j++) Console.Write($"{kappas[i, j],8:F3}");
            Console.WriteLine();
        }

        var flagged = ConsensusNoiseDetector.FlagSuspectLabels(m, 0.75, 3);
        Console.WriteLine($"\n== Suspicious labels flagged: {flagged.Count:N0} ==");
        foreach (var f in flagged.Take(10))
            Console.WriteLine($"  item={f.item,6}  annotator={f.annotator,2}  label={f.label}  consensus={f.consensus}");

        sw.Stop();
        Console.WriteLine($"\nTotal audit time: {sw.Elapsed.TotalMilliseconds:F1} ms");
        break;
    }

    case "evaluate":
    {
        string path  = args.Length > 1 ? args[1] : "synthetic.csv";
        string tpath = args.Length > 2 ? args[2] : "truth.csv";
        var m = AnnotationMatrix.LoadCsv(path);
        var truth = File.ReadAllLines(tpath).Select(int.Parse).ToArray();
        if (truth.Length != m.Items) throw new InvalidDataException("truth length != items");

        var sw = Stopwatch.StartNew();
        var flagged = ConsensusNoiseDetector.FlagSuspectLabels(m, 0.75, 3);
        sw.Stop();

        var flaggedSet = flagged.Select(f => (f.item, f.annotator)).ToHashSet();

        long trueNoise = 0, correct = 0, falsePositives = 0;
        for (int u = 0; u < m.Items; u++)
            for (int a = 0; a < m.Annotators; a++)
            {
                int v = m[u, a];
                if (v < 0) continue;
                bool isNoisy = v != truth[u];
                bool isFlagged = flaggedSet.Contains((u, a));
                if (isNoisy) trueNoise++;
                if (isFlagged && isNoisy) correct++;
                if (isFlagged && !isNoisy) falsePositives++;
            }

        double precision = flagged.Count == 0 ? 0 : (double)correct / flagged.Count;
        double recall    = trueNoise == 0   ? 0 : (double)correct / trueNoise;
        double f1        = (precision + recall) == 0 ? 0 : 2 * precision * recall / (precision + recall);

        Console.WriteLine($"== QualiBench vs. ground truth ({m.Items:N0} items, {m.Annotators} annotators) ==");
        Console.WriteLine($"  Injected noisy labels : {trueNoise:N0}");
        Console.WriteLine($"  Flagged by QualiBench : {flagged.Count:N0}  (false positives: {falsePositives:N0})");
        Console.WriteLine($"  Precision             : {precision:P2}");
        Console.WriteLine($"  Recall                : {recall:P2}");
        Console.WriteLine($"  F1                    : {f1:F3}");
        Console.WriteLine($"  Audit wall-clock      : {sw.Elapsed.TotalMilliseconds:F1} ms");
        if (failOnNoise.HasValue && f1 < failOnNoise.Value)
        {
            Console.Error.WriteLine($"FAIL: F1 {f1:F3} < threshold {failOnNoise.Value:F3}");
            Environment.Exit(1);
        }
        break;
    }

    case "sweep":
    {
        string path  = args.Length > 1 ? args[1] : "synthetic.csv";
        string tpath = args.Length > 2 ? args[2] : "truth.csv";
        int sample   = args.Length > 3 ? int.Parse(args[3]) : int.MaxValue;

        var m = AnnotationMatrix.LoadCsv(path);
        var truth = File.ReadAllLines(tpath).Select(int.Parse).ToArray();
        if (truth.Length != m.Items) throw new InvalidDataException("truth length != items");

        var sw = Stopwatch.StartNew();
        var rows = ThresholdSweep.Run(m, truth, sampleItems: sample);
        sw.Stop();

        Console.WriteLine($"== Precision / Recall frontier ({Math.Min(m.Items, sample):N0} items, {m.Annotators} annotators) ==");
        Console.WriteLine();
        Console.WriteLine($"{"minAgree",10} {"minAnn",7} {"flagged",9} {"precision",11} {"recall",9} {"F1",7}");
        Console.WriteLine(new string('-', 60));
        foreach (var r in rows)
            Console.WriteLine($"{r.MinAgreement,10:F2} {r.MinAnnotations,7} {r.Flagged,9:N0} {r.Precision,11:P2} {r.Recall,9:P2} {r.F1,7:F3}");
        Console.WriteLine();
        Console.WriteLine($"Sweep wall-clock: {sw.Elapsed.TotalMilliseconds:F1} ms for {rows.Count} operating points");
        break;
    }

    case "cl":
    {
        string path  = args.Length > 1 ? args[1] : "synthetic.csv";
        string tpath = args.Length > 2 ? args[2] : "truth.csv";
        var m = AnnotationMatrix.LoadCsv(path);
        var truth = File.ReadAllLines(tpath).Select(int.Parse).ToArray();
        if (truth.Length != m.Items) throw new InvalidDataException("truth length != items");

        var sw = Stopwatch.StartNew();
        var result = ConfidentLearning.Fit(m);
        sw.Stop();

        int itemCorrect = 0;
        for (int u = 0; u < m.Items; u++)
            if (result.PredictedLabels[u] == truth[u]) itemCorrect++;
        double itemAcc = (double)itemCorrect / m.Items;

        var suspectSet = result.Suspects.Select(s => (s.item, s.annotator)).ToHashSet();
        long trueNoise = 0, correct = 0, fp = 0;
        for (int u = 0; u < m.Items; u++)
            for (int a = 0; a < m.Annotators; a++)
            {
                int v = m[u, a];
                if (v < 0) continue;
                bool noisy = v != truth[u];
                bool flagged = suspectSet.Contains((u, a));
                if (noisy) trueNoise++;
                if (flagged && noisy) correct++;
                if (flagged && !noisy) fp++;
            }
        double precision = result.Suspects.Count == 0 ? 0 : (double)correct / result.Suspects.Count;
        double recall    = trueNoise == 0 ? 0 : (double)correct / trueNoise;
        double f1        = (precision + recall) == 0 ? 0 : 2 * precision * recall / (precision + recall);

        Console.WriteLine($"== Confident Learning ({m.Items:N0} items, {m.Annotators} annotators) ==");
        Console.WriteLine($"  Item-level accuracy    : {itemAcc:P2}   (predicted label == truth)");
        Console.WriteLine($"  Suspects flagged       : {result.Suspects.Count:N0}");
        Console.WriteLine($"  Noise precision        : {precision:P2}");
        Console.WriteLine($"  Noise recall           : {recall:P2}");
        Console.WriteLine($"  F1                     : {f1:F3}");
        Console.WriteLine($"  Fit wall-clock         : {sw.Elapsed.TotalMilliseconds:F1} ms");
        if (failOnNoise.HasValue && f1 < failOnNoise.Value)
        {
            Console.Error.WriteLine($"FAIL: F1 {f1:F3} < threshold {failOnNoise.Value:F3}");
            Environment.Exit(1);
        }
        break;
    }

    default:
        Console.WriteLine("QualiBench — independent label-quality auditor");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  qualibench generate [items=100000] [annotators=10] [classes=8] [noise=0.10] [out=synthetic.csv]");
        Console.WriteLine("  qualibench audit    [path=synthetic.csv]");
        Console.WriteLine("  qualibench evaluate [path=synthetic.csv] [truth=truth.csv]");
        Console.WriteLine("  qualibench sweep    [path=synthetic.csv] [truth=truth.csv] [sampleItems=all]");
        Console.WriteLine("  qualibench cl       [path=synthetic.csv] [truth=truth.csv]");
        break;
}
