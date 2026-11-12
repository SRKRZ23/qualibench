namespace QualiBench.Core.Metrics;

public static class ConsensusNoiseDetector
{
    public readonly record struct ItemResult(int Item, int MajorityLabel, double AgreementFraction, int NumAnnotations);

    public static ItemResult[] Analyze(AnnotationMatrix m)
    {
        int K = m.Annotators, N = m.Items;
        var result = new ItemResult[N];
        for (int u = 0; u < N; u++)
        {
            var counts = new Dictionary<int, int>();
            int total = 0;
            for (int a = 0; a < K; a++)
            {
                int v = m[u, a];
                if (v >= 0) { counts[v] = counts.GetValueOrDefault(v) + 1; total++; }
            }
            if (total == 0) { result[u] = new ItemResult(u, -1, 0, 0); continue; }
            int consensus = -1, best = -1;
            foreach (var (lab, c) in counts)
                if (c > best) { best = c; consensus = lab; }
            result[u] = new ItemResult(u, consensus, (double)best / total, total);
        }
        return result;
    }

    public static List<(int item, int annotator, int label, int consensus)> FlagSuspectLabels(
        AnnotationMatrix m, double minAgreement = 0.75, int minAnnotations = 3)
    {
        var items = Analyze(m);
        var flagged = new List<(int, int, int, int)>();
        for (int u = 0; u < m.Items; u++)
        {
            var it = items[u];
            if (it.NumAnnotations < minAnnotations) continue;
            if (it.AgreementFraction < minAgreement) continue;
            if (it.MajorityLabel < 0) continue;
            for (int a = 0; a < m.Annotators; a++)
            {
                int v = m[u, a];
                if (v >= 0 && v != it.MajorityLabel)
                    flagged.Add((u, a, v, it.MajorityLabel));
            }
        }
        return flagged;
    }
}
