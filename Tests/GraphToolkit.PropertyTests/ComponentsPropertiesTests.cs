using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using GraphToolkit.Components;
using GraphToolkit.Core;
using GraphToolkit.PropertyTests.Arbitraries;

namespace GraphToolkit.PropertyTests;

public class ComponentsPropertiesTests
{
    [Property(MaxTest = 50)]
    public Property ConnectedComponents_PartitionAllVertices()
    {
        return Prop.ForAll(GraphArbitraries.UndirectedGraph(), graph =>
        {
            var components = ConnectedComponents.Find(graph);
            int total = components.Sum(c => c.Count);

            return (total == graph.VertexCount).ToProperty()
                .Label($"{total} of {graph.VertexCount}");
        });
    }

    [Property(MaxTest = 50)]
    public Property Scc_PartitionsAllVertices()
    {
        return Prop.ForAll(GraphArbitraries.DirectedGraph(), graph =>
        {
            var sccs = StronglyConnectedComponents.Find(graph);
            int total = sccs.Sum(c => c.Count);

            return (total == graph.VertexCount).ToProperty();
        });
    }
}
