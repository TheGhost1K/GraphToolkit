using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using GraphToolkit.Core;

namespace GraphToolkit.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class FlowBenchmarks
{
    [Params(50, 100, 200)]
    public int N;

    private FlowNetwork<int> _network = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random(42);
        _network = new FlowNetwork<int>();
        int source = N, sink = N + 1;

        for (int i = 0; i < N; i++)
            _network.AddEdge(source, i, rnd.Next(1, 100));

        for (int i = 0; i < N - 1; i++)
            _network.AddEdge(i, i + 1, rnd.Next(1, 100));

        for (int i = 0; i < N; i++)
            _network.AddEdge(i, sink, rnd.Next(1, 100));
    }

    [Benchmark(Baseline = true)]
    public double Dinic() => Flow.Dinic.Compute(_network.Clone(), N, N + 1);

    [Benchmark]
    public double EdmondsKarp() => Flow.EdmondsKarp.Compute(_network.Clone(), N, N + 1);

    [Benchmark]
    public double FordFulkerson() => Flow.FordFulkerson.Compute(_network.Clone(), N, N + 1);
}
