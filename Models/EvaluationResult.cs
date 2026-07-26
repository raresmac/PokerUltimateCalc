namespace PokerUltimateCalc.Models;

/// <summary>
/// Contains the evaluated score, hand type, and descriptive string of a hand.
/// </summary>
public readonly record struct EvaluationResult(HandType HandType, long Score, string Description);
