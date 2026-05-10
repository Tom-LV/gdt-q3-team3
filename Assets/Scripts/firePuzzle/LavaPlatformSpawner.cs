using UnityEngine;

public class LavaPlatformSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Lava Platform Prefab to spawn.")]
    public GameObject platformPrefab;

    [Tooltip("Drag the GameObject with the LavaCurvePath script here.")]
    private Spline assignedPath;

    [Header("Spawn Settings")]
    [Tooltip("How many seconds between each platform spawning.")]
    public float spawnInterval = 4f;

    private float timer;

    void Start()
    {
        // Force a spawn immediately when the level starts
        SpawnPlatform();
        timer = 0;
        assignedPath = GetComponent<Spline>();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnPlatform();
            timer = spawnInterval;
        }
    }

    private void SpawnPlatform()
    {
        if (platformPrefab == null || assignedPath == null) return;

        // Instantiate the platform at the very first point of the curve
        GameObject newPlatform = Instantiate(platformPrefab, assignedPath.GetPoint(0f), Quaternion.identity);

        LavaPlatform script = newPlatform.GetComponent<LavaPlatform>();
        if (script != null)
        {
            // Pass the path object to the platform so it knows where to go
            script.Initialize(assignedPath);
        }
    }
}