using PokerUltimateCalc.Models;

namespace PokerUltimateCalc.Services;

/// <summary>
/// Defines hand evaluation logic and human-readable description formatting.
/// </summary>
public interface IHandEvaluator
{
    EvaluationResult Evaluate(ReadOnlySpan<Card> holeCards, ReadOnlySpan<Card> boardCards, ReadOnlySpan<Card> drawnCards = default);
    string GetDescription(ReadOnlySpan<Card> holeCards, ReadOnlySpan<Card> boardCards);
}
