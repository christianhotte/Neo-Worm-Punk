using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager6 : MonoBehaviour
{
    public Transform[] spawnPoints;
    private int lastSpawnIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        // Shuffle the spawn points array to get random order
        ShuffleArray(spawnPoints);
    }

    // Respawns a player
    public void Respawn(GameObject player)
    {
        // Find a new spawn point that hasn't been used recently
        int newSpawnIndex = GetNewSpawnIndex();
        Transform newSpawnPoint = spawnPoints[newSpawnIndex];

        // Set player's position to the new spawn point
        player.transform.position = newSpawnPoint.position;

        // Update the last spawn index
        lastSpawnIndex = newSpawnIndex;
    }

    private int GetNewSpawnIndex()
    {
        int newIndex = Random.Range(0, spawnPoints.Length);

        // If the new index is the same as the last one, get a new one
        if (newIndex == lastSpawnIndex)
        {
            newIndex = GetNewSpawnIndex();
        }

        return newIndex;
    }

    // Fisher-Yates shuffle algorithm
    private void ShuffleArray<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }
    }
}