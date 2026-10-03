using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class MatchingCommandTests
{
    [Fact]
    public void Matching_Blossom_FindsPairs()
    {
        var file = TestHelpers.TempFile(@"
            graph G {
                A -- B;
                C -- D;
                E -- F;
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "matching", file, "--algorithm", "blossom");

            exit.Should().Be(0);
            stdout.Should().Contain("Паросочетаний: 3");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Matching_Kuhn_FindsPairsWithLeft()
    {
        var file = TestHelpers.TempFile(@"
            graph G {
                A -- X;
                A -- Y;
                B -- X;
                C -- Y;
                C -- Z;
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "matching", file,
                "--algorithm", "kuhn",
                "--left", "A,B,C");

            exit.Should().Be(0);
            stdout.Should().Contain("Паросочетаний:");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
