```

BenchmarkDotNet v0.13.7, Debian GNU/Linux 12 (bookworm)
Intel Core i7-2630QM CPU 2.00GHz (Sandy Bridge), 1 CPU, 8 logical and 4 physical cores
.NET SDK 7.0.305
  [Host]     : .NET 7.0.8 (7.0.823.31807), X64 RyuJIT AVX
  DefaultJob : .NET 7.0.8 (7.0.823.31807), X64 RyuJIT AVX


```
|                 Method |   N |          Action |        Mean |    Error |   StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------- |---- |---------------- |------------:|---------:|---------:|------:|--------:|----------:|------------:|
|             **DoubleList** | **100** |          **Add100** |  **2,832.6 ns** | **12.15 ns** | **10.14 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| SparseLinearDictionary | 100 |          Add100 | 11,695.9 ns | 97.99 ns | 91.66 ns |  4.13 |    0.03 |         - |          NA |
|                        |     |                 |             |          |          |       |         |           |             |
|             **DoubleList** | **100** |       **Remove100** |  **1,378.5 ns** |  **4.70 ns** |  **4.17 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| SparseLinearDictionary | 100 |       Remove100 |  1,533.3 ns |  1.81 ns |  1.61 ns |  1.11 |    0.00 |         - |          NA |
|                        |     |                 |             |          |          |       |         |           |             |
|             **DoubleList** | **100** | **RandomLookup100** |  **3,507.4 ns** |  **8.33 ns** |  **6.95 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| SparseLinearDictionary | 100 | RandomLookup100 | 12,303.4 ns | 95.34 ns | 84.51 ns |  3.51 |    0.02 |         - |          NA |
|                        |     |                 |             |          |          |       |         |           |             |
|             **DoubleList** | **100** | **RandomUpdate100** |  **8,730.2 ns** | **35.28 ns** | **33.00 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| SparseLinearDictionary | 100 | RandomUpdate100 | 20,247.8 ns | 63.46 ns | 59.36 ns |  2.32 |    0.01 |         - |          NA |
|                        |     |                 |             |          |          |       |         |           |             |
|             **DoubleList** | **100** |         **Process** |    **977.7 ns** |  **3.07 ns** |  **2.57 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| SparseLinearDictionary | 100 |         Process |  1,009.3 ns |  1.80 ns |  1.60 ns |  1.03 |    0.00 |         - |          NA |
|                        |     |                 |             |          |          |       |         |           |             |
|             **DoubleList** | **100** |         **ForEach** | **10,425.1 ns** | **12.91 ns** | **12.07 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| SparseLinearDictionary | 100 |         ForEach |  3,082.3 ns |  3.87 ns |  3.43 ns |  0.30 |    0.00 |         - |          NA |
