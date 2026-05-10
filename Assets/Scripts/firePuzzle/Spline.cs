using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{
    [Tooltip("The local points relative to this object that the platform will travel through.")]
    public List<Vector3> waypoints = new List<Vector3>() {
        new Vector3(-5, 0, 0),
        new Vector3(0, 0, 5),
        new Vector3(5, 0, 0)
    };

    // --- NEW: LOOKUP TABLE DATA ---
    [Tooltip("How many measurements to take per segment. Higher is smoother but costs slightly more memory.")]
    public int resolutionPerSegment = 20;

    private float[] distanceTable;
    public float TotalLength { get; private set; }

    private void Awake()
    {
        PrecalculatePath();
    }

    // Walks the curve once at the start of the game to measure its exact physical length
    public void PrecalculatePath()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        int totalSamples = (waypoints.Count - 1) * resolutionPerSegment;
        distanceTable = new float[totalSamples + 1];
        distanceTable[0] = 0f;
        TotalLength = 0f;

        Vector3 lastPoint = GetPoint(0f);

        for (int i = 1; i <= totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            Vector3 currentPoint = GetPoint(t);
            TotalLength += Vector3.Distance(lastPoint, currentPoint);
            distanceTable[i] = TotalLength;
            lastPoint = currentPoint;
        }
    }

    // The Platform calls this to move at a constant physical speed!
    public Vector3 GetPointAtDistance(float distance)
    {
        // 1. Edge cases
        if (distance <= 0f) return GetPoint(0f);
        if (distance >= TotalLength) return GetPoint(1f);

        // 2. Find which two measurements we are currently between in our Lookup Table
        for (int i = 0; i < distanceTable.Length - 1; i++)
        {
            if (distance >= distanceTable[i] && distance <= distanceTable[i + 1])
            {
                // We found the segment! Calculate exactly how far we are between these two samples
                float segmentLength = distanceTable[i + 1] - distanceTable[i];
                float tLocal = (distance - distanceTable[i]) / segmentLength;

                // Convert that back into our 0.0 to 1.0 Parametric Time
                float t1 = (float)i / (distanceTable.Length - 1);
                float t2 = (float)(i + 1) / (distanceTable.Length - 1);
                float finalT = Mathf.Lerp(t1, t2, tLocal);

                return GetPoint(finalT);
            }
        }

        return GetPoint(1f);
    }

    // --- ORIGINAL METHODS (Untouched, used by the Editor tool) ---

    public Vector3 GetPoint(float t)
    {
        if (waypoints == null || waypoints.Count == 0) return transform.position;
        if (waypoints.Count == 1) return transform.TransformPoint(waypoints[0]);
        if (waypoints.Count == 2) return transform.TransformPoint(Vector3.Lerp(waypoints[0], waypoints[1], t));

        t = Mathf.Clamp01(t);
        if (t == 1f) return transform.TransformPoint(waypoints[waypoints.Count - 1]);

        float progress = t * (waypoints.Count - 1);
        int i = Mathf.FloorToInt(progress);
        float localT = progress - i;

        Vector3 p0 = waypoints[Mathf.Max(i - 1, 0)];
        Vector3 p1 = waypoints[i];
        Vector3 p2 = waypoints[Mathf.Min(i + 1, waypoints.Count - 1)];
        Vector3 p3 = waypoints[Mathf.Min(i + 2, waypoints.Count - 1)];

        Vector3 localPoint = CalculateCatmullRom(localT, p0, p1, p2, p3);

        return transform.TransformPoint(localPoint);
    }

    private Vector3 CalculateCatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
    }
}