namespace QualiBench.Core.Metrics;

public static class ThresholdSweep
{
    public readonly record struct Row(
        double MinAgreement,
        int MinAnnotations,
        int Flagged,
        double Precision,
        double Recall,
        double F1);

    /// <summary>Sweep the flagging detector over (agreement, minAnnotations).</summary>
    public static List<Row> Run(
        AnnotationMatrix m,
        int[] truth,
        double[] agreements = null!,
        int[] minAnnots = null!,
        int sampleItems = int.MaxValue)
    {
        agreements ??= new[] { 0.50, 0.60, 0.65, 0.70, 0.75, 0.80, 0.85, 0.90 };
        minAnnots  ??= new[] { 2, 3, 4, 5 };

        int items = Math.Min(m.Items, sampleItems);
        int K = m.Annotators;

        // Precompute true-noise masks (only over sampled items for speed)
        long totalNoise = 0;
        for (int u = 0; u < items; u++)
            for (int a = 0; a < K; a++)
            {
                int v = m[u, a];
                if (v >= 0 && v != truth[u]) totalNoise++;
            }

        var rows = new List<Row>();
        foreach (var agree in agreements)
        {
            var items_ = ConsensusNoiseDetector.Analyze(m);
            foreach (var minAnn in minAnnots)
            {
                long flagged = 0, correct = 0;
                for (int u = 0; u < items; u++)
                {
                    var it = items_[u];
                    if (it.NumAnnotations < minAnn) continue;
                    if (it.AgreementFraction < agree) continue;
                    if (it.MajorityLabel < 0) continue;
                    for (int a = 0; a < K; a++)
                    {
                        int v = m[u, a];
                        if (v < 0 || v == it.MajorityLabel) continue;
                        flagged++;
                        if (v != truth[u]) correct++;
                    }
                }
                double precision = flagged == 0 ? 0 : (double)correct / flagged;
                double recall    = totalNoise == 0 ? 0 : (double)correct / totalNoise;
                double f1        = (precision + recall) == 0 ? 0
                                   : 2 * precision * recall / (precision + recall);
                rows.Add(new Row(agree, minAnn, (int)flagged, precision, recall, f1));
            }
        }
        return rows;
    }
}
