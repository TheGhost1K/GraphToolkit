using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class CentralityCommandTests
{
    [Theory]
    [InlineData("pagerank")]
    [InlineData("degree")]
    [InlineData("closeness")]
    [InlineData("betweenness")]
    public void Centrality_EachMetric_ProducesOutput(string metric)
    {
        var file = TestHelpers.UndirectedDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "centrality", file, "--metric", metric);

            exit.Should().Be(0);
            stdout.Should().NotBeNullOrEmpty();
            stdout.Should().Contain(metric);
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Centrality_UnknownMetric_ReturnsError()
    {
        var file = TestHelpers.UndirectedDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, _, _) = TestHelpers.Run(
                root, "centrality", file, "--metric", "unknown");

            exit.Should().NotBe(0);
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
