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

    //Settings:

    //Runtime Variables:
    private List<Transform> usedSpawnPoints = new List<Transform>();

    //RUNTIME METHODS:
    void Awake()
    {
        current = this; //Always set newest-loaded spawnManager script to current
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
    /*
    /// <summary>
    /// Returns lowest spawn point which a player is not currently at. Returns -1 if no points are available.
    /// </summary>
    public int GetLowestAvailablePoint()
    {

    }*/
}