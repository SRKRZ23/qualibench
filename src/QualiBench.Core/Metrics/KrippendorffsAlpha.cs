namespace QualiBench.Core.Metrics;

public static class KrippendorffsAlpha
{
    public static double Nominal(AnnotationMatrix m)
    {
        int N = m.Items, K = m.Annotators;
        var perItem = new Dictionary<int, int>[N];
        long n = 0;

        for (int u = 0; u < N; u++)
        {
            var d = new Dictionary<int, int>();
            for (int a = 0; a < K; a++)
            {
                int v = m[u, a];
                if (v < 0) continue;
                d[v] = d.GetValueOrDefault(v) + 1;
                n++;
            }
            perItem[u] = d;
        }
        if (n == 0) return double.NaN;

        double Do = 0;
        for (int u = 0; u < N; u++)
        {
            var d = perItem[u];
            long mu = 0; foreach (var v in d.Values) mu += v;
            if (mu < 2) continue;
            long sq = 0; foreach (var v in d.Values) sq += (long)v * v;
            Do += (double)((long)mu * mu - sq) / (mu - 1);
        }
        Do /= n;

        var total = new Dictionary<int, long>();
        for (int u = 0; u < N; u++)
            foreach (var (lab, c) in perItem[u])
                total[lab] = total.GetValueOrDefault(lab) + c;

        double sumNc2 = 0;
        foreach (var c in total.Values) sumNc2 += (double)c * c;
        double De = ((double)n * n - sumNc2) / ((double)n * (n - 1));

        if (De < 1e-12) return double.NaN;
        return 1.0 - Do / De;
    }
}
