using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using GraphToolkit.Components;
using GraphToolkit.MinimumSpanningTree;
using GraphToolkit.PropertyTests.Arbitraries;

namespace GraphToolkit.PropertyTests;

public class MstPropertiesTests
{
    [Property(MaxTest = 100)]
    public Property MST_OnConnectedGraph_HasVMinusOneEdges()
    {
        return Prop.ForAll(GraphArbitraries.ConnectedGraph(), graph =>
        {
            var mst = Kruskal.Compute(graph);
            return (mst.Count == graph.VertexCount - 1).ToProperty()
                .Label($"V={graph.VertexCount}, E={graph.EdgeCount}, MST={mst.Count}");
        });
    }

    [Property(MaxTest = 100)]
    public Property MST_AllThreeAlgorithms_AgreeOnWeight()
    {
        return Prop.ForAll(GraphArbitraries.ConnectedGraph(), graph =>
        {
            if (graph.VertexCount < 2) return true.ToProperty();

            var kruskal = Kruskal.Compute(graph);
            var prim = Prim.Compute(graph, graph.Vertices.First());
            var boruvka = Boruvka.Compute(graph);

            double k = kruskal.Sum(e => e.Weight);
            double p = prim.Sum(e => e.Weight);
            double b = boruvka.Sum(e => e.Weight);

            if (Math.Abs(k - p) > 1e-6 || Math.Abs(k - b) > 1e-6)
            {
                // Собираем информацию о графе
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"V={graph.VertexCount}, E={graph.EdgeCount}");
                sb.AppendLine($"Kruskal edges={kruskal.Count}, weight={k}");
                sb.AppendLine($"Prim edges={prim.Count}, weight={p}");
                sb.AppendLine($"Boruvka edges={boruvka.Count}, weight={b}");
                sb.AppendLine($"Components={ConnectedComponents.Find(graph).Count}");

                return false.ToProperty().Label(sb.ToString());
            }

            return true.ToProperty();
        });
    }

    [Property(MaxTest = 100)]
    public Property MST_IsSubsetOfOriginalEdges()
    {
        return Prop.ForAll(GraphArbitraries.ConnectedGraph(), graph =>
        {
            var mst = Kruskal.Compute(graph);
            var original = graph.Edges
                .Select(e => (Math.Min(e.From, e.To), Math.Max(e.From, e.To)))
                .ToHashSet();

            foreach (var e in mst)
            {
                var key = (Math.Min(e.From, e.To), Math.Max(e.From, e.To));
                if (!original.Contains(key))
                    return false.ToProperty()
                        .Label($"MST edge {e} not in original graph");
            }
            return true.ToProperty();
        });
    }
}
