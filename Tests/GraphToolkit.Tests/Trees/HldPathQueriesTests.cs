using FluentAssertions;
using GraphToolkit.Trees;
using GraphToolkit.Utils;
using GraphToolkit.Core;
using Xunit;

namespace GraphToolkit.Tests.Trees;

public class HldPathQueriesTests
{
    private static (Graph<int> tree, Dictionary<int, long> weights) BuildTest()
    {
        //       1
        //      / \
        //     2   3
        //    / \
        //   4   5
        var tree = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(1, 3)
            .AddEdge(2, 4).AddEdge(2, 5)
            .Build();

        var weights = new Dictionary<int, long>
        {
            [1] = 10,
            [2] = 20,
            [3] = 30,
            [4] = 40,
            [5] = 50
        };

        return (tree, weights);
    }

    [Fact]
    public void Query_Sum_SimplePath_ShouldWork()
    {
        var (tree, weights) = BuildTest();
        var hld = new HldPathQueries<int, long>(
            tree, root: 1,
            valueOf: v => weights[v],
            combine: (a, b) => a + b,
            identity: 0);

        // Путь 4 → 5: 40 + 20 + 50 = 110
        hld.Query(4, 5).Should().Be(110);

        // Путь 4 → 3: 40 + 20 + 10 + 30 = 100
        hld.Query(4, 3).Should().Be(100);

        // Путь 1 → 1: 10
        hld.Query(1, 1).Should().Be(10);
    }

    [Fact]
    public void Query_Min_ShouldReturnMinOnPath()
    {
        var (tree, weights) = BuildTest();
        var hld = new HldPathQueries<int, long>(
            tree, root: 1,
            valueOf: v => weights[v],
            combine: Math.Min,
            identity: long.MaxValue);

        hld.Query(4, 5).Should().Be(20);    // min(40, 20, 50)
        hld.Query(4, 3).Should().Be(10);    // min(40, 20, 10, 30)
    }

    [Fact]
    public void Update_ShouldChangeValue()
    {
        var (tree, weights) = BuildTest();
        var hld = new HldPathQueries<int, long>(
            tree, root: 1,
            valueOf: v => weights[v],
            combine: (a, b) => a + b,
            identity: 0);

        hld.Update(2, 100);

        // Путь 4 → 5: 40 + 100 + 50 = 190
        hld.Query(4, 5).Should().Be(190);

        // Путь 1 → 5: 10 + 100 + 50 = 160
        hld.Query(1, 5).Should().Be(160);
    }

    [Fact]
    public void Query_ChainTree_ShouldWork()
    {
        var tree = new GraphBuilder<int>(isDirected: false)
            .AddEdge(1, 2).AddEdge(2, 3).AddEdge(3, 4).AddEdge(4, 5)
            .Build();

        var weights = new Dictionary<int, long>
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
            [4] = 4,
            [5] = 5
        };

        var hld = new HldPathQueries<int, long>(
            tree, root: 1,
            valueOf: v => weights[v],
            combine: (a, b) => a + b,
            identity: 0);

        hld.Query(1, 5).Should().Be(15);
        hld.Query(2, 4).Should().Be(9);
        hld.Query(3, 3).Should().Be(3);
    }

    [Fact]
    public void Query_LargeTree_ShouldWork()
    {
        // Бинарное дерево на 1000 вершин
        var builder = new GraphBuilder<int>(isDirected: false);
        for (int i = 2; i <= 1000; i++)
            builder.AddEdge(i / 2, i);

        var hld = new HldPathQueries<int, long>(
            builder.Build(), root: 1,
            valueOf: _ => 1,
            combine: (a, b) => a + b,
            identity: 0);

        // Путь между листьями в разных поддеревьях
        long result = hld.Query(1000, 999);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Update_MultipleTimes_ShouldBeConsistent()
    {
        var (tree, weights) = BuildTest();
        var hld = new HldPathQueries<int, long>(
            tree, root: 1,
            valueOf: v => weights[v],
            combine: (a, b) => a + b,
            identity: 0);

        hld.Update(1, 100);
        hld.Update(2, 200);
        hld.Update(3, 300);

        hld.Query(4, 5).Should().Be(40 + 200 + 50);       // 290
        hld.Query(4, 3).Should().Be(40 + 200 + 100 + 300); // 640
    }
}