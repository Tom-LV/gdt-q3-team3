using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    void OnEnable()
    {
        EventTriggers.OnPuzzleSymbolActivated += HandleSymbol;
    }

    void OnDisable()
    {
        EventTriggers.OnPuzzleSymbolActivated -= HandleSymbol;
    }

    void HandleSymbol(string symbolID)
    {
        Debug.Log("Symbol activated: " + symbolID);
    }
}
