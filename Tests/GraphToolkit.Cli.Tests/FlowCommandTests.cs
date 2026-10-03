using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class FlowCommandTests
{
    private const string FlowFile = @"
        digraph G {
            S -> A [label=""10""];
            A -> T [label=""5""];
        }
    ";

    [Theory]
    [InlineData("dinic")]
    [InlineData("edmonds-karp")]
    [InlineData("ford-fulkerson")]
    public void Flow_EachAlgorithm_ComputesMaxFlow(string algo)
    {
        var file = TestHelpers.TempFile(FlowFile);
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "flow", file,
                "--source", "S",
                "--sink", "T",
                "--algorithm", algo);

            exit.Should().Be(0);
            stdout.Should().Contain("Максимальный поток:");
            stdout.Should().Contain("5");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Flow_WithMinCut_ShowsSourceSide()
    {
        var file = TestHelpers.TempFile(FlowFile);
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "flow", file,
                "--source", "S", "--sink", "T",
                "--min-cut");

            exit.Should().Be(0);
            stdout.Should().Contain("Минимальный разрез");
            stdout.Should().Contain("S");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
