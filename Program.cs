using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PokerUltimateCalc
{
    public struct Card
    {
        public int Rank;
        public int Suit;

        public Card(int rank, int suit) { Rank = rank; Suit = suit; }

        public void Print()
        {
            string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
            string[] suits = { "♥", "♦", "♣", "♠" };
            if (Suit == 0 || Suit == 1) Console.ForegroundColor = ConsoleColor.Red;
            else Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{ranks[Rank]}{suits[Suit]} ");
            Console.ResetColor();
        }
    }

    public enum HandType { HighCard, Pair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush, RoyalFlush }

    // Helper to store probability data for sorting
    public struct ProbResult
    {
        public HandType Type;
        public double Value;
    }

    class Program
    {
        static Random rng = new Random();
        const int BOX_WIDTH = 62;
        static string[] RankNames = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║               TEXAS HOLD'EM SIMULATOR & CALC                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.Write("  Enter number of opponents (1-9): ");
            string input = Console.ReadLine();
            int numOpponents;
            if (!int.TryParse(input, out numOpponents)) numOpponents = 1;
            numOpponents = Math.Clamp(numOpponents, 1, 9);

            bool running = true;
            while (running)
            {
                RunGame(numOpponents);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n  [R] Reset & New Hand | [Q] Quit");
                Console.ResetColor();
                ConsoleKey key;
                do { key = Console.ReadKey(true).Key; } while (key != ConsoleKey.R && key != ConsoleKey.Q);
                if (key == ConsoleKey.Q) running = false;
            }
        }

        static void RunGame(int numOpponents)
        {
            Card[] deck = new Card[52];
            int dIdx = 0;
            for (int s = 0; s < 4; s++)
                for (int r = 0; r < 13; r++)
                    deck[dIdx++] = new Card(r, s);

            for (int i = 51; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                Card temp = deck[i]; deck[i] = deck[j]; deck[j] = temp;
            }

            int ptr = 0;
            Card[] playerHand = new Card[] { deck[ptr++], deck[ptr++] };
            Card[][] opponents = new Card[numOpponents][];
            for (int i = 0; i < numOpponents; i++)
                opponents[i] = new Card[] { deck[ptr++], deck[ptr++] };

            Card[] fullBoard = new Card[] { deck[ptr++], deck[ptr++], deck[ptr++], deck[ptr++], deck[ptr++] };
            Card[] currentBoard = new Card[0];

            string[] stageNames = { "PRE-FLOP", "FLOP", "TURN", "RIVER" };
            int[] cardsToReveal = { 0, 3, 1, 1 };

            for (int s = 0; s < 4; s++)
            {
                if (cardsToReveal[s] > 0)
                {
                    WaitForKey($"Deal the {stageNames[s]}...");
                    Card[] nextBoard = new Card[currentBoard.Length + cardsToReveal[s]];
                    Array.Copy(currentBoard, nextBoard, currentBoard.Length);
                    for (int i = 0; i < cardsToReveal[s]; i++) nextBoard[currentBoard.Length + i] = fullBoard[currentBoard.Length + i];
                    currentBoard = nextBoard;
                }

                Card[] knownCards = new Card[playerHand.Length + currentBoard.Length];
                playerHand.CopyTo(knownCards, 0);
                currentBoard.CopyTo(knownCards, playerHand.Length);
                Card[] simDeck = GetRemainingDeckManual(deck, knownCards);

                RunStep(stageNames[s], playerHand, currentBoard, simDeck, numOpponents, (s == 3), opponents);
            }
        }

        static void RunStep(string stage, Card[] hole, Card[] board, Card[] simDeck, int oppCount, bool isRiver, Card[][] opponents)
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ STAGE: {stage,-35} Opponents: {oppCount,-4} ║");

            Console.Write("║ Your Hand: ");
            foreach (var c in hole) c.Print();
            Console.WriteLine(new string(' ', BOX_WIDTH - 11 - (hole.Length * 3)) + "║");

            Console.Write("║ Board:     ");
            if (board.Length == 0)
            {
                string wait = "[Waiting...]";
                Console.Write(wait);
                Console.WriteLine(new string(' ', BOX_WIDTH - 11 - wait.Length) + "║");
            }
            else
            {
                foreach (var c in board) c.Print();
                Console.WriteLine(new string(' ', BOX_WIDTH - 11 - (board.Length * 3)) + "║");
            }
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

            HandType myType;
            long myScore = EvaluateScoreInternal(hole, board, out myType);

            if (!isRiver)
            {
                double winPct = CalculateEquityManual(hole, board, simDeck, oppCount, out double tiePct);
                Console.Write("  WIN CHANCE: ");
                Console.ForegroundColor = winPct > (100.0 / (oppCount + 1)) ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"{winPct:F2}% (Tie: {tiePct:F2}%)");
                Console.ResetColor();
                Console.WriteLine($"  CURRENT BEST: {GetDescriptionManual(hole, board)}\n");
                Console.WriteLine("  IMPROVEMENT PROBABILITIES (Likeliest first):");
                ShowProbabilitiesSorted(hole, board, simDeck, myType);
            }
            else
            {
                long maxScore = myScore;
                for (int i = 0; i < opponents.Length; i++)
                {
                    HandType dummy;
                    long s = EvaluateScoreInternal(opponents[i], board, out dummy);
                    if (s > maxScore) maxScore = s;
                }

                Console.WriteLine("  SHOWDOWN - RESULTS:");
                PrintPlayerShowdown("You", hole, board, myScore, maxScore);
                for (int i = 0; i < opponents.Length; i++)
                    PrintPlayerShowdown($"Opp {i + 1}", opponents[i], board, EvaluateScoreInternal(opponents[i], board, out _), maxScore);
            }
        }

        static void PrintPlayerShowdown(string name, Card[] hole, Card[] board, long score, long max)
        {
            Console.Write($"  {name,-7}: ");
            hole[0].Print(); hole[1].Print();
            Console.Write($"| {GetDescriptionManual(hole, board)}");
            if (score == max)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" [WINNER]");
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        static void ShowProbabilitiesSorted(Card[] hole, Card[] board, Card[] deck, HandType cur)
        {
            long[] counts = new long[10]; int toDraw = 5 - board.Length; long total = 0; HandType h;
            if (toDraw == 2)
            {
                for (int a = 0; a < deck.Length; a++)
                    for (int b = a + 1; b < deck.Length; b++)
                    {
                        EvaluateScoreInternal(hole, board, out h, deck[a], deck[b]);
                        counts[(int)h]++; total++;
                    }
            }
            else if (toDraw == 1)
            {
                for (int a = 0; a < deck.Length; a++)
                {
                    EvaluateScoreInternal(hole, board, out h, deck[a]);
                    counts[(int)h]++; total++;
                }
            }
            if (total == 0) return;

            ProbResult[] results = new ProbResult[10];
            int resCount = 0;
            for (int i = (int)cur; i < 10; i++)
            {
                double p = (double)counts[i] / total * 100;
                if (p > 0 || i == (int)cur)
                {
                    results[resCount].Type = (HandType)i;
                    results[resCount].Value = p;
                    resCount++;
                }
            }

            for (int i = 0; i < resCount - 1; i++)
            {
                for (int j = 0; j < resCount - i - 1; j++)
                {
                    if (results[j].Value < results[j + 1].Value)
                    {
                        ProbResult temp = results[j];
                        results[j] = results[j + 1];
                        results[j + 1] = temp;
                    }
                }
            }

            for (int i = 0; i < resCount; i++)
            {
                if (results[i].Type == cur) Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  - {results[i].Type,-16}: {results[i].Value,8:F2}%");
                Console.ResetColor();
            }
        }

        static string GetDescriptionManual(Card[] hole, Card[] board)
        {
            HandType type;
            EvaluateScoreInternal(hole, board, out type);

            Span<int> rCounts = stackalloc int[13];
            Span<uint> sMasks = stackalloc uint[4];
            uint allR = 0;
            for (int i = 0; i < hole.Length; i++) AddStats(hole[i], rCounts, sMasks, ref allR);
            for (int i = 0; i < board.Length; i++) AddStats(board[i], rCounts, sMasks, ref allR);

            if (type == HandType.RoyalFlush) return "Royal Flush";

            int q = -1, t1 = -1, t2 = -1, p1 = -1, p2 = -1, high = -1;
            for (int i = 12; i >= 0; i--)
            {
                if (rCounts[i] == 4) q = i;
                else if (rCounts[i] == 3) { if (t1 == -1) t1 = i; else t2 = i; }
                else if (rCounts[i] == 2) { if (p1 == -1) p1 = i; else p2 = i; }
                if (rCounts[i] > 0 && high == -1) high = i;
            }

            switch (type)
            {
                case HandType.StraightFlush: return $"Straight Flush ({RankNames[high]} high)";
                case HandType.FourOfAKind: return $"Four of a Kind: {RankNames[q]}s";
                case HandType.FullHouse: return $"Full House: {RankNames[t1]}s over {RankNames[p1 != -1 ? p1 : t2]}s";
                case HandType.Flush: return $"Flush ({RankNames[high]} high)";
                case HandType.Straight: return $"Straight ({RankNames[high]} high)";
                case HandType.ThreeOfAKind: return $"Three of a Kind: {RankNames[t1]}s";
                case HandType.TwoPair: return $"Two Pair: {RankNames[p1]}s and {RankNames[p2]}s";
                case HandType.Pair: return $"Pair of {RankNames[p1]}s";
                default: return $"High Card: {RankNames[high]}";
            }
        }

        static long EvaluateScoreInternal(Card[] hole, Card[] board, out HandType type, params Card[] drawn)
        {
            Span<int> rCounts = stackalloc int[13];
            Span<uint> sMasks = stackalloc uint[4];
            uint allR = 0;
            for (int i = 0; i < hole.Length; i++) AddStats(hole[i], rCounts, sMasks, ref allR);
            for (int i = 0; i < board.Length; i++) AddStats(board[i], rCounts, sMasks, ref allR);
            for (int i = 0; i < drawn.Length; i++) AddStats(drawn[i], rCounts, sMasks, ref allR);

            int fS = -1;
            for (int i = 0; i < 4; i++)
            {
                uint m = sMasks[i]; int c = 0; while (m > 0) { m &= m - 1; c++; }
                if (c >= 5) fS = i;
            }
            if (fS != -1)
            {
                uint m = sMasks[fS];
                if ((m & 0x1F00) == 0x1F00) { type = HandType.RoyalFlush; return 9000000; }
                for (int i = 8; i >= 0; i--) if ((m & (0x1Fu << i)) == (0x1Fu << i)) { type = HandType.StraightFlush; return 8000000 + i; }
                if ((m & 0x100F) == 0x100F) { type = HandType.StraightFlush; return 8000000; }
            }
            int q = -1, t1 = -1, t2 = -1, p1 = -1, p2 = -1;
            for (int i = 12; i >= 0; i--)
            {
                if (rCounts[i] == 4) q = i;
                else if (rCounts[i] == 3) { if (t1 == -1) t1 = i; else t2 = i; }
                else if (rCounts[i] == 2) { if (p1 == -1) p1 = i; else p2 = i; }
            }
            if (q != -1) { type = HandType.FourOfAKind; return 7000000 + q; }
            if (t1 != -1 && (p1 != -1 || t2 != -1)) { type = HandType.FullHouse; return 6000000 + t1; }
            if (fS != -1) { type = HandType.Flush; return 5000000 + (long)allR; }
            for (int i = 8; i >= 0; i--) if ((allR & (0x1Fu << i)) == (0x1Fu << i)) { type = HandType.Straight; return 4000000 + i; }
            if ((allR & 0x100F) == 0x100F) { type = HandType.Straight; return 4000000; }
            if (t1 != -1) { type = HandType.ThreeOfAKind; return 3000000 + t1; }
            if (p1 != -1 && p2 != -1) { type = HandType.TwoPair; return 2000000 + (p1 * 100) + p2; }
            if (p1 != -1) { type = HandType.Pair; return 1000000 + p1; }
            type = HandType.HighCard; return allR;
        }

        static void AddStats(Card c, Span<int> rCounts, Span<uint> sMasks, ref uint allR)
        {
            rCounts[c.Rank]++; sMasks[c.Suit] |= (1u << c.Rank); allR |= (1u << c.Rank);
        }

        static double CalculateEquityManual(Card[] hole, Card[] board, Card[] deck, int opps, out double tiePct)
        {
            int iter = 3000; int wins = 0, ties = 0;
            Card[] simDeckCopy = new Card[deck.Length];
            int boardNeeded = 5 - board.Length;

            for (int i = 0; i < iter; i++)
            {
                deck.CopyTo(simDeckCopy, 0);
                for (int sIdx = simDeckCopy.Length - 1; sIdx > 0; sIdx--)
                {
                    int j = rng.Next(sIdx + 1);
                    Card t = simDeckCopy[sIdx]; simDeckCopy[sIdx] = simDeckCopy[j]; simDeckCopy[j] = t;
                }
                Card[] sBoard = new Card[5]; board.CopyTo(sBoard, 0);
                for (int j = 0; j < boardNeeded; j++) sBoard[board.Length + j] = simDeckCopy[j];
                HandType dummy;
                long myS = EvaluateScoreInternal(hole, sBoard, out dummy);
                bool best = true; bool tie = false;
                for (int p = 0; p < opps; p++)
                {
                    Card[] oH = new Card[] { simDeckCopy[boardNeeded + (p * 2)], simDeckCopy[boardNeeded + (p * 2) + 1] };
                    long oS = EvaluateScoreInternal(oH, sBoard, out dummy);
                    if (oS > myS) { best = false; tie = false; break; }
                    if (oS == myS) tie = true;
                }
                if (best && !tie) wins++; else if (best && tie) ties++;
            }
            tiePct = (double)ties / iter * 100;
            return (double)wins / iter * 100;
        }

        static Card[] GetRemainingDeckManual(Card[] full, Card[] excluded)
        {
            List<Card> rem = new List<Card>();
            for (int i = 0; i < full.Length; i++)
            {
                bool found = false;
                for (int j = 0; j < excluded.Length; j++)
                    if (full[i].Rank == excluded[j].Rank && full[i].Suit == excluded[j].Suit) { found = true; break; }
                if (!found) rem.Add(full[i]);
            }
            return rem.ToArray();
        }

        static void WaitForKey(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\n  >>> {msg}"); Console.ResetColor();
            Console.ReadKey(true);
            for (int i = 0; i < 3; i++) { Console.Write("."); System.Threading.Thread.Sleep(50); }
        }
    }
}
