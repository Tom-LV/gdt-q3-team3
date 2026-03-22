using System.Collections;
using UnityEngine;

public class SlidingBlock : MonoBehaviour
{
    [Tooltip("How far the block moves per button press (e.g., 2 units)")]
    public float gridSize = 2f;
    [Tooltip("How fast the block slides")]
    public float slideSpeed = 5f;

    private bool isMoving = false;
    private BoxCollider[] myBoxColliders;

    void Start()
    {
        // Grab every single BoxCollider anywhere in the children
        myBoxColliders = GetComponentsInChildren<BoxCollider>();
    }

    // Unity Event Wrappers
    public void MoveForward() { TryMove(Vector3.back); }
    public void MoveBack() { TryMove(Vector3.forward); }
    public void MoveRight() { TryMove(Vector3.left); }
    public void MoveLeft() { TryMove(Vector3.right); }

    private void TryMove(Vector3 direction)
    {
        if (isMoving) return;

        bool pathBlocked = false;

        // Check the destination for every single differently-scaled box collider
        foreach (BoxCollider col in myBoxColliders)
        {
            if (col.isTrigger) continue;

            // 1. Calculate exact world center and size
            Vector3 worldCenter = col.transform.TransformPoint(col.center);
            Vector3 trueSize = Vector3.Scale(col.size, col.transform.lossyScale);
            Vector3 extents = (trueSize * 0.5f) * 0.95f;

            // 2. Calculate exactly where this specific box WANTS to go
            Vector3 targetCenter = worldCenter + (direction * gridSize);

            // 3. OVERLAP BOX: We check the exact destination space, not a swept path!
            Collider[] hits = Physics.OverlapBox(targetCenter, extents, col.transform.rotation);

            foreach (Collider hit in hits)
            {
                // Ignore other triggers
                if (hit.isTrigger) continue;

                // THE FIX: IsChildOf perfectly checks if the hit object belongs to THIS block's hierarchy,
                // completely ignoring the rest of the scene!
                if (!hit.transform.IsChildOf(this.transform))
                {
                    pathBlocked = true;
                    Debug.Log($"Block piece {col.name} is blocked by {hit.name}!");
                    break;
                }
            }

            if (pathBlocked) break;
        }

        if (pathBlocked)
        {
            // Optional: Hook up an AudioSource here to play a "heavy stone clunk" error sound
            return;
        }

        // 4. The destination is completely clear for the exact Tetris shape, slide!
        Vector3 targetPosition = transform.position + (direction * gridSize);
        StartCoroutine(SlideToPosition(targetPosition));
    }

    private IEnumerator SlideToPosition(Vector3 target)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, slideSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }
}