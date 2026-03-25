using UnityEngine;
using System.Collections;

public class CenterLightPole : LightPole
{
    [SerializeField] private Transform topCover;
    public void StartLight()
    {
        base.Collapse();
    }
    public override void Collapse()
    {
        LightPole[] allPoles = FindObjectsOfType<LightPole>();
        foreach(LightPole pole in allPoles)
        {
            if(pole.IsInteractable()) return;
        }
        RevealButton();
    }
    public void RevealButton()
    {
        if (IsInteractable()) return;
        StartCoroutine(RevealRoutine());
    }
    private IEnumerator RevealRoutine()
    {
        float t = 0f;
        Vector3 startPos = topCover.position;
        Vector3 endPos = bottomTransform.position;

        while (t < collapseTime)
        {
            t += Time.deltaTime;
            float lerpT = t / collapseTime;
            topCover.position = Vector3.Lerp(startPos, endPos, lerpT);

            yield return null;
        }
    }
}
