using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class CutCommandTests
{
    [Fact]
    public void Cut_StoerWagner_FindsMinCut()
    {
        // Два треугольника, соединённые мостом
        var file = TestHelpers.TempFile(@"
            graph G {
                A -- B [label=""10""];
                B -- C [label=""10""];
                C -- A [label=""10""];
                C -- D [label=""1""];
                D -- E [label=""10""];
                E -- F [label=""10""];
                F -- D [label=""10""];
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "cut", file, "--type", "stoer-wagner");

            exit.Should().Be(0);
            stdout.Should().Contain("Min-cut:");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Cut_GomoryHu_FindsPairwiseMinCut()
    {
        var file = TestHelpers.TempFile(@"
            graph G {
                A -- B [label=""1""];
                B -- C [label=""1""];
                A -- C [label=""3""];
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "cut", file,
                "--type", "gomory-hu",
                "--from", "A", "--to", "C");

            exit.Should().Be(0);
            stdout.Should().Contain("Min-cut(A, C)");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
