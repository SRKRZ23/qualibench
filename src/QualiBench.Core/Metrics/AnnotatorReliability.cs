namespace QualiBench.Core.Metrics;

public static class AnnotatorReliability
{
    public static double[] Compute(AnnotationMatrix m)
    {
        int K = m.Annotators, N = m.Items;
        var agree = new long[K];
        var total = new long[K];

        for (int u = 0; u < N; u++)
        {
            var counts = new Dictionary<int, int>();
            for (int a = 0; a < K; a++)
            {
                int v = m[u, a];
                if (v >= 0) counts[v] = counts.GetValueOrDefault(v) + 1;
            }
            if (counts.Count == 0) continue;

            int consensus = -1, best = -1; bool tie = false;
            foreach (var (lab, c) in counts)
            {
                if (c > best) { best = c; consensus = lab; tie = false; }
                else if (c == best) tie = true;
            }
            if (tie) continue;

            for (int a = 0; a < K; a++)
            {
                int v = m[u, a];
                if (v < 0) continue;
                total[a]++;
                if (v == consensus) agree[a]++;
            }
        }

        var rel = new double[K];
        for (int a = 0; a < K; a++)
            rel[a] = total[a] == 0 ? double.NaN : (double)agree[a] / total[a];
        return rel;
    }
}
