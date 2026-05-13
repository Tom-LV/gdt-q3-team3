using UnityEngine;
using UnityEditor;

public class MaterialMismatchFinder : MonoBehaviour
{
    // This adds a new button to your top Unity menu bar!
    [MenuItem("Tools/Find Material Mismatches")]
    public static void FindMismatches()
    {
        int foundCount = 0;

        // 1. Check all standard Mesh Renderers (including hidden/inactive ones)
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>(true);
        foreach (MeshRenderer mr in renderers)
        {
            MeshFilter mf = mr.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                if (mr.sharedMaterials.Length > mf.sharedMesh.subMeshCount)
                {
                    // The 'mr.gameObject' at the end makes it so clicking the console log highlights the object!
                    Debug.LogWarning($"[FOUND IT] MeshRenderer mismatch on: {mr.gameObject.name}", mr.gameObject);
                    foundCount++;
                }
            }
        }

        // 2. Check all animated/Skinned Mesh Renderers
        SkinnedMeshRenderer[] skinnedRenderers = FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None);
        foreach (SkinnedMeshRenderer smr in skinnedRenderers)
        {
            if (smr.sharedMesh != null)
            {
                if (smr.sharedMaterials.Length > smr.sharedMesh.subMeshCount)
                {
                    Debug.LogWarning($"[FOUND IT] SkinnedMesh mismatch on: {smr.gameObject.name}", smr.gameObject);
                    foundCount++;
                }
            }
        }

        if (foundCount == 0)
        {
            Debug.Log("<color=green>No mismatches found in this scene!</color> Check your other scenes or prefabs.");
        }
        else
        {
            Debug.Log($"<color=red>Found {foundCount} objects with too many materials.</color>");
        }
    }
}