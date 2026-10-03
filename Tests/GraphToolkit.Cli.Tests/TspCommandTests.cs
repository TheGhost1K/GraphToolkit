using FluentAssertions;
using GraphToolkit.Cli;
using Xunit;

namespace GraphToolkit.Cli.Tests;

public class TspCommandTests
{
    private const string TspFile = @"
        graph G {
            A -- B [label=""10""];
            A -- C [label=""15""];
            A -- D [label=""20""];
            B -- C [label=""35""];
            B -- D [label=""25""];
            C -- D [label=""30""];
        }
    ";

    [Fact]
    public void Tsp_Exact_FindsOptimalTour()
    {
        var file = TestHelpers.TempFile(TspFile);
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "tsp", file,
                "--algorithm", "tsp-exact", "--start", "A");

            exit.Should().Be(0);
            stdout.Should().Contain("Оптимальный маршрут:");
            stdout.Should().Contain("Стоимость:");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Tsp_Approx_FindsTour()
    {
        var file = TestHelpers.TempFile(TspFile);
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, stdout, _) = TestHelpers.Run(
                root, "tsp", file,
                "--algorithm", "tsp-approx", "--start", "A");

            exit.Should().Be(0);
            stdout.Should().Contain("Приближённый маршрут:");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }

    [Fact]
    public void Tsp_ExactWithoutStart_ReturnsError()
    {
        var file = TestHelpers.TempFile(TspFile);
        try
        {
            var root = Program.BuildRootCommand();
            var (exit, _, stderr) = TestHelpers.Run(
                root, "tsp", file, "--algorithm", "tsp-exact");

            exit.Should().NotBe(0);
            stderr.Should().Contain("--start");
        }
        finally
        {
            TestHelpers.SafeDelete(file);
        }
    }
}
