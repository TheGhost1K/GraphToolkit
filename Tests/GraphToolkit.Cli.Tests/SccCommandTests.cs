using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class SccCommandTests
{
    [Fact]
    public void Scc_TwoCycles_FindsTwoSccs()
    {
        var file = TestHelpers.TempFile(@"
            digraph G {
                A -> B;
                B -> C;
                C -> A;
                C -> D;
                D -> E;
                E -> F;
                F -> D;
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(root, "scc", file);

            exit.Should().Be(0);
            stdout.Should().Contain("SCC: 2");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Scc_WithComponents_ShowsBoth()
    {
        var file = TestHelpers.TempFile(@"
            digraph G {
                A -> B;
                B -> C;
                C -> A;
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "scc", file, "--components");

            exit.Should().Be(0);
            stdout.Should().Contain("SCC:");
            stdout.Should().Contain("Связные компоненты:");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
