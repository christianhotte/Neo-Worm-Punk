using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code was used from https://youtu.be/VtT6ZEcCVus?t=148

public class SpawnManager : MonoBehaviour
{
    //Objects & Components:
    /// <summary>
    /// Newest instance of spawn manager script (most likely the only instance at any given time).
    /// </summary>
    public static SpawnManager current;
    public List<Transform> spawnPoints = new List<Transform>();
    [Tooltip("The place players temporarily go to when they die.")] public Transform deathZone;
    public float deathZoneRotSpeed;
    public Transform[] spawnPoints6;
    private int lastSpawnIndex = -1;

    //Settings:

    //Runtime Variables:
    private List<Transform> usedSpawnPoints = new List<Transform>();

    //RUNTIME METHODS:
    void Awake()
    {
        current = this; //Always set newest-loaded spawnManager script to current
    }

    // Start is called before the first frame update
    void Start()
    {
        // Shuffle the spawn points array to get random order
        ShuffleArray(spawnPoints6);
    }

    private void Update()
    {
        if (deathZone != null && PlayerController.instance.isDead)
        {
            deathZone.Rotate(deathZoneRotSpeed * Time.deltaTime * Vector3.up);
        }
    }

    public Transform GetRandomSpawnPoint()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        usedSpawnPoints.Add(spawnPoint);
        spawnPoints.Remove(spawnPoint);
        
        if (spawnPoints.Count == 0)
        {
            foreach (Transform point in usedSpawnPoints) spawnPoints.Add(point);
            usedSpawnPoints = new List<Transform>();
        }

        return spawnPoint;
    }

    // Respawns a player
    public void Respawn(GameObject player)
    {
        // Find a new spawn point that hasn't been used recently
        int newSpawnIndex = GetNewSpawnIndex();
        Transform newSpawnPoint = spawnPoints6[newSpawnIndex];

        // Set player's position to the new spawn point
        player.transform.position = newSpawnPoint.position;

        // Update the last spawn index
        lastSpawnIndex = newSpawnIndex;
    }

    private int GetNewSpawnIndex()
    {
        int newIndex = Random.Range(0, spawnPoints6.Length);

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

    /*
    /// <summary>
    /// Returns lowest spawn point which a player is not currently at. Returns -1 if no points are available.
    /// </summary>
    public int GetLowestAvailablePoint()
    {

    }*/
}