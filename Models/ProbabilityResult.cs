namespace PokerUltimateCalc.Models;

/// <summary>
/// Stores the calculated probability percentage for a specific hand type.
/// </summary>
public readonly record struct ProbabilityResult(HandType HandType, double Percentage);
