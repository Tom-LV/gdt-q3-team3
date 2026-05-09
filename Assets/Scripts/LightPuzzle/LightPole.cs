using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Linq;

public class LightPole : MonoBehaviour
{
    [SerializeField] private Collider storedItem;
    [SerializeField] private Transform topTransform;
    [SerializeField] private Transform pivot;
    [SerializeField] protected Transform bottomTransform;
    [SerializeField] protected Transform crystalTransform;
    [SerializeField] protected float collapseTime = 2f;
    [SerializeField] private UnityEvent onPlayerHit;
    [SerializeField] private UnityEvent onCollapse;
    private LightPuzzleManager puzzleManager;
    private bool collapsed = false;
    private Transform[] otherPoles;
    private int currentAimedPole = 0;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float changeTime;
    private bool beamActive = false;

    public void Initialize(Transform[] positions, LightPuzzleManager manager)
    {
        puzzleManager = manager;
        if(positions == null || positions.Length == 0) return;
        otherPoles = positions.Where(t => t != transform).ToArray();
        Transform center = otherPoles[0];
        otherPoles = otherPoles.OrderBy(t =>
            {
                Vector3 dir = t.position - transform.position;
                return Mathf.Atan2(dir.x, dir.z);
                
            }).ToArray();
        for(int i = 0; i < otherPoles.Length; i++)
        {
            if(center == otherPoles[i])
            {
                currentAimedPole = i;
                break;
            }
        }
        SetTargetRotation(0);
    }
    void Start()
    {
        if (otherPoles == null || otherPoles.Length == 0) Collapse();
    }
    public void SetTargetRotation(int amount)
    {
        if(isCollapsed()) return;
        currentAimedPole += amount;
        if (currentAimedPole < 0) currentAimedPole = otherPoles.Length - 1;
        if (currentAimedPole >= otherPoles.Length) currentAimedPole = 0;
        Vector3 dir = otherPoles[currentAimedPole].position - pivot.position;
        dir.y = 0;
        startRotation = pivot.rotation;
        targetRotation = Quaternion.LookRotation(dir);
        changeTime = Time.time;
    }
    public void Rotate(int amount)
    {
        SetTargetRotation(amount);
        puzzleManager.Highlight(otherPoles[currentAimedPole]);
    }
    void Update()
    {
        if(Quaternion.Angle(startRotation,targetRotation) > 0.01f) RotateLerp();
        if(beamActive) UpdateBeam();
    }
    void RotateLerp()
    {
        if(isCollapsed()) return;
        float t = Mathf.Clamp01(Time.time - changeTime);
        pivot.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
    }
    public bool isCollapsed()
    {
        return collapsed;
    }
    public virtual void Collapse()
    {
        if(isCollapsed()) return;
        onCollapse?.Invoke();
        collapsed = true;
        targetRotation = startRotation;
        StartCoroutine(CollapseRoutine()); // this is magic to avoid lag
    }
    private IEnumerator CollapseRoutine() // aforementioned magic
    {
        float t = 0f;
        Vector3 startPos = topTransform.position;
        Vector3 endPos = bottomTransform.position;

        while (t < collapseTime)
        {
            t += Time.deltaTime;
            float lerpT = t / collapseTime;
            topTransform.position = Vector3.Lerp(startPos, endPos, lerpT);

            yield return null;
        }
        beamActive = true;
    }
    // light stuff
    private LineRenderer lr;
    private void UpdateBeam()
    {
        Vector3 origin = crystalTransform.position;
        Vector3 direction = topTransform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit))
        {
            DrawBeam(origin, hit.point);

            LightPole other = hit.collider.GetComponentInParent<LightPole>();
            other?.Collapse();
            if (hit.collider.transform.root.CompareTag("Player")) onPlayerHit?.Invoke();
        }
        else
        {
            DrawBeam(origin, origin + direction * 100f);
        }
    }
    void Awake()
    {
        if(storedItem != null) Physics.IgnoreCollision(topTransform.GetComponent<Collider>(), storedItem);

        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Unlit/Color")) {color = Color.yellow};
    }
    public void DrawBeam(Vector3 start, Vector3 end)
    {
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}
