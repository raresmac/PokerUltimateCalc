namespace PokerUltimateCalc.Models;

/// <summary>
/// Represents an immutable playing card.
/// </summary>
public readonly record struct Card(Rank Rank, Suit Suit)
{
    private static readonly string[] RankSymbols = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
    private static readonly string[] SuitSymbols = { "♥", "♦", "♣", "♠" };

    public string RankSymbol => RankSymbols[(int)Rank];
    public string SuitSymbol => SuitSymbols[(int)Suit];

    public bool IsRed => Suit == Suit.Hearts || Suit == Suit.Diamonds;

    public override string ToString() => $"{RankSymbol}{SuitSymbol}";
}
