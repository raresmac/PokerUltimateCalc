# 🃏 PokerUltimateCalc: High-Performance Poker Analytical Engine

A professional-grade **Texas Hold'em Analytical Engine** and terminal simulator built in C#. This project provides real-time, exact mathematical probabilities and win equity calculations for 1 to 10 players (you + 9 opponents).

## Overview
`PokerUltimateCalc` is a specialized tool for calculating poker "outs" and "equity." It uses a combination of **Combinatorial Analysis** and **Monte Carlo Simulations** to provide a clear statistical picture of any given hand at any stage (Pre-flop, Flop, Turn, or River).

---

## Key Features
*   **Multi-Player Simulation:** Dynamically calculate your win/tie/loss probabilities against up to 9 opponents.
*   **Real-Time Equity:** View your "Win Chance" updated at every street (Flop, Turn, River).
*   **Exact Combinatorics:** Calculates the exact percentage chance of improving your hand by analyzing every possible card remaining in the deck.
*   **Smart Hand Filtering:** Only displays hand probabilities that are better than or equal to your current hand.
*   **Detailed Showdown:** Reveals all opponent cards at the end of the round with human-readable descriptions (e.g., *"Full House: Aces full of Kings"*).
*   **Optimized Terminal UI:** Fixed-width box layout using UTF-8 card symbols (♥, ♦, ♣, ♠) and color-coding for red/black suits.

---

## Technical Architecture
This project focuses on **high performance** and **low-latency execution**, specifically tailored to the constraints of the C# memory model.

### Performance Optimization
*   **Bitmasking:** Hand evaluation is performed using bitwise operators and 32-bit masks, allowing the engine to identify straights and flushes in nanoseconds.
*   **Zero-Allocation Logic:** The evaluation loops use `Span<T>` and `stackalloc` to keep memory on the **Stack**, ensuring **Zero Garbage Collection (GC)** overhead during simulations.
*   **No LINQ/Lambdas:** To prevent "ref local" capture errors and hidden heap allocations, the engine uses strictly manual loops and static helper methods.
*   **Fisher-Yates Shuffling:** Implements an unbiased, O(n) complexity shuffling algorithm for true randomization.

### Mathematical Methodology
The engine uses a dual-math approach:
1.  **Combinatorial Analysis:** For improvement probabilities, it iterates through every remaining card combination ($nCr$) to provide the **exact** percentage of hitting a specific hand.
2.  **Monte Carlo Simulation:** For Win Equity, it simulates thousands of random "future scenarios" against pre-dealt hidden opponent hands to provide a statistically significant winning percentage.

---

## Usage
1.  **Initialize:** Enter the number of opponents.
2.  **Analyze:** Review the **Win Chance** and **Improvement Probabilities** table (sorted from most likely to least likely).
3.  **Progress:** Press any key to deal the next street.
4.  **Showdown:** Observe the final winners and the specific hand descriptions at the River.
5.  **Replay:** Press `R` to instantly deal a new hand or `Q` to quit.

---

### **Author**
Developed by Rares-Stefan Macovei
