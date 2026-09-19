using FluentAssertions;
using GraphToolkit.Core;
using Xunit;

namespace GraphToolkit.Tests.Core;

public class UnionFindTests
{
    [Fact]
    public void Find_SingleElement_ShouldReturnSelf()
    {
        var uf = new UnionFind<int>();
        uf.MakeSet(1);

        uf.Find(1).Should().Be(1);
    }

    [Fact]
    public void Union_TwoSets_ShouldMerge()
    {
        var uf = new UnionFind<int>();
        uf.MakeSet(1); uf.MakeSet(2);

        uf.Union(1, 2).Should().BeTrue();
        uf.Find(1).Should().Be(uf.Find(2));
    }

    [Fact]
    public void Union_AlreadyConnected_ShouldReturnFalse()
    {
        var uf = new UnionFind<int>();
        uf.MakeSet(1); uf.MakeSet(2); uf.MakeSet(3);
        uf.Union(1, 2);
        uf.Union(2, 3);

        uf.Union(1, 3).Should().BeFalse();
    }

    [Fact]
    public void Union_ManyElements_ShouldFormOneSet()
    {
        var uf = new UnionFind<int>();
        for (int i = 0; i < 100; i++) uf.MakeSet(i);

        for (int i = 1; i < 100; i++) uf.Union(0, i);

        for (int i = 0; i < 100; i++)
            uf.Find(i).Should().Be(uf.Find(0));
    }
}