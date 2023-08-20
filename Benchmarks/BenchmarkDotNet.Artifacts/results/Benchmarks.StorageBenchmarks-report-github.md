```

BenchmarkDotNet v0.13.7, Debian GNU/Linux 12 (bookworm)
Intel Core i7-2630QM CPU 2.00GHz (Sandy Bridge), 1 CPU, 8 logical and 4 physical cores
.NET SDK 7.0.305
  [Host]     : .NET 7.0.8 (7.0.823.31807), X64 RyuJIT AVX
  DefaultJob : .NET 7.0.8 (7.0.823.31807), X64 RyuJIT AVX


```
|                             Method |     N |       Mean |     Error |    StdDev | Allocated |
|----------------------------------- |------ |-----------:|----------:|----------:|----------:|
|                      **DoubleListAdd** |   **100** |   **2.826 μs** | **0.0171 μs** | **0.0160 μs** |         **-** |
|                   DoubleListRemove |   100 |   1.378 μs | 0.0036 μs | 0.0032 μs |         - |
|             DoubleListRandomLookup |   100 |   3.011 μs | 0.0154 μs | 0.0144 μs |         - |
|             DoubleListRandomUpdate |   100 |   8.806 μs | 0.0433 μs | 0.0405 μs |         - |
|                  DoubleListProcess |   100 |   5.882 μs | 0.0145 μs | 0.0136 μs |     144 B |
|                  DoubleListForEach |   100 |   7.924 μs | 0.0209 μs | 0.0185 μs |         - |
|          SparseLinearDictionaryAdd |   100 |  13.479 μs | 0.0438 μs | 0.0410 μs |         - |
|       SparseLinearDictionaryRemove |   100 |   1.529 μs | 0.0027 μs | 0.0022 μs |         - |
| SparseLinearDictionaryRandomLookup |   100 |  12.075 μs | 0.0431 μs | 0.0403 μs |         - |
| SparseLinearDictionaryRandomUpdate |   100 |  19.781 μs | 0.0373 μs | 0.0331 μs |         - |
|      SparseLinearDictionaryProcess |   100 |   5.790 μs | 0.0465 μs | 0.0435 μs |     144 B |
|      SparseLinearDictionaryForEach |   100 |   8.000 μs | 0.0506 μs | 0.0473 μs |         - |
|                      **DoubleListAdd** |  **1000** |   **2.857 μs** | **0.0098 μs** | **0.0087 μs** |         **-** |
|                   DoubleListRemove |  1000 |  22.512 μs | 0.1156 μs | 0.1082 μs |         - |
|             DoubleListRandomLookup |  1000 |  13.704 μs | 0.0331 μs | 0.0276 μs |         - |
|             DoubleListRandomUpdate |  1000 |  20.354 μs | 0.0240 μs | 0.0201 μs |         - |
|                  DoubleListProcess |  1000 |  16.537 μs | 0.0296 μs | 0.0262 μs |     144 B |
|                  DoubleListForEach |  1000 | 108.309 μs | 0.2217 μs | 0.2074 μs |         - |
|          SparseLinearDictionaryAdd |  1000 |  13.644 μs | 0.0434 μs | 0.0406 μs |         - |
|       SparseLinearDictionaryRemove |  1000 |  19.643 μs | 0.0609 μs | 0.0539 μs |         - |
| SparseLinearDictionaryRandomLookup |  1000 |  21.613 μs | 0.1281 μs | 0.1198 μs |         - |
| SparseLinearDictionaryRandomUpdate |  1000 |  28.246 μs | 0.0609 μs | 0.0540 μs |         - |
|      SparseLinearDictionaryProcess |  1000 |  13.932 μs | 0.0524 μs | 0.0490 μs |     144 B |
|      SparseLinearDictionaryForEach |  1000 |  36.702 μs | 0.1377 μs | 0.1288 μs |         - |
|                      **DoubleListAdd** | **10000** |   **2.845 μs** | **0.0052 μs** | **0.0048 μs** |         **-** |
|                   DoubleListRemove | 10000 | 271.559 μs | 3.3630 μs | 3.1458 μs |         - |
|             DoubleListRandomLookup | 10000 | 126.004 μs | 0.1691 μs | 0.1499 μs |         - |
|             DoubleListRandomUpdate | 10000 | 119.009 μs | 0.2000 μs | 0.1871 μs |         - |
|                  DoubleListProcess | 10000 | 115.950 μs | 0.1457 μs | 0.1363 μs |     144 B |
|                  DoubleListForEach | 10000 | 311.940 μs | 0.4334 μs | 0.3619 μs |         - |
|          SparseLinearDictionaryAdd | 10000 |  11.723 μs | 0.0465 μs | 0.0435 μs |         - |
|       SparseLinearDictionaryRemove | 10000 |  29.310 μs | 0.1038 μs | 0.0971 μs |         - |
| SparseLinearDictionaryRandomLookup | 10000 |  29.138 μs | 0.0436 μs | 0.0341 μs |         - |
| SparseLinearDictionaryRandomUpdate | 10000 |  35.365 μs | 0.0510 μs | 0.0477 μs |         - |
|      SparseLinearDictionaryProcess | 10000 |  92.362 μs | 0.0475 μs | 0.0397 μs |     144 B |
|      SparseLinearDictionaryForEach | 10000 | 292.755 μs | 0.4660 μs | 0.4359 μs |         - |
