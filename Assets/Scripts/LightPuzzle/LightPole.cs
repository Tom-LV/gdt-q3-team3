using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class LightPole : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How fast the pole rotates in degrees per second.")]
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] protected float collapseTime = 2f;

    [Header("References")]
    [SerializeField] private Material beamMaterial;
    [SerializeField] private Collider storedItem;
    [SerializeField] private Transform topTransform;
    [SerializeField] private Transform pivot;
    [SerializeField] protected Transform bottomTransform;
    [SerializeField] protected Transform crystalTransform;

    [Header("Events")]
    [SerializeField] private UnityEvent m_OnPlayerHit;
    [SerializeField] private UnityEvent m_OnCollapse;
    [SerializeField] private UnityEvent m_OnUncollapse;

    private LightPuzzleManager puzzleManager;
    private bool collapsed = false;
    private Transform[] otherPoles;
    private int currentAimedPole = 0;

    private float currentVisualYaw;
    private float targetVisualYaw;
    private bool beamActive = false;

    // Core State
    private LineRenderer lr;
    private Vector3 initialTopLocalPosition;

    // Update-based Transition Tracking
    private bool isTransitioning = false;
    private float transitionProgress = 0f;
    private float currentTransitionTime = 0f;
    private Vector3 startLocalPos;
    private Vector3 targetLocalPos;
    private bool activateBeamOnFinish = false;

    void Awake()
    {
        if (storedItem != null)
        {
            Collider topCol = topTransform.GetComponent<Collider>();
            if (topCol != null) Physics.IgnoreCollision(topCol, storedItem);
        }

        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = beamMaterial;
        lr.enabled = false;
    }

    void Start()
    {
        initialTopLocalPosition = topTransform.localPosition;
        if (otherPoles == null || otherPoles.Length == 0) Collapse();
    }

    public void Initialize(Transform[] positions, LightPuzzleManager manager)
    {
        puzzleManager = manager;
        if (positions == null || positions.Length == 0) return;

        // Get all other poles
        var allOtherPoles = positions.Where(t => t != transform).ToList();
        if (allOtherPoles.Count == 0) return;

        var uniqueDirectionPoles = new List<Transform>();

        foreach (var pole in allOtherPoles)
        {
            Vector3 dir = pole.position - pivot.position;
            dir.y = 0;
            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float dist = dir.magnitude;

            bool foundSimilar = false;
            for (int i = 0; i < uniqueDirectionPoles.Count; i++)
            {
                var existingPole = uniqueDirectionPoles[i];
                Vector3 existingDir = existingPole.position - pivot.position;
                existingDir.y = 0;
                float existingAngle = Mathf.Atan2(existingDir.x, existingDir.z) * Mathf.Rad2Deg;

                // Check if they share the exact same line of sight (within 1 degree)
                if (Mathf.Abs(Mathf.DeltaAngle(angle, existingAngle)) < 1f)
                {
                    foundSimilar = true;
                    // If the new pole is closer than the existing one, replace it!
                    if (dist < existingDir.magnitude)
                    {
                        uniqueDirectionPoles[i] = pole;
                    }
                    break;
                }
            }

            if (!foundSimilar)
            {
                uniqueDirectionPoles.Add(pole);
            }
        }

        // Sort the filtered poles clockwise
        Transform center = uniqueDirectionPoles.Count > 0 ? uniqueDirectionPoles[0] : null;
        otherPoles = uniqueDirectionPoles.OrderBy(t =>
        {
            Vector3 dir = t.position - transform.position;
            return Mathf.Atan2(dir.x, dir.z);
        }).ToArray();

        for (int i = 0; i < otherPoles.Length; i++)
        {
            if (center == otherPoles[i])
            {
                currentAimedPole = i;
                break;
            }
        }

        // Initialize visual rotation tracking
        Vector3 initialDir = otherPoles[currentAimedPole].position - pivot.position;
        initialDir.y = 0;
        float initialYaw = Quaternion.LookRotation(initialDir).eulerAngles.y;

        currentVisualYaw = initialYaw;
        targetVisualYaw = initialYaw;
        pivot.rotation = Quaternion.Euler(0, currentVisualYaw, 0);
    }

    public void SetTargetRotation(int amount)
    {
        if (isCollapsed()) return;
        if (otherPoles == null || otherPoles.Length == 0) return;

        // Update the index securely
        currentAimedPole += amount;
        currentAimedPole = (currentAimedPole % otherPoles.Length + otherPoles.Length) % otherPoles.Length;

        Vector3 dir = otherPoles[currentAimedPole].position - pivot.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            float newWorldYaw = Quaternion.LookRotation(dir).eulerAngles.y;

            // Force rotation direction based on input amount
            float delta = Mathf.DeltaAngle(targetVisualYaw, newWorldYaw);

            // If amount > 0 (Right), force delta to be positive.
            if (amount > 0 && delta <= 0.01f) delta += 360f;

            // If amount < 0 (Left), force delta to be negative.
            else if (amount < 0 && delta >= -0.01f) delta -= 360f;

            targetVisualYaw += delta;
        }
    }

    public void Rotate(int amount)
    {
        if (isCollapsed()) return;
        SetTargetRotation(amount);
        puzzleManager.Highlight(otherPoles[currentAimedPole]);
    }

    void Update()
    {
        // Directional Rotation
        if (!isCollapsed() && !Mathf.Approximately(currentVisualYaw, targetVisualYaw))
        {
            currentVisualYaw = Mathf.MoveTowards(currentVisualYaw, targetVisualYaw, rotationSpeed * Time.deltaTime);
            pivot.rotation = Quaternion.Euler(0, currentVisualYaw, 0);
        }

        // Handle Vertical Transition
        if (isTransitioning) HandleTransitionUpdate();

        // Beam Logic
        if (beamActive) UpdateBeam();
    }

    public void DisableBeam()
    {
        beamActive = false;
    }

    public bool isCollapsed()
    {
        return collapsed;
    }

    public virtual void Collapse()
    {
        if (isCollapsed()) return;
        collapsed = true;
        m_OnCollapse?.Invoke();

        StartTransition(bottomTransform.localPosition, true);
    }

    public virtual void Uncollapse()
    {
        if (!isCollapsed()) return;
        collapsed = false;

        beamActive = false;
        lr.enabled = false;
        m_OnUncollapse?.Invoke();

        StartTransition(initialTopLocalPosition, false);
    }

    // Setup the variables for the Update loop to process
    private void StartTransition(Vector3 targetPosition, bool activateBeam)
    {
        startLocalPos = topTransform.localPosition;
        targetLocalPos = targetPosition;
        activateBeamOnFinish = activateBeam;
        transitionProgress = 0f;

        // Calculate the actual distance needed
        float distance = Vector3.Distance(startLocalPos, targetLocalPos);
        float totalDistance = Vector3.Distance(initialTopLocalPosition, bottomTransform.localPosition);

        // Prevent division by zero if positions are identical
        currentTransitionTime = (distance / Mathf.Max(totalDistance, 0.001f)) * collapseTime;

        isTransitioning = true;
    }

    // The actual Lerp math moved into Update
    private void HandleTransitionUpdate()
    {
        transitionProgress += Time.deltaTime;

        // Protect against 0 transition time
        float lerpT = currentTransitionTime > 0f ? (transitionProgress / currentTransitionTime) : 1f;

        if (lerpT >= 1f)
        {
            // Transition Complete
            topTransform.localPosition = targetLocalPos;
            isTransitioning = false;

            if (activateBeamOnFinish)
            {
                beamActive = true;
                lr.enabled = true;
            }
        }
        else
        {
            // Still Transitioning
            topTransform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, lerpT);
        }
    }

    private void UpdateBeam()
    {
        Vector3 origin = crystalTransform.position;
        Vector3 direction = topTransform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit))
        {
            DrawBeam(origin, hit.point);

            LightPole other = hit.collider.GetComponentInParent<LightPole>();
            other?.Collapse();

            if (hit.collider.transform.root.CompareTag("Player")) m_OnPlayerHit?.Invoke();
        }
        else
        {
            DrawBeam(origin, origin + direction * 100f);
        }
    }

    public void DrawBeam(Vector3 start, Vector3 end)
    {
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}