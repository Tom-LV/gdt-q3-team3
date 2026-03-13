using UnityEngine;
using System;

public static class EventTriggers
{
    public static Action<string> OnPuzzleSymbolActivated;
    public static void ActivatePuzzleSymbol(string symbolID)
    {
        OnPuzzleSymbolActivated?.Invoke(symbolID);
    }
}
