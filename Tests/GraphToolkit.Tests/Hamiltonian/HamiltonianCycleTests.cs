using FluentAssertions;
using GraphToolkit.Hamiltonian;
using GraphToolkit.Utils;
using Xunit;

namespace GraphToolkit.Tests.Hamiltonian;

public class HamiltonianCycleTests
{
    [Fact]
    public void FindCycle_Complete4_ShouldReturnCycle()
    {
        // K4 — полный граф на 4 вершинах, всегда есть гамильтонов цикл
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(0, 2)
            .AddEdge(0, 3)
            .AddEdge(1, 2)
            .AddEdge(1, 3)
            .AddEdge(2, 3)
            .Build();

        var cycle = HamiltonianCycle.FindCycle(g);

        cycle.Should().NotBeNull();
        cycle!.Should().HaveCount(4);
        cycle.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void FindCycle_StarGraph_ShouldReturnNull()
    {
        // Звезда — нет гамильтонова цикла
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(0, 2)
            .AddEdge(0, 3)
            .Build();

        HamiltonianCycle.FindCycle(g).Should().BeNull();
    }

    [Fact]
    public void FindPath_ShouldReturnPathWithoutReturn()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(1, 2)
            .AddEdge(2, 3)
            .Build();

        var path = HamiltonianCycle.FindPath(g);

        path.Should().NotBeNull();
        path!.Should().HaveCount(4);
    }

    [Fact]
    public void FindPath_Disconnected_ShouldReturnNull()
    {
        var g = new GraphBuilder<int>(isDirected: false)
            .AddEdge(0, 1)
            .AddEdge(2, 3)  // отдельная компонента
            .Build();

        HamiltonianCycle.FindPath(g).Should().BeNull();
    }
}