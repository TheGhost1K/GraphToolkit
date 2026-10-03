using System.CommandLine;
using GraphToolkit.Cli.Commands;

namespace GraphToolkit.Cli;

/// <summary>
/// Точка входа CLI-утилиты <c>graph-toolkit</c>.
/// </summary>
/// <remarks>
/// Построена на System.CommandLine 2.0 GA.
/// </remarks>
public static class Program
{
    public static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var root = BuildRootCommand();
        return root.Parse(args).Invoke();
    }

    /// <summary>
    /// Собирает корневую команду со всеми подкомандами.
    /// Вынесено отдельно для тестируемости.
    /// </summary>
    public static RootCommand BuildRootCommand()
    {
        var root = new RootCommand(
            "graph-toolkit — работа с графами из командной строки. " +
            "Поддерживает форматы: DOT, GraphML, GEXF, JSON, CSV.");

        // ---------- Основные ----------
        root.Subcommands.Add(InfoCommand.Create());
        root.Subcommands.Add(ConvertCommand.Create());
        root.Subcommands.Add(VisualizeCommand.Create());

        // ---------- Пути и обходы ----------
        root.Subcommands.Add(PathCommand.Create());
        root.Subcommands.Add(TraversalCommand.Create());

        // ---------- Структура ----------
        root.Subcommands.Add(MstCommand.Create());
        root.Subcommands.Add(ComponentsCommand.Create());
        root.Subcommands.Add(SccCommand.Create());
        root.Subcommands.Add(EulerianCommand.Create());

        // ---------- Потоки и разрезы ----------
        root.Subcommands.Add(FlowCommand.Create());
        root.Subcommands.Add(MinCostFlowCommand.Create());
        root.Subcommands.Add(CutCommand.Create());

        // ---------- Специальные ----------
        root.Subcommands.Add(MatchingCommand.Create());
        root.Subcommands.Add(ColoringCommand.Create());
        root.Subcommands.Add(TspCommand.Create());

        // ---------- Анализ ----------
        root.Subcommands.Add(CentralityCommand.Create());
        root.Subcommands.Add(CommunityCommand.Create());

        return root;
    }
}
