using FluentAssertions;
using GraphToolkit.Matching;
using Xunit;

namespace GraphToolkit.Tests.Matching;

public class KuhnTests
{
    [Fact]
    public void Compute_PerfectMatching_ShouldReturnAllPairs()
    {
        var left = new[] { "A", "B", "C" };
        var adjacency = new Dictionary<string, string[]>
        {
            ["A"] = new[] { "X", "Y" },
            ["B"] = new[] { "X" },
            ["C"] = new[] { "Y", "Z" }
        };

        var matching = Kuhn.Compute(left, u => adjacency[u]);

        matching.Should().HaveCount(3);
        matching.Keys.Should().BeEquivalentTo(left);
    }

    [Fact]
    public void Compute_PartialMatching_ShouldReturnMaxPossible()
    {
        var left = new[] { "A", "B", "C" };
        // B и C имеют только одного соседа
        var adjacency = new Dictionary<string, string[]>
        {
            ["A"] = new[] { "X" },
            ["B"] = new[] { "X" },
            ["C"] = new[] { "Y" }
        };

        var matching = Kuhn.Compute(left, u => adjacency[u]);

        matching.Should().HaveCount(2);
    }

    [Fact]
    public void Compute_EmptyLeft_ShouldReturnEmpty()
    {
        var matching = Kuhn.Compute(
            Array.Empty<string>(),
            _ => Array.Empty<string>());

        matching.Should().BeEmpty();
    }
}