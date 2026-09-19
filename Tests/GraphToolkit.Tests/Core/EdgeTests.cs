using FluentAssertions;
using GraphToolkit.Core;
using Xunit;

namespace GraphToolkit.Tests.Core;

public class EdgeTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var e = new Edge<string>("A", "B", 3.5);

        e.From.Should().Be("A");
        e.To.Should().Be("B");
        e.Weight.Should().Be(3.5);
    }

    [Fact]
    public void DefaultWeight_ShouldBeOne()
    {
        var e = new Edge<int>(1, 2);
        e.Weight.Should().Be(1.0);
    }

    [Fact]
    public void CompareTo_ShouldOrderByWeight()
    {
        var e1 = new Edge<string>("A", "B", 1);
        var e2 = new Edge<string>("C", "D", 5);

        e1.CompareTo(e2).Should().BeNegative();
        e2.CompareTo(e1).Should().BePositive();
    }

    [Fact]
    public void Equals_SameValues_ShouldBeTrue()
    {
        var e1 = new Edge<string>("A", "B", 3);
        var e2 = new Edge<string>("A", "B", 3);

        e1.Equals(e2).Should().BeTrue();
        e1.GetHashCode().Should().Be(e2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValues_ShouldBeFalse()
    {
        var e1 = new Edge<string>("A", "B", 3);
        var e2 = new Edge<string>("A", "B", 4);

        e1.Equals(e2).Should().BeFalse();
    }

    [Fact]
    public void Constructor_NullFrom_ShouldThrow()
    {
        var act = () => new Edge<string>(null!, "B");
        act.Should().Throw<ArgumentNullException>();
    }
}