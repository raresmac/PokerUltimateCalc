# PokerUltimateCalc

A terminal-based Texas Hold'em poker calculator and simulator written in C#. It gives you real-time win probabilities and hand improvement odds against 1 to 9 opponents.

## Features

* **Multi-Player Support**: Play hands against 1 to 9 opponents.
* **Real-Time Odds**: Calculates win and tie percentages updated at every street (Pre-flop, Flop, Turn, River).
* **Hand Improvement Probabilities**: Shows exact odds for making a Pair, Straight, Flush, Full House, etc.
* **Showdown Breakdown**: Reveals opponent hands at the River with clear descriptions of each hand.
* **Clean Terminal Interface**: Uses colored UTF-8 card symbols (♥, ♦, ♣, ♠) and boxed layouts.

## Project Architecture & Performance

The project is structured into modular layers following SOLID and OOP principles:

* **Models**: Immutable card representations (`Card` struct), deck management, and result contracts.
* **Services**: 
  * `HandEvaluator`: Evaluates 7-card poker hands using 32-bit masks and `Span<T>` buffers for high performance.
  * `EquityCalculator`: Runs 10,000 Monte Carlo simulations per street to calculate win/tie equity.
  * `ProbabilityCalculator`: Computes exact combinatorial odds for remaining deck cards.
* **UI & Engine**: `ConsoleRenderer` handles terminal output, and `PokerGameEngine` manages game state transitions.

## Quick Start

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Running the App

```bash
git clone https://github.com/raresmac/PokerUltimateCalc.git
cd PokerUltimateCalc
dotnet run
```

## Controls

1. Enter the number of opponents (1 to 9).
2. Press any key to proceed through each street (Flop, Turn, River).
3. Press **[R]** to reset and start a new hand.
4. Press **[Q]** to quit.

---

**Author:** Rares-Stefan Macovei
