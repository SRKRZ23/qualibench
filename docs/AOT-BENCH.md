## NativeAOT vs JIT — M3 Pro, .NET 10

### help (startup only)
Benchmark 1: dotnet src/QualiBench.Cli/bin/Release/net10.0/QualiBench.Cli.dll (20 runs)
                mean     ±       σ          min     …     max
  Wall Time     19.7 ms  ±     1.3 ms      18.6 ms  …    23.4 ms
  Memory        36.4 MiB ±     0.1 MiB     36.2 MiB …    36.7 MiB
 
Benchmark 2: ./publish-aot/QualiBench.Cli (20 runs)
                mean     ±       σ          min     …     max
  Wall Time      3.0 ms  ±     0.2 ms       2.6 ms  …     3.4 ms           -85.0%  (6.6x faster)
  Memory         4.5 MiB ±     0.1 MiB      4.3 MiB …     4.6 MiB          -87.7%  (8.1x less)
 

### evaluate
Benchmark 1: dotnet src/QualiBench.Cli/bin/Release/net10.0/QualiBench.Cli.dll evaluate synthetic.csv truth.csv (20 runs)
                mean     ±       σ          min     …     max
  Wall Time    129.1 ms  ±     3.2 ms     125.3 ms  …   137.3 ms
  Memory        72.5 MiB ±     0.4 MiB     71.8 MiB …    73.3 MiB
 
Benchmark 2: ./publish-aot/QualiBench.Cli evaluate synthetic.csv truth.csv (20 runs)
                mean     ±       σ          min     …     max
  Wall Time     77.4 ms  ±     1.3 ms      75.2 ms  …    79.8 ms           -40.0%  (1.7x faster)
  Memory        35.0 MiB ±     0.1 MiB     34.8 MiB …    35.3 MiB          -51.8%  (2.1x less)
 

### sweep
Benchmark 1: dotnet src/QualiBench.Cli/bin/Release/net10.0/QualiBench.Cli.dll sweep synthetic.csv truth.csv (20 runs)
                mean     ±       σ          min     …     max
  Wall Time    190.7 ms  ±     4.1 ms     186.1 ms  …   202.5 ms
  Memory        78.9 MiB ±     0.2 MiB     78.6 MiB …    79.3 MiB
 
Benchmark 2: ./publish-aot/QualiBench.Cli sweep synthetic.csv truth.csv (20 runs)
                mean     ±       σ          min     …     max
  Wall Time    173.6 ms  ±     2.5 ms     170.6 ms  …   179.2 ms            -9.0%
  Memory        39.7 MiB ±     0.1 MiB     39.6 MiB …    39.8 MiB          -49.6%  (2.0x less)
 

### binary size
-rwxr-xr-x  1 sardorrazikov1  staff   1.6M Nov 12 15:57 publish-aot/QualiBench.Cli

### dynamic dependencies
publish-aot/QualiBench.Cli:
	/usr/lib/libSystem.B.dylib (compatibility version 1.0.0, current version 1351.0.0)
	/usr/lib/libobjc.A.dylib (compatibility version 1.0.0, current version 228.0.0)
	/usr/lib/swift/libswiftCore.dylib (compatibility version 1.0.0, current version 0.0.0)
	/usr/lib/swift/libswiftFoundation.dylib (compatibility version 1.0.0, current version 120.100.0)
	/usr/lib/libicucore.A.dylib (compatibility version 1.0.0, current version 76.1.0)
	/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation (compatibility version 150.0.0, current version 3502.1.255)
	/System/Library/Frameworks/CryptoKit.framework/Versions/A/CryptoKit (compatibility version 1.0.0, current version 1.0.0)
	/System/Library/Frameworks/Foundation.framework/Versions/C/Foundation (compatibility version 300.0.0, current version 3502.1.255)
	/System/Library/Frameworks/Network.framework/Versions/A/Network (compatibility version 1.0.0, current version 4277.121.5)
	/System/Library/Frameworks/Security.framework/Versions/A/Security (compatibility version 1.0.0, current version 61439.120.27)
	/System/Library/Frameworks/GSS.framework/Versions/A/GSS (compatibility version 1.0.0, current version 1.0.0)
