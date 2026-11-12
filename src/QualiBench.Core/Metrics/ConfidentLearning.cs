namespace QualiBench.Core.Metrics;

public static class ConfidentLearning
{
    public sealed record Result(
        int[] PredictedLabels,
        double[] Confidence,
        double[,,] AnnotatorConfusion,
        List<(int item, int annotator, int label, int predicted, double confidence)> Suspects);

    public static Result Fit(AnnotationMatrix m)
    {
        int N = m.Items, K = m.Annotators;
        int C = m.MaxLabel - m.MinLabel + 1;

        var prior = new int[N];
        for (int u = 0; u < N; u++)
        {
            var counts = new int[C];
            for (int a = 0; a < K; a++)
            {
                int v = m[u, a];
                if (v >= 0) counts[v - m.MinLabel]++;
            }
            int best = 0;
            for (int c = 1; c < C; c++) if (counts[c] > counts[best]) best = c;
            prior[u] = best + m.MinLabel;
        }

        var confusion = new double[K, C, C];
        for (int a = 0; a < K; a++)
        {
            var rowTotals = new int[C];
            for (int u = 0; u < N; u++)
            {
                int v = m[u, a];
                if (v < 0) continue;
                int x = prior[u] - m.MinLabel;
                int y = v - m.MinLabel;
                confusion[a, x, y] += 1;
                rowTotals[x]++;
            }
            for (int x = 0; x < C; x++)
            {
                double total = rowTotals[x];
                if (total == 0) continue;
                for (int y = 0; y < C; y++)
                    confusion[a, x, y] /= total;
            }
        }

        var predicted = new int[N];
        var confidence = new double[N];
        var suspects = new List<(int, int, int, int, double)>();

        for (int u = 0; u < N; u++)
        {
            var posterior = new double[C];
            double sum = 0;
            for (int c = 0; c < C; c++)
            {
                double p = 1.0;
                for (int a = 0; a < K; a++)
                {
                    int v = m[u, a];
                    if (v < 0) continue;
                    p *= Math.Max(confusion[a, c, v - m.MinLabel], 1e-9);
                }
                posterior[c] = p;
                sum += p;
            }
            if (sum < 1e-30)
            {
                predicted[u] = prior[u];
                confidence[u] = 0;
                continue;
            }
            int best = 0;
            for (int c = 1; c < C; c++) if (posterior[c] > posterior[best]) best = c;
            predicted[u] = best + m.MinLabel;
            confidence[u] = posterior[best] / sum;

            if (confidence[u] >= 0.50)
            {
                for (int a = 0; a < K; a++)
                {
                    int v = m[u, a];
                    if (v >= 0 && v != predicted[u])
                        suspects.Add((u, a, v, predicted[u], confidence[u]));
                }
            }
        }

        return new Result(predicted, confidence, confusion, suspects);
    }
}
