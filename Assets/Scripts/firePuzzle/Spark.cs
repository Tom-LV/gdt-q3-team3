using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spark : MonoBehaviour
{
    public float burnSpeed = 3f;

    private FuseNode currentNode;
    private FuseNode previousNode; // Prevents the spark from turning around!

    // Call this from the Magnifying Glass interaction
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

        // CRITICAL: Clear the trail renderer so the new clone doesn't 
        // draw an ugly line from its spawn origin!
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.Clear();

        // Start traveling down the branched path
        StartCoroutine(TravelAndContinue(fromNode, toNode));
    }

    private IEnumerator BurnRoutine()
    {
        // 1. Find all valid paths we can take from here
        List<FuseNode> validNextNodes = new List<FuseNode>();

        // Check internal doubly-linked connections (assuming your FuseNode has a list called 'connectedNodes')
        foreach (FuseNode node in currentNode.connectedNodes)
        {
            // Don't go back the way we came!
            if (node != null && node != previousNode)
            {
                validNextNodes.Add(node);
            }
        }

        // 2. Check for external connections (overlapping nodes on adjacent Tetris blocks)
        Collider[] hits = Physics.OverlapSphere(currentNode.transform.position, 0.2f);
        foreach (Collider hit in hits)
        {
            FuseNode externalNode = hit.GetComponent<FuseNode>();

            // If we found a node, and it's not the one we are on, and we didn't just come from it
            if (externalNode != null && externalNode != currentNode && externalNode != previousNode)
            {
                // Ensure we don't double-count, and make sure it's actually on a DIFFERENT block!
                // (transform.root gets the top-most parent, i.e., the Tetris Block)
                if (!validNextNodes.Contains(externalNode) && externalNode.transform != currentNode.transform)
                {
                    validNextNodes.Add(externalNode);
                }
            }
        }

        // 3. Decide what to do based on paths found
        if (validNextNodes.Count == 0)
        {
            Debug.Log("The fuse fizzled out! Reached a dead end.");
            Extinguish();
            yield break; // Stop the coroutine
        }

        // 4. Split the spark if there are multiple paths (T-Junctions)
        for (int i = 1; i < validNextNodes.Count; i++)
        {
            // Spawn a clone for every extra path
            Spark splitSpark = Instantiate(this.gameObject, transform.position, transform.rotation).GetComponent<Spark>();
            splitSpark.Branch(currentNode, validNextNodes[i]);
        }

        // 5. This original spark travels down the first path
        yield return StartCoroutine(TravelAndContinue(currentNode, validNextNodes[0]));
    }

    private IEnumerator TravelAndContinue(FuseNode fromNode, FuseNode toNode)
    {
        transform.SetParent(fromNode.transform.root);

        // 1. Check if there is a physical wire between these nodes (Internal connections)
        FuseWire currentWire = null;
        if (fromNode.connectingWires.ContainsKey(toNode))
        {
            currentWire = fromNode.connectingWires[toNode];
        }

        // 2. Move the spark and burn the wire!
        float distance = Vector3.Distance(fromNode.transform.position, toNode.transform.position);
        float timeToTravel = distance / burnSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < timeToTravel)
        {
            elapsedTime += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsedTime / timeToTravel);

            // Move the glowing particle
            transform.position = Vector3.Lerp(fromNode.transform.position, toNode.transform.position, percent);

            // Update the LineRenderer to turn gray right behind the spark!
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