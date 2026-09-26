using FluentAssertions;
using GraphToolkit.Matching;
using Xunit;

namespace GraphToolkit.Tests.Matching;

public class HungarianAlgorithmTests
{
    [Fact]
    public void Solve_Simple3x3_ShouldFindOptimal()
    {
        var cost = new double[,]
        {
            { 10, 5, 13 },
            { 3, 7, 9 },
            { 6, 8, 4 }
        };

        var result = HungarianAlgorithm.Solve(cost);

        // Оптимум: 10 (0,0) + 3 (1,... нет)
        // Переберём: 
        // 0→0 (10) + 1→1 (7) + 2→2 (4) = 21
        // 0→1 (5) + 1→0 (3) + 2→2 (4) = 12
        // 0→1 (5) + 1→2 (9) + 2→0 (6) = 20
        // 0→0 (10) + 1→2 (9) + 2→1 (8) = 27
        // 0→2 (13) + 1→0 (3) + 2→1 (8) = 24
        // 0→2 (13) + 1→1 (7) + 2→0 (6) = 26
        // Минимум: 12 (0→1, 1→0, 2→2)
        result.TotalCost.Should().Be(12);
        result.Assignments.Should().HaveCount(3);
    }

    [Fact]
    public void Solve_IdentityMatrix_ShouldPickDiagonal()
    {
        var cost = new double[,]
        {
            { 1, 100, 100 },
            { 100, 1, 100 },
            { 100, 100, 1 }
        };

        var result = HungarianAlgorithm.Solve(cost);

        result.TotalCost.Should().Be(3);
        result.Assignments.Should().BeEquivalentTo(new[]
        {
            (0, 0), (1, 1), (2, 2)
        });
    }

    [Fact]
    public void Solve_1x1_ShouldReturnThatCell()
    {
        var cost = new double[,] { { 42.5 } };

        var result = HungarianAlgorithm.Solve(cost);

        result.TotalCost.Should().Be(42.5);
        result.Assignments.Should().Equal((0, 0));
    }

    [Fact]
    public void Solve_EmptyMatrix_ShouldReturnEmpty()
    {
        var cost = new double[0, 0];

        var result = HungarianAlgorithm.Solve(cost);

        result.Assignments.Should().BeEmpty();
        result.TotalCost.Should().Be(0);
    }

    [Fact]
    public void Solve_NegativeValues_ShouldWork()
    {
        var cost = new double[,]
        {
            { -5, -1 },
            { -2, -8 }
        };

        var result = HungarianAlgorithm.Solve(cost);

        // Оптимум: -5 + -8 = -13
        result.TotalCost.Should().Be(-13);
    }

    [Fact]
    public void Solve_Maximization_ShouldFindMaxProfit()
    {
        var profit = new double[,]
        {
            { 10, 5, 13 },
            { 3, 7, 9 },
            { 6, 8, 4 }
        };

        var result = HungarianAlgorithm.SolveMaximization(profit);

        // Максимум: 10 + 9 + 8 = 27 (0→0, 1→2, 2→1)
        result.TotalCost.Should().Be(27);
    }

    [Fact]
    public void Solve_RectangularMatrix_ShouldWork()
    {
        // 2 задачи, 3 исполнителя
        var cost = new double[,]
        {
            { 5, 2, 8 },
            { 3, 7, 1 }
        };

        var result = HungarianAlgorithm.SolveRectangular(cost);

        result.Assignments.Should().HaveCount(2);
        // Оптимум: 0→1 (2), 1→2 (1) = 3
        result.TotalCost.Should().Be(3);
    }

    [Fact]
    public void Solve_NonSquareMatrix_ShouldThrow()
    {
        var cost = new double[2, 3];

        var act = () => HungarianAlgorithm.Solve(cost);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Solve_NullMatrix_ShouldThrow()
    {
        var act = () => HungarianAlgorithm.Solve(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Solve_RealWorldExample_ShouldFindMinimum()
    {
        // 4 работника, 4 задачи.
        // Стоимость = зарплата (тыс. руб).
        var cost = new double[,]
        {
            { 9, 2, 7, 8 },   // Иванов
            { 6, 4, 3, 7 },   // Петров
            { 5, 8, 1, 8 },   // Сидоров
            { 7, 6, 9, 4 }    // Кузнецов
        };

        var result = HungarianAlgorithm.Solve(cost);

        result.Assignments.Should().HaveCount(4);
        result.TotalCost.Should().Be(13);   // 2 + 3 + 1 + 7 (проверить можно вручную)
    }
}