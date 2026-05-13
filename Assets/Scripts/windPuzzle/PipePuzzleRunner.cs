using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PipePuzzleRunner : MonoBehaviour
{
    [Header("Puzzle Setup")]
    [Tooltip("The distance between the center of each pipe/grid tile.")]
    public float gridSpacing = 1f;

    [Header("Puzzle Events")]
    public UnityEvent OnPuzzleSolved;
    public UnityEvent OnPuzzleFailed;

    // The physical map of all pipes in the room
    private Dictionary<Vector2Int, Pipe> pipeGrid = new Dictionary<Vector2Int, Pipe>();

    // Called when the player pulls the Master Lever
    public void StartPuzzle()
    {

        // 1. Build a fresh map of the room every time the lever is pulled!
        BuildSpatialGrid();

        // 2. Find all Inputs and Outputs in the scene automatically
        PipeInput[] allInputs = FindObjectsByType<PipeInput>(FindObjectsSortMode.None);
        PipeOutput[] allOutputs = FindObjectsByType<PipeOutput>(FindObjectsSortMode.None);


        // Reset all outputs to false before checking
        foreach (PipeOutput output in allOutputs) output.isSatisfied = false;

        // 3. Shoot air out of every input (Air pitch starts at 0)
        foreach (PipeInput input in allInputs)
        {
            Pipe startPipe = input.GetComponent<Pipe>();
            Search(startPipe, new HashSet<Pipe>(), 0, Vector3.zero);
        }

        // 4. Check if ALL outputs got what they needed
        bool allSolved = true;
        if (allOutputs.Length == 0) allSolved = false; // Failsafe

        foreach (PipeOutput output in allOutputs)
        {
            if (!output.isSatisfied) allSolved = false;
        }

        // Trigger the final events!
        if (allSolved)
        {
            Debug.Log("PUZZLE SOLVED! All flutes are playing the correct notes!");
            if (OnPuzzleSolved != null) OnPuzzleSolved.Invoke();
        }
        else
        {
            Debug.Log("Puzzle Failed. Air routed to the wrong place or playing the wrong pitch.");
            if (OnPuzzleFailed != null) OnPuzzleFailed.Invoke();
        }
    }

    // Scans the room and maps pipes to physical coordinates
    private void BuildSpatialGrid()
    {
        pipeGrid.Clear();
        Pipe[] allPipesInScene = FindObjectsByType<Pipe>(FindObjectsSortMode.None);

        foreach (Pipe pipe in allPipesInScene)
        {
            // If the pipe is an item, make sure the player isn't currently carrying it!
            PickableItem pickable = pipe.GetComponent<PickableItem>();
            if (pickable != null && pickable.IsHeld()) continue;

            // Map its physical position to a clean grid coordinate
            int x = Mathf.FloorToInt(pipe.transform.position.x / gridSpacing);
            int z = Mathf.FloorToInt(pipe.transform.position.z / gridSpacing);
            Vector2Int coord = new Vector2Int(x, z);

            // Add it to our routing map
            if (!pipeGrid.ContainsKey(coord))
            {
                pipe.ClearLeaks();
                pipeGrid.Add(coord, pipe);
            }
        }
    }

    // The Recursive Branching Search
    private void Search(Pipe currentPipe, HashSet<Pipe> visited, int currentPitch, Vector3 entryPort)
    {
        HashSet<Pipe> currentVisited = new HashSet<Pipe>(visited);
        currentVisited.Add(currentPipe);

        // Math Modifier Logic
        if (currentPipe.pipeColor == Pipe.PipeColor.Red) currentPitch++;
        else if (currentPipe.pipeColor == Pipe.PipeColor.Blue) currentPitch--;

        // Did we hit an output flute?
        PipeOutput output = currentPipe.GetComponent<PipeOutput>();
        if (output != null)
        {
            output.Reached(currentPitch);
            if (currentPitch == output.targetNoteValue) output.isSatisfied = true;
            return; // Air stops here
        }

        // Look for neighbors based on our physical location
        int currentX = Mathf.FloorToInt(currentPipe.transform.position.x / gridSpacing);
        int currentZ = Mathf.FloorToInt(currentPipe.transform.position.z / gridSpacing);
        Vector2Int currentCoord = new Vector2Int(currentX, currentZ);

        foreach (Vector3 openPort in currentPipe.GetWorldPorts())
        {
            if (entryPort != Vector3.zero && Vector3.Distance(openPort, entryPort) < 0.1f) continue;

            Vector2Int neighborOffset = new Vector2Int(Mathf.RoundToInt(openPort.x), Mathf.RoundToInt(openPort.z));
            Vector2Int neighborCoord = currentCoord + neighborOffset;

            if (pipeGrid.TryGetValue(neighborCoord, out Pipe neighborPipe))
            {
                if (currentVisited.Contains(neighborPipe)) continue;

                // Does the neighbor pipe have a port facing back at us?
                Vector3 directionBackToUs = -openPort;
                bool portsConnect = false;

                foreach (Vector3 neighborPort in neighborPipe.GetWorldPorts())
                {
                    // If the vectors are pointing in almost exactly the same direction, accept it!
                    if (Vector3.Distance(neighborPort, directionBackToUs) < 0.1f)
                    {
                        portsConnect = true;
                        break;
                    }
                }

                if (portsConnect)
                {
                    // Branch into the next pipe!
                    // We pass 'directionBackToUs' so the next pipe knows where the air came from
                    Search(neighborPipe, currentVisited, currentPitch, directionBackToUs);
                }
                else
                {
                    // A pipe is there, but its hole doesn't line up. LEAK!
                    currentPipe.LeakAir(openPort, gridSpacing);
                }
            }
            else
            {
                // No valid neighbor pipe found. LEAK!
                currentPipe.LeakAir(openPort, gridSpacing);
            }
        }
    }
}