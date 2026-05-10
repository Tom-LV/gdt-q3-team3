using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spline))]
public class SplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Spline path = (Spline)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Add New Waypoint", GUILayout.Height(30)))
        {
            Undo.RecordObject(path, "Add Waypoint");

            // Add the new point relative to the last point (Local space)
            Vector3 newLocalPos = path.waypoints.Count > 0
                ? path.waypoints[path.waypoints.Count - 1] + new Vector3(2, 0, 0)
                : Vector3.zero;

            path.waypoints.Add(newLocalPos);
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        Spline path = (Spline)target;
        if (path.waypoints == null || path.waypoints.Count < 2) return;

        // Draw a movement handle for EVERY point in the list
        for (int i = 0; i < path.waypoints.Count; i++)
        {
            // 1. Convert the local waypoint to World Space so the handle is drawn in the right spot
            Vector3 worldPos = path.transform.TransformPoint(path.waypoints[i]);

            EditorGUI.BeginChangeCheck();

            // Draw the handle
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(path, "Move Waypoint");
                // 2. Convert the dragged World position back to Local Space before saving!
                path.waypoints[i] = path.transform.InverseTransformPoint(newWorldPos);
            }
        }

        // --- DRAW THE SMOOTH RED CURVE ---
        Handles.color = Color.red;

        // GetPoint already returns World Space, so we can draw it directly!
        Vector3 lastPos = path.GetPoint(0f);

        int resolution = path.waypoints.Count * 15;
        for (int i = 1; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            Vector3 currentPos = path.GetPoint(t);
            Handles.DrawLine(lastPos, currentPos);
            lastPos = currentPos;
        }
    }
}