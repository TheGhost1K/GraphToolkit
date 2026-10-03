using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using GraphToolkit.Core;
using GraphToolkit.ShortestPaths;

namespace GraphToolkit.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class ShortestPathsBenchmarks
{
    [Params(100, 1_000, 10_000)]
    public int N;

    private Graph<int> _sparse = null!;

    [GlobalSetup]
    public void Setup()
    {
        _sparse = GraphGenerators.SparseDirected(N);
    }

    [Benchmark(Baseline = true)]
    public int Dijkstra() => ShortestPaths.Dijkstra.Compute(_sparse, 0).Distances.Count;

    [Benchmark]
    public int BellmanFord() => ShortestPaths.BellmanFord.Compute(_sparse, 0).Distances.Count;

    [Benchmark]
    [Arguments(50)]
    [Arguments(100)]
    public int FloydWarshall_SmallOnly(int size)
    {
        var g = GraphGenerators.SparseDirected(size);
        return FloydWarshall.Compute(g).Distances.GetLength(0);
    }
}
