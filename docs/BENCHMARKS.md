```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.6 (24G84) [Darwin 24.6.0]
Apple M3 Pro, 1 CPU, 11 logical and 11 physical cores
.NET SDK 10.0.403
  [Host]     : .NET 10.0.14 (10.0.14, 10.0.1426.52326), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.14 (10.0.14, 10.0.1426.52326), Arm64 RyuJIT armv8.0-a


```
| Method               | Mean      | Error     | StdDev    | Gen0      | Gen1      | Gen2     | Allocated   |
|--------------------- |----------:|----------:|----------:|----------:|----------:|---------:|------------:|
| KrippendorffAlpha    | 15.620 ms | 0.0184 ms | 0.0153 ms | 2031.2500 | 1250.0000 | 468.7500 | 14689.41 KB |
| AnnotatorReliability |  5.742 ms | 0.0070 ms | 0.0062 ms | 1601.5625 |         - |        - | 13125.89 KB |
| CohenKappaPairwise   | 22.055 ms | 0.0319 ms | 0.0283 ms |   62.5000 |   62.5000 |  62.5000 |    479.1 KB |
| FlagSuspects         |  5.695 ms | 0.0068 ms | 0.0060 ms | 2601.5625 |  984.3750 | 984.3750 | 16346.48 KB |
