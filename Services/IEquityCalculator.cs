using PokerUltimateCalc.Models;

namespace PokerUltimateCalc.Services;

/// <summary>
/// Computes win and tie equity percentages for Texas Hold'em hands.
/// </summary>
public interface IEquityCalculator
{
    (double WinPercentage, double TiePercentage) CalculateEquity(
        ReadOnlySpan<Card> holeCards,
        ReadOnlySpan<Card> boardCards,
        ReadOnlySpan<Card> simulationDeck,
        int opponentCount,
        int iterations = 10000);
}
