using PokerUltimateCalc.Models;

namespace PokerUltimateCalc.Services;

/// <summary>
/// Executes Monte Carlo simulations to estimate win and tie probabilities against opponents.
/// </summary>
public class EquityCalculator(IHandEvaluator evaluator, Random random) : IEquityCalculator
{
    private readonly IHandEvaluator _evaluator = evaluator;
    private readonly Random _random = random;

    public (double WinPercentage, double TiePercentage) CalculateEquity(
        ReadOnlySpan<Card> holeCards,
        ReadOnlySpan<Card> boardCards,
        ReadOnlySpan<Card> simulationDeck,
        int opponentCount,
        int iterations = 10000)
    {
        int wins = 0;
        int ties = 0;
        Card[] deckBuffer = new Card[simulationDeck.Length];
        int boardNeeded = 5 - boardCards.Length;

        for (int i = 0; i < iterations; i++)
        {
            simulationDeck.CopyTo(deckBuffer);
            for (int sIdx = deckBuffer.Length - 1; sIdx > 0; sIdx--)
            {
                int j = _random.Next(sIdx + 1);
                (deckBuffer[sIdx], deckBuffer[j]) = (deckBuffer[j], deckBuffer[sIdx]);
            }

            Card[] simBoard = new Card[5];
            boardCards.CopyTo(simBoard);
            for (int j = 0; j < boardNeeded; j++)
            {
                simBoard[boardCards.Length + j] = deckBuffer[j];
            }

            long myScore = _evaluator.Evaluate(holeCards, simBoard).Score;
            bool isBest = true;
            bool isTie = false;

            for (int p = 0; p < opponentCount; p++)
            {
                int oppCardOffset = boardNeeded + (p * 2);
                ReadOnlySpan<Card> oppHand = stackalloc Card[] { deckBuffer[oppCardOffset], deckBuffer[oppCardOffset + 1] };
                long oppScore = _evaluator.Evaluate(oppHand, simBoard).Score;

                if (oppScore > myScore)
                {
                    isBest = false;
                    isTie = false;
                    break;
                }
                if (oppScore == myScore)
                {
                    isTie = true;
                }
            }

            if (isBest && !isTie) wins++;
            else if (isBest && isTie) ties++;
        }

        double winPct = (double)wins / iterations * 100;
        double tiePct = (double)ties / iterations * 100;

        return (winPct, tiePct);
    }
}
