using UnityEngine;
using System.Collections;

public class CenterLightPole : LightPole
{
    [Header("Center Pole Settings")]
    [SerializeField] private Transform topCover;
    [SerializeField] private LightPuzzleManager manager;
    
    [Tooltip("How far down the entire pole should sink into the ground during Phase 2.")]
    [SerializeField] private float finalDropAmount = 1f;

    private bool isRevealing = false;

    public void StartLight()
    {
        // Triggers the standard base class logic
        base.Collapse();
    }

    public override void Uncollapse()
    {
        if (isRevealing) return;
        base.Uncollapse();
    }

    public override void Collapse()
    {
        // Ignore if puzzle isn't solved, or if we are already doing the animation
        if (!manager.PuzzleSolved() || isRevealing) return;
        DisableBeam();
        RevealButton();
    }

    public void RevealButton()
    {
        if (isRevealing) return;
        isRevealing = true;
        
        StartCoroutine(RevealSequence());
    }

    private IEnumerator RevealSequence()
    {
        // Lower the Top Cover
        float t = 0f;
        Vector3 startCoverPos = topCover.localPosition;
        
        // Safely calculate the local target position based on the base class's bottomTransform
        Vector3 endCoverPos = topCover.parent != null 
            ? topCover.parent.InverseTransformPoint(bottomTransform.position) 
            : bottomTransform.position;

        while (t < collapseTime)
        {
            t += Time.deltaTime;
            float lerpT = t / collapseTime;
            topCover.localPosition = Vector3.Lerp(startCoverPos, endCoverPos, lerpT);
            yield return null;
        }
        
        // Snap precisely at the end to prevent floating point inaccuracies
        topCover.localPosition = endCoverPos; 


        // Lower the Entire Pole into the floor
        t = 0f;
        Vector3 startPolePos = transform.localPosition;
        Vector3 endPolePos = startPolePos + (Vector3.down * finalDropAmount);

        while (t < collapseTime)
        {
            t += Time.deltaTime;
            float lerpT = t / collapseTime;
            transform.localPosition = Vector3.Lerp(startPolePos, endPolePos, lerpT);
            yield return null;
        }
        
        // Snap precisely at the end
        transform.localPosition = endPolePos; 
    }
}