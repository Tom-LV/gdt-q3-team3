using UnityEngine;

[RequireComponent(typeof(Pipe))]
public class PipeOutput : MonoBehaviour
{
    [Tooltip("The final pitch value required to activate this flute. Red adds 1, Blue subtracts 1.")]
    public int targetNoteValue = 0;

    [HideInInspector] public bool isSatisfied = false;

    public void Reached(int pitch)
    {
        Debug.Log(gameObject.name + ": " + (pitch == targetNoteValue));
    }
}