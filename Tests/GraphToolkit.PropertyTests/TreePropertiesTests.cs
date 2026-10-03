using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using GraphToolkit.Components;
using GraphToolkit.Core;
using GraphToolkit.PropertyTests.Arbitraries;
using GraphToolkit.Trees;

namespace GraphToolkit.PropertyTests;

public class TreePropertiesTests
{
    [Property(MaxTest = 30)]
    public Property Hld_Lca_MatchesBinaryLifting()
    {
        return Prop.ForAll(GraphArbitraries.Tree(), tree =>
        {
            if (tree.VertexCount < 2) return true.ToProperty();
            if (tree.EdgeCount != tree.VertexCount - 1) return true.ToProperty();
            if (ConnectedComponents.Find(tree).Count != 1) return true.ToProperty();

            var root = tree.Vertices.First();
            var hld = new HeavyLightDecomposition<int>(tree, root);
            var lca = new Lca<int>(tree, root);

            var vertices = tree.Vertices.ToList();
            for (int i = 0; i < Math.Min(10, vertices.Count); i++)
            {
                for (int j = 0; j < Math.Min(10, vertices.Count); j++)
                {
                    var u = vertices[i];
                    var v = vertices[j];

                    var hldLca = hld.Lca(u, v);
                    var binaryLca = lca.Query(u, v);

                    if (!EqualityComparer<int>.Default.Equals(hldLca, binaryLca))
                        return false.ToProperty()
                            .Label($"LCA({u},{v}): HLD={hldLca}, Binary={binaryLca}");
                }
            }
            return true.ToProperty();
        });
    }

    [Property(MaxTest = 30)]
    public Property Diameter_IsNonNegative()
    {
        return Prop.ForAll(GraphArbitraries.Tree(), tree =>
        {
            if (tree.VertexCount < 2) return true.ToProperty();

            var diameter = TreeMetrics.Diameter(tree);

            return (diameter.Distance >= 0
                    && diameter.Path.Count >= 1).ToProperty()
                .Label($"Diameter = {diameter.Distance}, path = {diameter.Path.Count}");
        });
    }

    [Property(MaxTest = 30)]
    public Property Centroid_RemovalLeavesSmallComponents()
    {
        return Prop.ForAll(GraphArbitraries.Tree(), tree =>
        {
            if (tree.VertexCount < 2) return true.ToProperty();

            // Граф должен быть деревом
            if (tree.EdgeCount != tree.VertexCount - 1) return true.ToProperty();
            if (ConnectedComponents.Find(tree).Count != 1) return true.ToProperty();

            var centroid = TreeMetrics.Centroid(tree);
            int n = tree.VertexCount;

            // Удаляем центроид и считаем компоненты
            var remaining = tree.Vertices
                .Where(v => !EqualityComparer<int>.Default.Equals(v, centroid))
                .ToHashSet();

            var reduced = new Graph<int>(isDirected: false);
            foreach (var v in remaining) reduced.AddVertex(v);
            foreach (var e in tree.Edges)
                if (remaining.Contains(e.From) && remaining.Contains(e.To))
                    reduced.AddEdge(e.From, e.To, e.Weight);

            var components = ConnectedComponents.Find(reduced);
            int maxComponent = components.Count > 0
                ? components.Max(c => c.Count)
                : 0;

            return (maxComponent <= n / 2).ToProperty()
                .Label($"Centroid={centroid}, max={maxComponent}, n/2={n / 2}");
        });
    }
}
