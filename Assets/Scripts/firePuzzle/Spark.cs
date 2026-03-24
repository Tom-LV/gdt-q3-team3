using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spark : MonoBehaviour
{
    public float burnSpeed = 3f;

    private FuseNode currentNode;
    private FuseNode previousNode; // Prevents the spark from turning around

    public void Ignite(FuseNode startNode)
    {
        transform.position = startNode.transform.position;
        currentNode = startNode;
        previousNode = null;

        gameObject.SetActive(true);
        StartCoroutine(BurnRoutine());
    }

    // Called by clones when the fuse hits a junction and splits
    public void Branch(FuseNode fromNode, FuseNode toNode)
    {
        transform.position = fromNode.transform.position;
        gameObject.SetActive(true);

        // Clear the trail renderer
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.Clear();

        // Start traveling down the branched path
        StartCoroutine(TravelAndContinue(fromNode, toNode));
    }

    private IEnumerator BurnRoutine()
    {
        currentNode.OnSparkReached();
        // 1. Find all valid paths
        List<FuseNode> validNextNodes = new List<FuseNode>();

        // Check internal doubly-linked connections
        foreach (FuseNode node in currentNode.connectedNodes)
        {
            // Don't go back the way we came
            if (node != null && node != previousNode)
            {
                validNextNodes.Add(node);
            }
        }

        // Check for external connections (overlapping nodes on adjacent Tetris blocks)
        Collider[] hits = Physics.OverlapSphere(currentNode.transform.position, 0.2f);
        foreach (Collider hit in hits)
        {
            FuseNode externalNode = hit.GetComponent<FuseNode>();

            // If found a node, and it's not the one we are on, and we didn't just come from it
            if (externalNode != null && externalNode != currentNode && externalNode != previousNode)
            {
                // Ensure we don't double-count, and make sure it's actually on a DIFFERENT block
                if (!validNextNodes.Contains(externalNode) && externalNode.transform != currentNode.transform)
                {
                    validNextNodes.Add(externalNode);
                }
            }
        }

        // Decide what to do based on paths found
        if (validNextNodes.Count == 0)
        {
            Debug.Log("Reached a dead end.");
            Extinguish();
            yield break; // Stop the coroutine
        }

        // Split the spark if there are multiple paths
        for (int i = 1; i < validNextNodes.Count; i++)
        {
            // Spawn a clone for every extra path
            Spark splitSpark = Instantiate(this.gameObject, transform.position, transform.rotation, currentNode.transform.parent).GetComponent<Spark>();
            splitSpark.Branch(currentNode, validNextNodes[i]);
        }

        // This original spark travels down the first path
        yield return StartCoroutine(TravelAndContinue(currentNode, validNextNodes[0]));
    }

    private IEnumerator TravelAndContinue(FuseNode fromNode, FuseNode toNode)
    {
        transform.SetParent(fromNode.transform.parent);

        // Check if there is a physical wire between these nodes
        FuseWire currentWire = null;
        if (fromNode.connectingWires.ContainsKey(toNode))
        {
            currentWire = fromNode.connectingWires[toNode];
        }

        // Move the spark and burn the wire
        float distance = Vector3.Distance(fromNode.transform.position, toNode.transform.position);
        float timeToTravel = distance / burnSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < timeToTravel)
        {
            elapsedTime += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsedTime / timeToTravel);

            // Move the glowing particle
            transform.position = Vector3.Lerp(fromNode.transform.position, toNode.transform.position, percent);

            // Update the LineRenderer to turn gray right behind the spark
            if (currentWire != null)
            {
                currentWire.UpdateBurnVisual(fromNode, percent);
            }

            yield return null;
        }

        // Snap to exact position at the end
        transform.position = toNode.transform.position;
        if (currentWire != null) currentWire.UpdateBurnVisual(fromNode, 1f);

        // Update state and loop
        previousNode = fromNode;
        currentNode = toNode;
        StartCoroutine(BurnRoutine());
    }

    private void Extinguish()
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.Clear();

        gameObject.SetActive(false);
    }
}