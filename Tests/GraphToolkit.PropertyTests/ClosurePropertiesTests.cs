using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using GraphToolkit.Closure;
using GraphToolkit.PropertyTests.Arbitraries;

namespace GraphToolkit.PropertyTests;

public class ClosurePropertiesTests
{
    [Property(MaxTest = 30)]
    public Property Closure_IsReflexive()
    {
        return Prop.ForAll(GraphArbitraries.DirectedGraph(), graph =>
        {
            if (graph.VertexCount > 15) return true.ToProperty();

            var reach = TransitiveClosure.Compute(graph);
            var vertices = graph.Vertices.ToList();

            for (int i = 0; i < vertices.Count; i++)
                if (!reach[i, i])
                    return false.ToProperty()
                        .Label($"Vertex {vertices[i]} not reachable from itself");

            return true.ToProperty();
        });
    }

    [Property(MaxTest = 30)]
    public Property Closure_IsTransitive()
    {
        return Prop.ForAll(GraphArbitraries.DirectedGraph(), graph =>
        {
            if (graph.VertexCount > 15) return true.ToProperty();

            var reach = TransitiveClosure.Compute(graph);
            int n = graph.VertexCount;

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < n; k++)
                        if (reach[i, j] && reach[j, k] && !reach[i, k])
                            return false.ToProperty()
                                .Label($"Transitivity violated {i}->{j}->{k}");

            return true.ToProperty();
        });
    }
}
