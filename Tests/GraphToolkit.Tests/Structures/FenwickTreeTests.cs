using FluentAssertions;
using GraphToolkit.Structures;
using Xunit;

namespace GraphToolkit.Tests.Structures;

public class FenwickTreeTests
{
    [Fact]
    public void PrefixSum_ShouldBeCorrect()
    {
        var bit = new FenwickTree<long>(
            new long[] { 1, 2, 3, 4, 5 },
            add: (a, b) => a + b,
            identity: 0,
            subtract: (a, b) => a - b);

        bit.PrefixAggregate(0).Should().Be(0);
        bit.PrefixAggregate(1).Should().Be(1);
        bit.PrefixAggregate(3).Should().Be(6);
        bit.PrefixAggregate(5).Should().Be(15);
    }

    [Fact]
    public void RangeSum_ShouldBeCorrect()
    {
        var bit = new FenwickTree<long>(
            new long[] { 1, 2, 3, 4, 5 },
            add: (a, b) => a + b,
            identity: 0,
            subtract: (a, b) => a - b);

        bit.RangeAggregate(0, 5).Should().Be(15);
        bit.RangeAggregate(1, 4).Should().Be(9);
        bit.RangeAggregate(2, 3).Should().Be(3);
    }

    [Fact]
    public void Add_ShouldUpdateSum()
    {
        var bit = new FenwickTree<long>(
            new long[] { 1, 2, 3, 4, 5 },
            add: (a, b) => a + b,
            identity: 0,
            subtract: (a, b) => a - b);

        bit.Add(2, 10);
        bit.RangeAggregate(0, 5).Should().Be(25);
        bit.RangeAggregate(2, 3).Should().Be(13);
    }

    [Fact]
    public void RangeAggregate_WithoutSubtract_ShouldThrow()
    {
        var bit = new FenwickTree<long>(
            new long[] { 1, 2, 3 },
            add: (a, b) => a + b,
            identity: 0);

        var act = () => bit.RangeAggregate(0, 2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EmptyTree_ShouldWork()
    {
        var bit = new FenwickTree<long>(
            Array.Empty<long>(),
            add: (a, b) => a + b,
            identity: 0,
            subtract: (a, b) => a - b);

        bit.Count.Should().Be(0);
        bit.PrefixAggregate(0).Should().Be(0);
    }

    [Fact]
    public void SingleElement_ShouldWork()
    {
        var bit = new FenwickTree<long>(
            new long[] { 42 },
            add: (a, b) => a + b,
            identity: 0,
            subtract: (a, b) => a - b);

        bit.RangeAggregate(0, 1).Should().Be(42);
    }
}