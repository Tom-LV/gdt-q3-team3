using UnityEngine;

public class LightPuzzleManager : MonoBehaviour
{
    [SerializeField] private LightPole[] puzzleElements;
    [SerializeField] private Door starter;

    void Awake()
    {
        // CenterPole goes at index 0
        Transform[] polePositions = new Transform[puzzleElements.Length];

        for (int i = 0; i < puzzleElements.Length; i++)
        {
            polePositions[i] = puzzleElements[i].transform;
        }

        foreach (LightPole pole in puzzleElements)
        {
            pole.Initialize(polePositions);
        }
        starter.Open();
    }

    public bool PuzzleSolved()
    {
        foreach (LightPole pole in puzzleElements)
        {
            if(!pole.isCollapsed()) return false;
        }
        return true;
    }
}