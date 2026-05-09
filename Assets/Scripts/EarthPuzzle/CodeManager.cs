using UnityEngine;
using UnityEngine.Events;

public class CodeManager : MonoBehaviour
{
    [SerializeField] private UnityEvent onFinishedPuzzle;
    public void CheckFinishedPuzzleAndDoEvent()
    {
        CodeElement[] children = GetComponentsInChildren<CodeElement>();
        foreach(CodeElement bolt in children)
        {
            if(!bolt.IsInCorrectState()) return;
        }
        onFinishedPuzzle?.Invoke();
    }
}
