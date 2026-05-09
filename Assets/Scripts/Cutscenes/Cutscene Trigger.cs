using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private Vector3 cutscenePos;
    [SerializeField] private Vector3 cutsceneEuler;
    [SerializeField] private float cutsceneTime;
    [SerializeField] private bool triggerOnlyOnce = true;
    private bool triggered;
    public void DoStaticCutscene()
    {
        if(!triggerOnlyOnce || !triggered)
        {
            CutsceneManager.Instance.DoStaticCutscene(cutscenePos, Quaternion.Euler(cutsceneEuler), cutsceneTime);
            triggered = true;
        }
    }
}
