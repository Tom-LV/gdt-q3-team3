using UnityEngine;
using System.Collections;

public class CenterLightPole : LightPole
{
    [SerializeField] private Transform topCover;
    [SerializeField] private LightPuzzleManager manager;
    public void StartLight()
    {
        base.Collapse();
    }
    public override void Collapse()
    {
        if(!manager.PuzzleSolved()) return;
        RevealButton();
    }
    public void RevealButton()
    {
        StartCoroutine(RevealRoutine1());
    }
    private IEnumerator RevealRoutine1()
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
        StartCoroutine(RevealRoutine2());
    }
    private IEnumerator RevealRoutine2()
    {
        float t = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position - Vector3.up;

        while (t < collapseTime)
        {
            t += Time.deltaTime;
            float lerpT = t / collapseTime;
            transform.position = Vector3.Lerp(startPos, endPos, lerpT);

            yield return null;
        }
        transform.position = endPos;
    }
}
