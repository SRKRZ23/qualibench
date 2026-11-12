namespace QualiBench.Core.Metrics;

public static class CohensKappa
{
    public static double Compute(ReadOnlySpan<int> a, ReadOnlySpan<int> b)
    {
        if (a.Length != b.Length) throw new ArgumentException("Length mismatch");
        int n = 0; long agree = 0;
        var countsA = new Dictionary<int, long>();
        var countsB = new Dictionary<int, long>();
        for (int i = 0; i < a.Length; i++)
        {
            int x = a[i], y = b[i];
            if (x < 0 || y < 0) continue;
            n++;
            if (x == y) agree++;
            countsA[x] = countsA.GetValueOrDefault(x) + 1;
            countsB[y] = countsB.GetValueOrDefault(y) + 1;
        }
        if (n == 0) return double.NaN;
        double po = (double)agree / n;
        double pe = 0;
        foreach (var (lab, ca) in countsA)
            if (countsB.TryGetValue(lab, out var cb))
                pe += ((double)ca / n) * ((double)cb / n);
        if (Math.Abs(1.0 - pe) < 1e-12) return double.NaN;
        return (po - pe) / (1.0 - pe);
    }

    public static double[,] Pairwise(AnnotationMatrix m)
    {
        int k = m.Annotators;
        var result = new double[k, k];
        var colA = new int[m.Items];
        var colB = new int[m.Items];
        for (int i = 0; i < k; i++)
        {
            result[i, i] = 1.0;
            for (int j = i + 1; j < k; j++)
            {
                for (int r = 0; r < m.Items; r++)
                {
                    colA[r] = m[r, i];
                    colB[r] = m[r, j];
                }
                double kappa = Compute(colA, colB);
                result[i, j] = kappa; result[j, i] = kappa;
            }
        }
        return result;
    }
}
