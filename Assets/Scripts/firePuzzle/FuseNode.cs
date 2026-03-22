using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FuseNode : MonoBehaviour
{
    [Tooltip("Drag the other nodes ON THIS SAME BLOCK that connect to this one.")]
    public List<FuseNode> connectedNodes;

    [Header("Visuals")]
    [Tooltip("Drag your FuseWire Prefab here!")]
    public FuseWire wirePrefab;

    // A dictionary so the Spark can easily look up which wire connects to which node
    public Dictionary<FuseNode, FuseWire> connectingWires = new Dictionary<FuseNode, FuseWire>();

    void Start()
    {
        // Generate the physical wire visuals when the game starts
        foreach (FuseNode neighbor in connectedNodes)
        {
            if (neighbor == null) continue;

            // Simple trick to ensure we only draw ONE wire between A and B
            // Only the node with the lower Instance ID spawns it
            if (this.GetInstanceID() < neighbor.GetInstanceID())
            {
                FuseWire newWire = Instantiate(wirePrefab, transform.position, Quaternion.identity, transform.parent);
                newWire.SetupWire(this, neighbor);

                // Tell both nodes about this wire so the spark can find it later
                this.connectingWires.Add(neighbor, newWire);
                neighbor.connectingWires.Add(this, newWire);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.05f);
        if (connectedNodes != null)
        {
            foreach (FuseNode node in connectedNodes)
            {
                if (node != null) Gizmos.DrawLine(transform.position, node.transform.position);
            }
        }
    }
}