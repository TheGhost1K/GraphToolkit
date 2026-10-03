using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class InfoCommandTests
{
    [Fact]
    public void Info_SimpleGraph_ShowsCorrectInfo()
    {
        var file = TestHelpers.SimpleDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(root, "info", file);

            exit.Should().Be(0);
            stdout.Should().Contain("Вершин:");
            stdout.Should().Contain("3");
            stdout.Should().Contain("Рёбер:");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Info_MissingFile_ReturnsError()
    {
        var root = Program.BuildRootCommand();
        var (exit, _, stderr) = TestHelpers.Run(
            root, "info", "missing-file.dot");

        exit.Should().NotBe(0);
        stderr.Should().Contain("не найден");
    }
}
