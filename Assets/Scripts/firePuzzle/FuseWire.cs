using UnityEngine;

public class FuseWire : MonoBehaviour
{
    [Header("Visuals")]
    public Color unburntColor = new Color(0.8f, 0.7f, 0.5f);
    public Color burntColor = new Color(0.2f, 0.2f, 0.2f);
    public float lineWidth = 0.05f;

    public Material lineMaterial;

    private LineRenderer burntLine;
    private LineRenderer unburntLine;

    public FuseNode nodeA;
    public FuseNode nodeB;

    private float currentBurnPct = 0f;
    private bool burningFromA = true;

    void Awake()
    {
        // Automatically create the two line segments when the game starts
        burntLine = CreateLineSegment("Burnt Segment", burntColor);
        unburntLine = CreateLineSegment("Unburnt Segment", unburntColor);
    }

    private LineRenderer CreateLineSegment(string name, Color color)
    {
        // Create a child object to hold the line
        GameObject child = new GameObject(name);
        child.transform.SetParent(this.transform);

        LineRenderer lr = child.AddComponent<LineRenderer>();
        lr.useWorldSpace = true; // Crucial so it tracks moving blocks
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = 2;

        // Apply material
        lr.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));

        // Set to a solid color
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        lr.colorGradient = grad;

        return lr;
    }

    public void SetupWire(FuseNode a, FuseNode b)
    {
        nodeA = a;
        nodeB = b;
        UpdateBurnVisual(a, 0f);
    }

    public void UpdateBurnVisual(FuseNode startNode, float percentage)
    {
        burningFromA = (startNode == nodeA);
        currentBurnPct = Mathf.Clamp01(percentage);
        UpdateLineGeometry();
    }

    void Update()
    {
        if (nodeA != null && nodeB != null)
        {
            UpdateLineGeometry();
        }
    }

    private void UpdateLineGeometry()
    {
        Vector3 posA = nodeA.transform.position;
        Vector3 posB = nodeB.transform.position;

        // Calculate the exact center of the spark particle
        Vector3 sparkPos = burningFromA ?
            Vector3.Lerp(posA, posB, currentBurnPct) :
            Vector3.Lerp(posB, posA, currentBurnPct);

        // Draw the Burnt path (From the Starting Node to the Spark)
        burntLine.SetPosition(0, burningFromA ? posA : posB);
        burntLine.SetPosition(1, sparkPos);

        // Draw the Unburnt path (From the Spark to the End Node)
        unburntLine.SetPosition(0, sparkPos);
        unburntLine.SetPosition(1, burningFromA ? posB : posA);
    }
}