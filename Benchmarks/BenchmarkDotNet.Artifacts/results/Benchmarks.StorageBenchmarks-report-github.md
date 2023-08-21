```

BenchmarkDotNet v0.13.7, Debian GNU/Linux 12 (bookworm)
Intel Core i7-2630QM CPU 2.00GHz (Sandy Bridge), 1 CPU, 8 logical and 4 physical cores
.NET SDK 7.0.305
  [Host]     : .NET 7.0.8 (7.0.823.31807), X64 RyuJIT AVX
  DefaultJob : .NET 7.0.8 (7.0.823.31807), X64 RyuJIT AVX


```
|                           Method |     N |          Action |       Mean |     Error |    StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------------- |------ |---------------- |-----------:|----------:|----------:|------:|--------:|----------:|------------:|
|                       **DoubleList** | **10000** |          **Add100** |   **2.877 μs** | **0.0180 μs** | **0.0168 μs** |  **1.00** |    **0.00** |         **-** |          **NA** |
|           SparseLinearDictionary | 10000 |          Add100 |   3.666 μs | 0.0207 μs | 0.0193 μs |  1.27 |    0.01 |         - |          NA |
| SparseLinearDictionaryWithLookup | 10000 |          Add100 |  13.009 μs | 0.0762 μs | 0.0713 μs |  4.52 |    0.04 |         - |          NA |
|                                  |       |                 |            |           |           |       |         |           |             |
|                       **DoubleList** | **10000** |       **Remove100** | **237.002 μs** | **1.4293 μs** | **1.1159 μs** |  **1.00** |    **0.00** |         **-** |          **NA** |
|           SparseLinearDictionary | 10000 |       Remove100 | 240.450 μs | 0.5420 μs | 0.4526 μs |  1.01 |    0.01 |         - |          NA |
| SparseLinearDictionaryWithLookup | 10000 |       Remove100 |  29.529 μs | 0.0966 μs | 0.0903 μs |  0.12 |    0.00 |         - |          NA |
|                                  |       |                 |            |           |           |       |         |           |             |
|                       **DoubleList** | **10000** |    **RemoveAdd100** | **282.278 μs** | **0.9162 μs** | **0.7650 μs** |  **1.00** |    **0.00** |         **-** |          **NA** |
|           SparseLinearDictionary | 10000 |    RemoveAdd100 | 212.446 μs | 3.3259 μs | 3.1110 μs |  0.75 |    0.01 |         - |          NA |
| SparseLinearDictionaryWithLookup | 10000 |    RemoveAdd100 |  25.993 μs | 0.1257 μs | 0.1176 μs |  0.09 |    0.00 |         - |          NA |
|                                  |       |                 |            |           |           |       |         |           |             |
|                       **DoubleList** | **10000** | **RandomLookup100** | **113.938 μs** | **0.5642 μs** | **0.4712 μs** |  **1.00** |    **0.00** |         **-** |          **NA** |
|           SparseLinearDictionary | 10000 | RandomLookup100 | 114.589 μs | 0.3907 μs | 0.3464 μs |  1.01 |    0.00 |         - |          NA |
| SparseLinearDictionaryWithLookup | 10000 | RandomLookup100 |  31.016 μs | 0.0616 μs | 0.0514 μs |  0.27 |    0.00 |         - |          NA |
|                                  |       |                 |            |           |           |       |         |           |             |
|                       **DoubleList** | **10000** | **RandomUpdate100** | **119.658 μs** | **1.9028 μs** | **1.4856 μs** |  **1.00** |    **0.00** |         **-** |          **NA** |
|           SparseLinearDictionary | 10000 | RandomUpdate100 | 122.354 μs | 2.3338 μs | 2.3967 μs |  1.02 |    0.02 |         - |          NA |
| SparseLinearDictionaryWithLookup | 10000 | RandomUpdate100 |  36.027 μs | 0.6835 μs | 0.6393 μs |  0.30 |    0.00 |         - |          NA |
|                                  |       |                 |            |           |           |       |         |           |             |
|                       **DoubleList** | **10000** |         **Process** | **113.235 μs** | **2.0998 μs** | **2.0622 μs** |  **1.00** |    **0.00** |         **-** |          **NA** |
|           SparseLinearDictionary | 10000 |         Process | 101.252 μs | 0.5740 μs | 0.5088 μs |  0.90 |    0.02 |         - |          NA |
| SparseLinearDictionaryWithLookup | 10000 |         Process |  85.529 μs | 0.6334 μs | 0.5615 μs |  0.76 |    0.01 |         - |          NA |
|                                  |       |                 |            |           |           |       |         |           |             |
|                       **DoubleList** | **10000** |         **ForEach** | **305.507 μs** | **0.4585 μs** | **0.3580 μs** |  **1.00** |    **0.00** |         **-** |          **NA** |
|           SparseLinearDictionary | 10000 |         ForEach | 311.499 μs | 6.1796 μs | 6.0691 μs |  1.02 |    0.02 |         - |          NA |
| SparseLinearDictionaryWithLookup | 10000 |         ForEach | 278.791 μs | 0.7130 μs | 0.6321 μs |  0.91 |    0.00 |         - |          NA |
