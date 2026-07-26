using PokerUltimateCalc.Models;
using PokerUltimateCalc.Services;
using PokerUltimateCalc.UI;

namespace PokerUltimateCalc.Engine;

/// <summary>
/// Controls Texas Hold'em game rounds, street progression, and state transitions.
/// </summary>
public class PokerGameEngine(
    IHandEvaluator evaluator,
    IEquityCalculator equityCalculator,
    IProbabilityCalculator probabilityCalculator,
    ConsoleRenderer renderer,
    Random random)
{
    private readonly IHandEvaluator _evaluator = evaluator;
    private readonly IEquityCalculator _equityCalculator = equityCalculator;
    private readonly IProbabilityCalculator _probabilityCalculator = probabilityCalculator;
    private readonly ConsoleRenderer _renderer = renderer;
    private readonly Random _random = random;

    public void RunHand(int opponentCount)
    {
        var deck = new Deck();
        deck.Shuffle(_random);

        Card[] playerHand = deck.Draw(2);
        Card[][] opponents = new Card[opponentCount][];
        for (int i = 0; i < opponentCount; i++)
        {
            opponents[i] = deck.Draw(2);
        }

        Card[] fullBoard = deck.Draw(5);
        Card[] currentBoard = Array.Empty<Card>();

        string[] stageNames = { "PRE-FLOP", "FLOP", "TURN", "RIVER" };
        int[] cardsToReveal = { 0, 3, 1, 1 };

        for (int s = 0; s < 4; s++)
        {
            if (cardsToReveal[s] > 0)
            {
                _renderer.RenderPromptMessage($"Deal the {stageNames[s]}...");
                Card[] nextBoard = new Card[currentBoard.Length + cardsToReveal[s]];
                Array.Copy(currentBoard, nextBoard, currentBoard.Length);
                for (int i = 0; i < cardsToReveal[s]; i++)
                {
                    nextBoard[currentBoard.Length + i] = fullBoard[currentBoard.Length + i];
                }
                currentBoard = nextBoard;
            }

            var knownCards = new Card[playerHand.Length + currentBoard.Length];
            playerHand.CopyTo(knownCards, 0);
            currentBoard.CopyTo(knownCards, playerHand.Length);

            Card[] simDeck = deck.GetRemainingCards(knownCards);
            bool isRiver = (s == 3);

            RunStreetStep(stageNames[s], playerHand, currentBoard, simDeck, opponentCount, isRiver, opponents);
        }
    }

    private void RunStreetStep(
        string stage,
        Card[] hole,
        Card[] board,
        Card[] simDeck,
        int opponentCount,
        bool isRiver,
        Card[][] opponents)
    {
        _renderer.RenderStageBox(stage, opponentCount, hole, board);

        var playerEval = _evaluator.Evaluate(hole, board);

        if (!isRiver)
        {
            var (winPct, tiePct) = _equityCalculator.CalculateEquity(hole, board, simDeck, opponentCount);
            _renderer.RenderEquityResults(winPct, tiePct, opponentCount, playerEval.Description);

            var probabilities = _probabilityCalculator.CalculateImprovementProbabilities(hole, board, simDeck, playerEval.HandType);
            _renderer.RenderImprovementProbabilities(probabilities, playerEval.HandType);
        }
        else
        {
            long maxScore = playerEval.Score;
            for (int i = 0; i < opponents.Length; i++)
            {
                long oppScore = _evaluator.Evaluate(opponents[i], board).Score;
                if (oppScore > maxScore)
                {
                    maxScore = oppScore;
                }
            }

            _renderer.RenderShowdownHeader();
            _renderer.RenderPlayerShowdown("You", hole, playerEval.Description, playerEval.Score == maxScore);

            for (int i = 0; i < opponents.Length; i++)
            {
                var oppEval = _evaluator.Evaluate(opponents[i], board);
                _renderer.RenderPlayerShowdown($"Opp {i + 1}", opponents[i], oppEval.Description, oppEval.Score == maxScore);
            }
        }
    }
}
