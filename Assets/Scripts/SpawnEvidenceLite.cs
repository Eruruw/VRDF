using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEvidenceLite : MonoBehaviour
{
    public GameObject[] prefabsToSpawn; // Array of prefabs to choose from
    public float spawnChance = 0.4f; // Chance of spawning an instance
    private int currentInstances = 0;
    private BoxCollider triggerCollider; // Reference to the BoxCollider component
    private List<int> spawnedPrefabIndices = new List<int>(); // List to keep track of spawned prefab indices

    // Start is called before the first frame update
    void Start()
    {
        triggerCollider = GetComponent<BoxCollider>(); // Get the BoxCollider component
        CreateInstances();
    }

    private void CreateInstances()
    {
            if (Random.value < spawnChance) // Check if spawn chance succeeds or min instances not reached
            {
                if (prefabsToSpawn.Length > 0) // Check if there are prefabs to spawn
                {
                    int randomPrefabIndex = GetRandomPrefabIndex(); // Get a random prefab index

                    if (randomPrefabIndex != -1) // Check if a valid index is obtained
                    {
                        GameObject prefabToSpawn = prefabsToSpawn[randomPrefabIndex];

                        // Spawn the chosen prefab
                        SpawnObject(prefabToSpawn);

                        currentInstances++;
                    }
                }
            }
    }

    private int GetRandomPrefabIndex()
    {
        List<int> availableIndices = new List<int>(); // List to store available prefab indices

        // Populate available indices with all indices initially
        for (int i = 0; i < prefabsToSpawn.Length; i++)
        {
            availableIndices.Add(i);
        }

        // Remove indices that have already been spawned
        foreach (int index in spawnedPrefabIndices)
        {
            availableIndices.Remove(index);
        }

        // Check if there are available indices left
        if (availableIndices.Count > 0)
        {
            // Choose a random index from available indices
            int randomIndex = Random.Range(0, availableIndices.Count);
            int randomPrefabIndex = availableIndices[randomIndex];

            // Add the chosen index to spawnedPrefabIndices list
            spawnedPrefabIndices.Add(randomPrefabIndex);

            return randomPrefabIndex;
        }

        // If all prefabs have been spawned, return -1
        return -1;
    }

    private void SpawnObject(GameObject prefab)
    {
        // Random position within the trigger area
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        // Instantiate the prefab at the random position
        GameObject spawnedObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}
