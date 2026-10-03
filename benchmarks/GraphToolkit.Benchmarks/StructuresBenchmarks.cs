using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using GraphToolkit.Structures;

namespace GraphToolkit.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class StructuresBenchmarks
{
    [Params(1_000, 10_000, 100_000)]
    public int N;

    private long[] _data = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random(42);
        _data = new long[N];
        for (int i = 0; i < N; i++) _data[i] = rnd.Next(1_000);
    }

    [Benchmark(Baseline = true)]
    public long SegmentTree_BuildAndQuery()
    {
        var st = new SegmentTree<long>(
            _data,
            combine: (a, b) => a + b,
            identity: 0);
        long sum = 0;
        for (int i = 0; i < 100; i++)
            sum += st.Query(i, N - i);
        return sum;
    }

    [Benchmark]
    public long Fenwick_BuildAndQuery()
    {
        var bit = new FenwickTree<long>(
            _data,
            add: (a, b) => a + b,
            identity: 0,
            subtract: (a, b) => a - b);
        long sum = 0;
        for (int i = 0; i < 100; i++)
            sum += bit.RangeAggregate(i, N - i);
        return sum;
    }
}
