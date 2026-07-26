using System.Text;
using PokerUltimateCalc.Models;

namespace PokerUltimateCalc.UI;

/// <summary>
/// Handles terminal presentation, ANSI box rendering, card formatting, and output coloring.
/// </summary>
public class ConsoleRenderer
{
    private const int BoxWidth = 62;

    public void InitializeConsole()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Clear();
    }

    public void RenderHeader()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║               TEXAS HOLD'EM SIMULATOR & CALC                 ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    public int PromptOpponentCount()
    {
        Console.Write("  Enter number of opponents (1-9): ");
        string input = Console.ReadLine() ?? "1";
        if (!int.TryParse(input, out int numOpponents)) numOpponents = 1;
        return Math.Clamp(numOpponents, 1, 9);
    }

    public void RenderStageBox(string stage, int oppCount, ReadOnlySpan<Card> playerHand, ReadOnlySpan<Card> board)
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║ STAGE: {stage,-35} Opponents: {oppCount,-4} ║");

        Console.Write("║ Your Hand: ");
        foreach (var card in playerHand) RenderCard(card);
        Console.WriteLine(new string(' ', BoxWidth - 11 - (playerHand.Length * 3)) + "║");

        Console.Write("║ Board:     ");
        if (board.Length == 0)
        {
            string waitText = "[Waiting...]";
            Console.Write(waitText);
            Console.WriteLine(new string(' ', BoxWidth - 11 - waitText.Length) + "║");
        }
        else
        {
            foreach (var card in board) RenderCard(card);
            Console.WriteLine(new string(' ', BoxWidth - 11 - (board.Length * 3)) + "║");
        }
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    }

    public void RenderCard(Card card)
    {
        Console.ForegroundColor = card.IsRed ? ConsoleColor.Red : ConsoleColor.Cyan;
        Console.Write($"{card} ");
        Console.ResetColor();
    }

    public void RenderEquityResults(double winPct, double tiePct, int oppCount, string currentBestDescription)
    {
        Console.Write("  WIN CHANCE: ");
        Console.ForegroundColor = winPct > (100.0 / (oppCount + 1)) ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"{winPct:F2}% (Tie: {tiePct:F2}%)");
        Console.ResetColor();
        Console.WriteLine($"  CURRENT BEST: {currentBestDescription}\n");
    }

    public void RenderImprovementProbabilities(IEnumerable<ProbabilityResult> probabilities, HandType currentType)
    {
        Console.WriteLine("  IMPROVEMENT PROBABILITIES (Likeliest first):");
        foreach (var prob in probabilities)
        {
            if (prob.HandType == currentType) Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  - {prob.HandType,-16}: {prob.Percentage,8:F2}%");
            Console.ResetColor();
        }
    }

    public void RenderShowdownHeader()
    {
        Console.WriteLine("  SHOWDOWN - RESULTS:");
    }

    public void RenderPlayerShowdown(string name, ReadOnlySpan<Card> hole, string description, bool isWinner)
    {
        Console.Write($"  {name,-7}: ");
        RenderCard(hole[0]);
        RenderCard(hole[1]);
        Console.Write($"| {description}");
        if (isWinner)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" [WINNER]");
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    public void RenderPromptMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"\n  >>> {message}");
        Console.ResetColor();
        Console.ReadKey(true);
        for (int i = 0; i < 3; i++)
        {
            Console.Write(".");
            Thread.Sleep(50);
        }
    }

    public bool PromptReplayOrQuit()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n  [R] Reset & New Hand | [Q] Quit");
        Console.ResetColor();
        ConsoleKey key;
        do
        {
            key = Console.ReadKey(true).Key;
        } while (key != ConsoleKey.R && key != ConsoleKey.Q);

        return key == ConsoleKey.R;
    }
}
