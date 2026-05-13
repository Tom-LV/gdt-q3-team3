using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public enum PipeColor { Normal, Red, Blue }

    [Header("Pipe Properties")]
    [Tooltip("What color is this pipe?")]
    public PipeColor pipeColor = PipeColor.Normal;

    [Header("Pipe Connections")]
    public Vector3[] localPorts;

    [Header("Debug Visuals")]
    [Tooltip("Draw lines in the Scene view to show where this pipe is pointing?")]
    public bool showDebugLines = true;
    [Tooltip("How long should the debug lines be?")]
    public float debugLineLength = 0.5f;

    public List<Vector3> GetWorldPorts()
    {
        List<Vector3> worldPorts = new List<Vector3>();
        foreach (Vector3 port in localPorts)
        {
            Vector3 worldDir = transform.TransformDirection(port);
            worldDir.x = Mathf.Round(worldDir.x);
            worldDir.y = 0;
            worldDir.z = Mathf.Round(worldDir.z);
            worldPorts.Add(worldDir.normalized);
        }
        return worldPorts;
    }

    private void OnDrawGizmos()
    {
        // Only draw if the toggle is checked and we have ports to draw!
        if (!showDebugLines || localPorts == null || localPorts.Length == 0) return;

        // Color-code the debug lines based on the pipe's assigned color
        switch (pipeColor)
        {
            case PipeColor.Red: Gizmos.color = Color.red; break;
            case PipeColor.Blue: Gizmos.color = Color.cyan; break;
            default: Gizmos.color = Color.green; break;
        }

        Vector3 center = transform.position;
        center.y += 0.2f;

        // Draw a line for every active world port
        foreach (Vector3 portDirection in GetWorldPorts())
        {
            Vector3 endPoint = center + (portDirection * debugLineLength);

            // Draw the line
            Gizmos.DrawLine(center, endPoint);

            // Draw a little sphere at the tip to make the direction obvious
            Gizmos.DrawSphere(endPoint, 0.1f);
        }
    }
}
