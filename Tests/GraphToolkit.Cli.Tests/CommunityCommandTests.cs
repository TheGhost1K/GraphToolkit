using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class CommunityCommandTests
{
    [Fact]
    public void Community_Louvain_FindsCommunities()
    {
        var file = TestHelpers.TempFile(@"
            graph G {
                A -- B [label=""1""];
                B -- C [label=""1""];
                A -- C [label=""1""];
                D -- E [label=""1""];
                E -- F [label=""1""];
                D -- F [label=""1""];
                C -- D [label=""0.1""];
            }
        ");
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "community", file, "--algorithm", "louvain");

            exit.Should().Be(0);
            stdout.Should().Contain("Найдено сообществ");
            stdout.Should().Contain("Модулярность");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Community_LabelPropagation_Works()
    {
        var file = TestHelpers.UndirectedDotFile();
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "community", file,
                "--algorithm", "label-propagation",
                "--seed", "42");

            exit.Should().Be(0);
            stdout.Should().Contain("Найдено сообществ");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
