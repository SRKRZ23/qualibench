using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using QualiBench.Core;
using QualiBench.Core.Metrics;

[MemoryDiagnoser]
public class AuditBenchmarks
{
    private AnnotationMatrix _m = null!;

    [GlobalSetup]
    public void Setup()
    {
        var (m, _) = SyntheticData.Generate(50_000, 10, 8, noiseRate: 0.10, seed: 7);
        _m = m;
    }

    [Benchmark] public double KrippendorffAlpha()       => KrippendorffsAlpha.Nominal(_m);
    [Benchmark] public double[] AnnotatorReliability()  => MetricsWrapper.AnnotatorReliability(_m);
    [Benchmark] public double[,] CohenKappaPairwise()   => CohensKappa.Pairwise(_m);
    [Benchmark] public int FlagSuspects()               => ConsensusNoiseDetector.FlagSuspectLabels(_m).Count;
}

internal static class MetricsWrapper
{
    public static double[] AnnotatorReliability(AnnotationMatrix m)
        => QualiBench.Core.Metrics.AnnotatorReliability.Compute(m);
}

public static class Program
{
    public static void Main(string[] args) => BenchmarkRunner.Run<AuditBenchmarks>(args: args);
}
