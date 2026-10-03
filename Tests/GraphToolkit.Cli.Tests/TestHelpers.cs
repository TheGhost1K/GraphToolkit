using System.CommandLine;
using System.IO;

namespace GraphToolkit.Cli.Tests;

internal static class TestHelpers
{
    public static (int ExitCode, string StdOut, string StdErr) Run(
        RootCommand root, params string[] args)
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var config = new InvocationConfiguration
        {
            Output = stdout,
            Error = stderr
        };

        var parseResult = root.Parse(args);
        int exitCode = parseResult.Invoke(config);

        return (exitCode, stdout.ToString(), stderr.ToString());
    }

    public static string TempFile(string content, string extension = ".dot")
    {
        var path = Path.Combine(Path.GetTempPath(),
            $"gt-test-{Guid.NewGuid():N}{extension}");
        File.WriteAllText(path, content);
        return path;
    }

    public static string SimpleDotFile() => TempFile(@"
        digraph G {
            A -> B [label=""1""];
            B -> C [label=""2""];
            A -> C [label=""4""];
        }
    ");

    public static string UndirectedDotFile() => TempFile(@"
        graph G {
            A -- B [label=""1""];
            B -- C [label=""2""];
            C -- D [label=""3""];
            A -- C [label=""5""];
        }
    ");

    public static string TriangleDotFile() => TempFile(@"
        graph G {
            A -- B [label=""1""];
            B -- C [label=""1""];
            C -- A [label=""1""];
        }
    ");

    public static void SafeDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* ignore */ }
    }
}
