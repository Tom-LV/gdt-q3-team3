using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Allows us to trigger doors/chests in the inspector!

public class SequencePuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("The exact order the elements must be touched.")]
    public List<Element> CorrectSequence;

    [Header("Scene References")]
    public List<PuzzleTile> AllNodes;
    public PuzzleRoom room;

    [Header("Win Event")]
    public UnityEvent m_OnPuzzleSolved;

    private int _currentStepIndex = 0;
    private bool _isSolved = false;


    // The individual nodes will call this method when the player touches them
    public void OnNodeTouched(PuzzleTile touchedNode)
    {
        if (_isSolved) return; // Don't do anything if already solved
        if (touchedNode.IsLit) return; // Ignore if the player touches an already lit node

        // Check if the touched element matches the current step in our sequence
        if (touchedNode.RequiredElement == CorrectSequence[_currentStepIndex])
        {
            // CORRECT!
            Debug.Log($"Correct! {touchedNode.RequiredElement} touched.");
            touchedNode.SetLitState(true);
            AllNodes[_currentStepIndex].SetLitState(true);
            _currentStepIndex++;

            // Did we finish the sequence?
            if (_currentStepIndex >= CorrectSequence.Count)
            {
                Debug.Log("Puzzle Solved!");
                PhoneOS.Instance.GetApp<ChatApp>().ReceiveMessage("System", "Puzzle solved, the code is 225599!", Color.cyan);
                _isSolved = true;
                m_OnPuzzleSolved?.Invoke(); // Trigger your doors, chests, or sounds here!
            }
        }
        else
        {
            // INCORRECT! Reset the puzzle.
            Debug.Log($"Wrong! Expected {CorrectSequence[_currentStepIndex]}, but touched {touchedNode.RequiredElement}. Resetting...");
            room.ResetRoom();
        }
    }
}