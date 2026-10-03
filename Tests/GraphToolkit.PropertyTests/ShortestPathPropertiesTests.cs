using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using GraphToolkit.Core;
using GraphToolkit.PropertyTests.Arbitraries;
using GraphToolkit.ShortestPaths;

namespace GraphToolkit.PropertyTests;

public class ShortestPathPropertiesTests
{
    [Property(MaxTest = 50)]
    public Property Dijkstra_MatchesBellmanFord_OnPositiveWeights()
    {
        return Prop.ForAll(GraphArbitraries.ConnectedGraph(), graph =>
        {
            var source = graph.Vertices.First();

            var (dijkstraDist, _) = Dijkstra.Compute(graph, source);
            var (bellmanDist, _, hasNegCycle) = BellmanFord.Compute(graph, source);

            if (hasNegCycle) return true.ToProperty();

            foreach (var v in graph.Vertices)
            {
                double d1 = dijkstraDist[v];
                double d2 = bellmanDist[v];
                bool bothInf = double.IsPositiveInfinity(d1)
                            && double.IsPositiveInfinity(d2);
                bool bothEq = Math.Abs(d1 - d2) < 1e-6;

                if (!bothInf && !bothEq)
                    return false.ToProperty()
                        .Label($"V={v}: Dijkstra={d1}, BF={d2}");
            }
            return true.ToProperty();
        });
    }

    [Property(MaxTest = 30)]
    public Property FloydWarshall_MatchesDijkstra_ForAllPairs()
    {
        return Prop.ForAll(GraphArbitraries.ConnectedGraph(), graph =>
        {
            if (graph.VertexCount > 15) return true.ToProperty();

            var (fwDist, _, vertices) = FloydWarshall.Compute(graph);
            var index = vertices.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);

            foreach (var source in vertices)
            {
                var (diDist, _) = Dijkstra.Compute(graph, source);
                foreach (var target in vertices)
                {
                    double fw = fwDist[index[source], index[target]];
                    double di = diDist[target];

                    bool bothInf = double.IsPositiveInfinity(fw)
                                && double.IsPositiveInfinity(di);
                    bool bothEq = Math.Abs(fw - di) < 1e-6;

                    if (!bothInf && !bothEq)
                        return false.ToProperty()
                            .Label($"[{source}->{target}] FW={fw}, Dijkstra={di}");
                }
            }
            return true.ToProperty();
        });
    }
}
