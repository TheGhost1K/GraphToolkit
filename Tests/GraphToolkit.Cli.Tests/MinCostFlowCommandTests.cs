using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class MinCostFlowCommandTests
{
    [Theory]
    [InlineData("dijkstra")]
    [InlineData("spfa")]
    public void MinCostFlow_EachAlgorithm_ComputesResult(string algo)
    {
        var file = TestHelpers.TempFile(@"
            digraph G {
                S -> A [label=""3""];
                A -> T [label=""2""];
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "min-cost-flow", file,
                "--source", "S", "--sink", "T",
                "--algorithm", algo);

            exit.Should().Be(0);
            stdout.Should().Contain("Поток:");
            stdout.Should().Contain("Стоимость:");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
