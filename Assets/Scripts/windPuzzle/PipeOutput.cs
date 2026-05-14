using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Pipe))]
public class PipeOutput : MonoBehaviour
{
    [Tooltip("The final pitch value required to activate this flute. Red adds 1, Blue subtracts 1.")]
    public int targetNoteValue = 0;
    [SerializeField] UnityEvent<int> m_OnAirReached;

    [HideInInspector] public bool isSatisfied = false;

    public void Reached(int pitch)
    {
        if (m_OnAirReached != null) m_OnAirReached.Invoke(pitch);
    }
}