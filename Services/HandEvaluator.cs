using PokerUltimateCalc.Models;

namespace PokerUltimateCalc.Services;

/// <summary>
/// Evaluates Texas Hold'em hands using bitmask arithmetic and zero-allocation stack buffers.
/// </summary>
public class HandEvaluator : IHandEvaluator
{
    private static readonly string[] RankNames = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

    public EvaluationResult Evaluate(ReadOnlySpan<Card> holeCards, ReadOnlySpan<Card> boardCards, ReadOnlySpan<Card> drawnCards = default)
    {
        Span<int> rankCounts = stackalloc int[13];
        Span<uint> suitMasks = stackalloc uint[4];
        uint allRanksMask = 0;

        for (int i = 0; i < holeCards.Length; i++) AddStats(holeCards[i], rankCounts, suitMasks, ref allRanksMask);
        for (int i = 0; i < boardCards.Length; i++) AddStats(boardCards[i], rankCounts, suitMasks, ref allRanksMask);
        for (int i = 0; i < drawnCards.Length; i++) AddStats(drawnCards[i], rankCounts, suitMasks, ref allRanksMask);

        int flushSuit = -1;
        for (int i = 0; i < 4; i++)
        {
            uint mask = suitMasks[i];
            int count = 0;
            while (mask > 0)
            {
                mask &= mask - 1;
                count++;
            }
            if (count >= 5)
            {
                flushSuit = i;
            }
        }

        if (flushSuit != -1)
        {
            uint mask = suitMasks[flushSuit];
            if ((mask & 0x1F00) == 0x1F00)
            {
                return new EvaluationResult(HandType.RoyalFlush, 9000000, "Royal Flush");
            }
            for (int i = 8; i >= 0; i--)
            {
                if ((mask & (0x1Fu << i)) == (0x1Fu << i))
                {
                    return new EvaluationResult(HandType.StraightFlush, 8000000 + i, $"Straight Flush ({RankNames[i + 4]} high)");
                }
            }
            if ((mask & 0x100F) == 0x100F)
            {
                return new EvaluationResult(HandType.StraightFlush, 8000000, "Straight Flush (5 high)");
            }
        }

        int quads = -1, trips1 = -1, trips2 = -1, pair1 = -1, pair2 = -1;
        for (int i = 12; i >= 0; i--)
        {
            if (rankCounts[i] == 4) quads = i;
            else if (rankCounts[i] == 3)
            {
                if (trips1 == -1) trips1 = i;
                else trips2 = i;
            }
            else if (rankCounts[i] == 2)
            {
                if (pair1 == -1) pair1 = i;
                else pair2 = i;
            }
        }

        if (quads != -1)
        {
            return new EvaluationResult(HandType.FourOfAKind, 7000000 + quads, $"Four of a Kind: {RankNames[quads]}s");
        }

        if (trips1 != -1 && (pair1 != -1 || trips2 != -1))
        {
            int pairRank = pair1 != -1 ? pair1 : trips2;
            return new EvaluationResult(HandType.FullHouse, 6000000 + trips1, $"Full House: {RankNames[trips1]}s over {RankNames[pairRank]}s");
        }

        if (flushSuit != -1)
        {
            uint mask = suitMasks[flushSuit];
            int flushHigh = -1;
            for (int i = 12; i >= 0; i--)
            {
                if ((mask & (1u << i)) != 0)
                {
                    flushHigh = i;
                    break;
                }
            }
            return new EvaluationResult(HandType.Flush, 5000000 + allRanksMask, $"Flush ({RankNames[flushHigh]} high)");
        }

        for (int i = 8; i >= 0; i--)
        {
            if ((allRanksMask & (0x1Fu << i)) == (0x1Fu << i))
            {
                return new EvaluationResult(HandType.Straight, 4000000 + i, $"Straight ({RankNames[i + 4]} high)");
            }
        }

        if ((allRanksMask & 0x100F) == 0x100F)
        {
            return new EvaluationResult(HandType.Straight, 4000000, "Straight (5 high)");
        }

        if (trips1 != -1)
        {
            return new EvaluationResult(HandType.ThreeOfAKind, 3000000 + trips1, $"Three of a Kind: {RankNames[trips1]}s");
        }

        if (pair1 != -1 && pair2 != -1)
        {
            return new EvaluationResult(HandType.TwoPair, 2000000 + (pair1 * 100) + pair2, $"Two Pair: {RankNames[pair1]}s and {RankNames[pair2]}s");
        }

        if (pair1 != -1)
        {
            return new EvaluationResult(HandType.Pair, 1000000 + pair1, $"Pair of {RankNames[pair1]}s");
        }

        int highCard = -1;
        for (int i = 12; i >= 0; i--)
        {
            if (rankCounts[i] > 0)
            {
                highCard = i;
                break;
            }
        }

        return new EvaluationResult(HandType.HighCard, allRanksMask, $"High Card: {RankNames[highCard]}");
    }

    public string GetDescription(ReadOnlySpan<Card> holeCards, ReadOnlySpan<Card> boardCards)
    {
        return Evaluate(holeCards, boardCards).Description;
    }

    private static void AddStats(Card c, Span<int> rankCounts, Span<uint> suitMasks, ref uint allRanksMask)
    {
        rankCounts[(int)c.Rank]++;
        suitMasks[(int)c.Suit] |= (1u << (int)c.Rank);
        allRanksMask |= (1u << (int)c.Rank);
    }
}
