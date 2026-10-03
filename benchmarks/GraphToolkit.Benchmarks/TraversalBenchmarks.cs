using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using GraphToolkit.Core;
using GraphToolkit.Traversal;

namespace GraphToolkit.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MarkdownExporter]
public class TraversalBenchmarks
{
    [Params(100, 1_000, 10_000)]
    public int N;

    private Graph<int> _sparse = null!;
    private Graph<int> _path = null!;

    [GlobalSetup]
    public void Setup()
    {
        _sparse = GraphGenerators.SparseDirected(N);
        _path = GraphGenerators.Path(N);
    }

    [Benchmark]
    [BenchmarkCategory("BFS")]
    public int Bfs_Sparse() => Bfs.FindPath(_sparse, 0, N - 1)?.Count ?? -1;

    [Benchmark]
    [BenchmarkCategory("DFS")]
    public int Dfs_Sparse() => Dfs.FindPath(_sparse, 0, N - 1)?.Count ?? -1;

    [Benchmark]
    [BenchmarkCategory("BFS")]
    public int Bfs_Path() => Bfs.FindPath(_path, 0, N - 1)?.Count ?? -1;

    [Benchmark]
    [BenchmarkCategory("DFS")]
    public int Dfs_Path_Iterative() => Dfs.FindPath(_path, 0, N - 1)?.Count ?? -1;
}
