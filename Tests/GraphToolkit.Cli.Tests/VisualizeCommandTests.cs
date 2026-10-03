using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class VisualizeCommandTests
{
    [Theory]
    [InlineData("dot", "digraph")]
    [InlineData("mermaid", "graph")]
    [InlineData("matrix", "A")]
    [InlineData("list", "A:")]
    public void Visualize_EachFormat_ProducesOutput(
        string format, string expectedSubstring)
    {
        var file = TestHelpers.SimpleDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "visualize", file, "--format", format);

            exit.Should().Be(0);
            stdout.Should().Contain(expectedSubstring);
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
