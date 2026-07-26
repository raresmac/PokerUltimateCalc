using PokerUltimateCalc.Engine;
using PokerUltimateCalc.Services;
using PokerUltimateCalc.UI;

namespace PokerUltimateCalc;

/// <summary>
/// Main application entry point for the Texas Hold'em Analytical Engine.
/// </summary>
internal class Program
{
    private static void Main(string[] args)
    {
        var random = new Random();
        var renderer = new ConsoleRenderer();
        var handEvaluator = new HandEvaluator();
        var equityCalculator = new EquityCalculator(handEvaluator, random);
        var probabilityCalculator = new ProbabilityCalculator(handEvaluator);

        var gameEngine = new PokerGameEngine(
            handEvaluator,
            equityCalculator,
            probabilityCalculator,
            renderer,
            random);

        renderer.InitializeConsole();
        renderer.RenderHeader();

        int opponentCount = renderer.PromptOpponentCount();
        bool keepPlaying = true;

        while (keepPlaying)
        {
            gameEngine.RunHand(opponentCount);
            keepPlaying = renderer.PromptReplayOrQuit();
        }
    }
}
