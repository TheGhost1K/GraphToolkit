using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class ConvertCommandTests
{
    [Theory]
    [InlineData("json")]
    [InlineData("graphml")]
    [InlineData("gexf")]
    [InlineData("csv")]
    [InlineData("dot")]
    public void Convert_ToEachFormat_CreatesFile(string format)
    {
        var file = TestHelpers.SimpleDotFile();
        var output = Path.Combine(
            Path.GetTempPath(), $"gt-conv-{Guid.NewGuid():N}.{format}");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, _, _) = TestHelpers.Run(
                root, "convert", file, "--to", format, "--output", output);

            exit.Should().Be(0);
            File.Exists(output).Should().BeTrue(
                $"файл {format} должен быть создан");
            new FileInfo(output).Length.Should().BeGreaterThan(0);
        }
        finally
        {
            TestHelpers.SafeDelete(file);
            TestHelpers.SafeDelete(output);
        }
    }

    [Fact]
    public void Convert_UnknownFormat_ReturnsError()
    {
        var file = TestHelpers.SimpleDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, _, _) = TestHelpers.Run(
                root, "convert", file, "--to", "unknown-format");

            exit.Should().NotBe(0);
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
