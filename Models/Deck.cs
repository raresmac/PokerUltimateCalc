namespace PokerUltimateCalc.Models;

/// <summary>
/// Encapsulates a standard 52-card deck and shuffling operations.
/// </summary>
public class Deck
{
    private readonly Card[] _cards;
    private int _currentIndex;

    public Deck()
    {
        _cards = new Card[52];
        int index = 0;
        for (int s = 0; s < 4; s++)
        {
            for (int r = 0; r < 13; r++)
            {
                _cards[index++] = new Card((Rank)r, (Suit)s);
            }
        }
        _currentIndex = 0;
    }

    public void Shuffle(Random random)
    {
        for (int i = _cards.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
        _currentIndex = 0;
    }

    public Card Draw()
    {
        if (_currentIndex >= _cards.Length)
        {
            throw new InvalidOperationException("No cards remaining in deck.");
        }
        return _cards[_currentIndex++];
    }

    public Card[] Draw(int count)
    {
        var drawn = new Card[count];
        for (int i = 0; i < count; i++)
        {
            drawn[i] = Draw();
        }
        return drawn;
    }

    public Card[] GetRemainingCards(ReadOnlySpan<Card> knownCards)
    {
        var remaining = new List<Card>(52 - knownCards.Length);
        for (int i = 0; i < _cards.Length; i++)
        {
            bool isKnown = false;
            for (int j = 0; j < knownCards.Length; j++)
            {
                if (_cards[i] == knownCards[j])
                {
                    isKnown = true;
                    break;
                }
            }
            if (!isKnown)
            {
                remaining.Add(_cards[i]);
            }
        }
        return remaining.ToArray();
    }
}
