```

BenchmarkDotNet v0.13.7, Debian GNU/Linux 12 (bookworm)
Intel Core i7-2630QM CPU 2.00GHz (Sandy Bridge), 1 CPU, 8 logical and 4 physical cores
.NET SDK 7.0.305
  [Host]     : .NET 7.0.8 (7.0.823.31807), X64 RyuJIT AVX
  DefaultJob : .NET 7.0.8 (7.0.823.31807), X64 RyuJIT AVX


```
|         Method |     N |       Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------- |------ |-----------:|----------:|----------:|------:|--------:|
|    **ListIndexOf** |   **100** |   **2.460 μs** | **0.0132 μs** | **0.0124 μs** |  **1.00** |    **0.00** |
|    SpanIndexOf |   100 |   1.858 μs | 0.0188 μs | 0.0176 μs |  0.76 |    0.01 |
| VectorTIndexOf |   100 |   6.092 μs | 0.0262 μs | 0.0232 μs |  2.48 |    0.02 |
|                |       |            |           |           |       |         |
|    **ListIndexOf** | **10000** | **109.151 μs** | **1.5616 μs** | **1.4608 μs** |  **1.00** |    **0.00** |
|    SpanIndexOf | 10000 | 107.074 μs | 0.3517 μs | 0.2937 μs |  0.98 |    0.01 |
| VectorTIndexOf | 10000 | 445.114 μs | 1.3622 μs | 1.2742 μs |  4.08 |    0.06 |
