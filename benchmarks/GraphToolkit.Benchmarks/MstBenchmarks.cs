using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using GraphToolkit.Core;

namespace GraphToolkit.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class MstBenchmarks
{
    [Params(100, 1_000, 10_000)]
    public int N;

    private Graph<int> _sparse = null!;
    private Graph<int> _dense = null!;

    [GlobalSetup]
    public void Setup()
    {
        _sparse = GraphGenerators.DenseUndirected(N);
        _dense = GraphGenerators.DenseUndirected(N);
    }

    [Benchmark(Baseline = true)]
    public int Kruskal() => MinimumSpanningTree.Kruskal.Compute(_sparse).Count;

    [Benchmark]
    public int Prim() => MinimumSpanningTree.Prim.Compute(_sparse, 0).Count;

    [Benchmark]
    public int Boruvka() => MinimumSpanningTree.Boruvka.Compute(_sparse).Count;
}
