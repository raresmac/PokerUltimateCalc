using PokerUltimateCalc.Models;

namespace PokerUltimateCalc.Services;

/// <summary>
/// Calculates odds of achieving specific hand improvements given remaining deck cards.
/// </summary>
public interface IProbabilityCalculator
{
    IReadOnlyList<ProbabilityResult> CalculateImprovementProbabilities(
        ReadOnlySpan<Card> holeCards,
        ReadOnlySpan<Card> boardCards,
        ReadOnlySpan<Card> simulationDeck,
        HandType currentHandType);
}
