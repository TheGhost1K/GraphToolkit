using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using GraphToolkit.Core;
using GraphToolkit.PropertyTests.Arbitraries;
using GraphToolkit.Traversal;

namespace GraphToolkit.PropertyTests;

public class TraversalPropertiesTests
{
    [Property(MaxTest = 50)]
    public Property TopologicalSort_RespectsAllEdges()
    {
        return Prop.ForAll(GraphArbitraries.Dag(), dag =>
        {
            var order = TopologicalSort.Sort(dag);
            if (order is null) return true.ToProperty();

            var position = order.Select((v, i) => (v, i))
                .ToDictionary(x => x.v, x => x.i);

            foreach (var e in dag.Edges)
            {
                if (position[e.From] >= position[e.To])
                    return false.ToProperty()
                        .Label($"Edge {e.From}->{e.To} violates order");
            }
            return true.ToProperty();
        });
    }

    [Property(MaxTest = 50)]
    public Property Bfs_ReachesSameSetAsDfs()
    {
        return Prop.ForAll(GraphArbitraries.ConnectedGraph(), graph =>
        {
            var source = graph.Vertices.First();
            var reachable = new HashSet<int> { source };
            var queue = new Queue<int>();
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                foreach (var e in graph.Neighbors(v))
                    if (reachable.Add(e.To))
                        queue.Enqueue(e.To);
            }

            foreach (var v in graph.Vertices)
            {
                var path = Dfs.FindPath(graph, source, v);
                bool dfsReachable = path is not null;
                bool bfsReachable = reachable.Contains(v);

                if (dfsReachable != bfsReachable)
                    return false.ToProperty()
                        .Label($"{v}: BFS={bfsReachable}, DFS={dfsReachable}");
            }
            return true.ToProperty();
        });
    }
}
