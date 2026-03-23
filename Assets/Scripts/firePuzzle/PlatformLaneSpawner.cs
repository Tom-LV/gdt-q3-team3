using System.Collections;
using UnityEngine;

public class PlatformLaneSpawner : MonoBehaviour
{
    [Header("Platform Settings")]
    public GameObject platformPrefab;

    [Tooltip("How fast the platforms move down this lane")]
    public float platformSpeed = 5f;

    [Tooltip("How far the platform travels before deleting itself")]
    public float distanceToDespawn = 40f;

    [Header("Spawn Timing")]
    [Tooltip("Minimum time between platforms")]
    public float minSpawnDelay = 1.5f;

    [Tooltip("Maximum time between platforms")]
    public float maxSpawnDelay = 3.5f;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Pick a random wait time and wait
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            // Spawn the platform at this spawner's exact position and rotation
            GameObject newPlatform = Instantiate(platformPrefab, transform.position, transform.rotation);

            // Setup the platform's speed and direction
            LavaPlatform platformScript = newPlatform.GetComponent<LavaPlatform>();
            if (platformScript != null)
            {
                platformScript.moveDirection = transform.forward;
                platformScript.moveSpeed = platformSpeed;

                // Calculate exactly how many seconds it takes to reach the despawn distance
                platformScript.lifeTime = distanceToDespawn / platformSpeed;
            }
        }
    }

    // Draws a line in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * distanceToDespawn);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}