using UnityEngine;

public class LightPuzzleManager : MonoBehaviour
{
    [Tooltip("CenterPole goes at index 0.")]
    [SerializeField] private LightPole[] puzzleElements;
    private int interactOutlineLayer = 0;
    private LightPole highlighted;
    private float highlightTimer;

    void Awake()
    {
        interactOutlineLayer = LayerMask.NameToLayer("InteractOutline");
        // CenterPole goes at index 0
        Transform[] polePositions = new Transform[puzzleElements.Length];

        for (int i = 0; i < puzzleElements.Length; i++)
        {
            polePositions[i] = puzzleElements[i].transform;
        }

        foreach (LightPole pole in puzzleElements)
        {
            pole.Initialize(polePositions, this);
        }
    }

    public bool PuzzleSolved()
    {
        foreach (LightPole pole in puzzleElements)
        {
            if(!pole.isCollapsed()) return false;
        }
        return true;
    }

    public void Highlight(Transform polePosition)
    {
        LightPole closestPole = null;
        float currentDistance = 100000000f;
        foreach(LightPole pole in puzzleElements)
        {
            float newDistance = Vector3.Distance(pole.transform.position, polePosition.transform.position);
            if(newDistance < currentDistance)
            {
                currentDistance = newDistance;
                closestPole = pole;
            }  
        }
        UnHighlightLayer(highlighted);
        HighlightLayer(closestPole);
        highlightTimer = 0f;
    }
    void Update()
    {
        if(highlighted != null) highlightTimer += Time.deltaTime;
        if(highlightTimer > 1f) UnHighlightLayer(highlighted);
    }

    private void HighlightLayer(LightPole pole)
    {
        highlighted = pole;
        highlightTimer = 0f;
        SetLayerRecursively(pole, interactOutlineLayer);
    }

    private void UnHighlightLayer(LightPole pole)
    {
        if(highlighted == null) return;
        SetLayerRecursively(pole, 0);
        highlighted = null;
    }
    private void SetLayerRecursively(LightPole obj, int newLayer)
    {
        Transform[] allChildren = obj.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            child.gameObject.layer = newLayer;
        }
    }
}