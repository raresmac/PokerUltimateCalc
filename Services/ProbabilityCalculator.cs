using PokerUltimateCalc.Models;

namespace PokerUltimateCalc.Services;

/// <summary>
/// Evaluates exact combinatorics for hand improvement possibilities on turn and river streets.
/// </summary>
public class ProbabilityCalculator(IHandEvaluator evaluator) : IProbabilityCalculator
{
    private readonly IHandEvaluator _evaluator = evaluator;

    public IReadOnlyList<ProbabilityResult> CalculateImprovementProbabilities(
        ReadOnlySpan<Card> holeCards,
        ReadOnlySpan<Card> boardCards,
        ReadOnlySpan<Card> simulationDeck,
        HandType currentHandType)
    {
        long[] counts = new long[10];
        int toDraw = 5 - boardCards.Length;
        long total = 0;

        if (toDraw == 2)
        {
            for (int a = 0; a < simulationDeck.Length; a++)
            {
                for (int b = a + 1; b < simulationDeck.Length; b++)
                {
                    ReadOnlySpan<Card> drawn = stackalloc Card[] { simulationDeck[a], simulationDeck[b] };
                    var result = _evaluator.Evaluate(holeCards, boardCards, drawn);
                    counts[(int)result.HandType]++;
                    total++;
                }
            }
        }
        else if (toDraw == 1)
        {
            for (int a = 0; a < simulationDeck.Length; a++)
            {
                ReadOnlySpan<Card> drawn = stackalloc Card[] { simulationDeck[a] };
                var result = _evaluator.Evaluate(holeCards, boardCards, drawn);
                counts[(int)result.HandType]++;
                total++;
            }
        }

        if (total == 0)
        {
            return Array.Empty<ProbabilityResult>();
        }

        var results = new List<ProbabilityResult>();
        for (int i = (int)currentHandType; i < 10; i++)
        {
            double percentage = (double)counts[i] / total * 100;
            if (percentage > 0 || i == (int)currentHandType)
            {
                results.Add(new ProbabilityResult((HandType)i, percentage));
            }
        }

        results.Sort((x, y) => y.Percentage.CompareTo(x.Percentage));
        return results;
    }
}
