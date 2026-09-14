```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7623/24H2/2024Update/HudsonValley)
13th Gen Intel Core i9-13980HX 2.20GHz, 1 CPU, 32 logical and 24 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 9.0.12 (9.0.12, 9.0.1225.60609), X64 RyuJIT x86-64-v3
  Job-CNUJVU : .NET 9.0.12 (9.0.12, 9.0.1225.60609), X64 RyuJIT x86-64-v3

InvocationCount=1  UnrollFactor=1  

```
| Method                   | EntityCount | Mean      | Error     | StdDev    | Ratio | RatioSD | Rank | Gen0      | Allocated   | Alloc Ratio |
|------------------------- |------------ |----------:|----------:|----------:|------:|--------:|-----:|----------:|------------:|------------:|
| InMemory_MixedOperations | 50          |  6.263 ms | 0.5843 ms |  1.723 ms |  1.09 |    0.46 |    1 |         - |   639.13 KB |        1.00 |
| Markdown_MixedOperations | 50          | 13.917 ms | 0.7340 ms |  2.106 ms |  2.42 |    0.84 |    2 |         - |   5077.3 KB |        7.94 |
| Sqlite_MixedOperations   | 50          | 66.457 ms | 5.4902 ms | 16.015 ms | 11.54 |    4.58 |    3 | 1000.0000 | 26972.26 KB |       42.20 |
