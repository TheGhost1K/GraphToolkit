using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class ComponentsCommandTests
{
    [Fact]
    public void Components_Connected_CountsComponents()
    {
        var file = TestHelpers.TempFile(@"
            graph G {
                A -- B;
                C -- D;
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(root, "components", file);

            exit.Should().Be(0);
            stdout.Should().Contain("Связных компонент");
            stdout.Should().Contain("2");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Components_Bridges_FindsBridges()
    {
        var file = TestHelpers.TempFile(@"
            graph G {
                A -- B;
                B -- C;
                C -- A;
                C -- D;
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "components", file, "--type", "bridges");

            exit.Should().Be(0);
            stdout.Should().Contain("Мостов:");
            stdout.Should().Contain("C");
            stdout.Should().Contain("D");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
