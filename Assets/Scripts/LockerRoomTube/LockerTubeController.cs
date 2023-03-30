using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LockerTubeController : MonoBehaviour
{
    private static TubeManager tubeManager;

    public static int OccupiedTubes
    {
        get
        {
            int x = 0;
            foreach (LockerTubeController tube in tubeManager.roomTubes) if (tube.occupied) x++;
            return x;
        }
    }

    [SerializeField, Tooltip("The parent that holds all of the ready lights.")] private Transform readyLights;
    [SerializeField, Tooltip("The spawn point for the player's name.")] private Transform playerNameSpawnPoint;
    [SerializeField, Tooltip("The prefab that displays the player's name.")] private GameObject playerNamePrefab;
    [SerializeField, Tooltip("The GameObject for the host settings.")] private GameObject hostSettings;

    //internal int tubeNumber;
    public bool occupied = false;
    /// <summary>
    /// ID of player which is currently in this tube.
    /// </summary>
    internal int currentPlayerID;
    internal Transform spawnPoint;

    private void Awake()
    {
        tubeManager = FindObjectOfType<TubeManager>();
        /*        tubeNumber = int.Parse(name.Replace("TestTube", ""));
                Debug.Log("Assigning Tube Number: " + tubeNumber);*/
        spawnPoint = transform.Find("Spawnpoint");
    }

    /// <summary>
    /// Updates the lights depending on whether the player is ready or not.
    /// </summary>
    /// <param name="isActivated">If true, the player is ready. If false, the player is not ready.</param>
    public void UpdateLights(bool isActivated)
    {
        foreach (var light in readyLights.GetComponentsInChildren<ReadyLightController>())
            light.ActivateLight(isActivated);
    }

    /// <summary>
    /// Spawns the player name in the tube.
    /// </summary>
    /// <param name="playerName">The name of the player.</param>
    public void SpawnPlayerName(string playerName)
    {
        //If there is not a name in the tube, add a name
        if(playerNameSpawnPoint.childCount == 0)
        {
            GameObject playerNameObject = Instantiate(playerNamePrefab, playerNameSpawnPoint);
            playerNameObject.transform.localPosition = Vector3.zero;

            playerNameObject.GetComponentInChildren<TextMeshProUGUI>().text = playerName;
        }
    }

    /// <summary>
    /// Shows the host settings in the tube.
    /// </summary>
    /// <param name="showSettings">If true, the host settings are shown. If false, they are hidden.</param>
    public void ShowHostSettings(bool showSettings)
    {
        hostSettings.SetActive(showSettings);
    }

    public int GetTubeNumber()
    {
        for(int i = 0; i < tubeManager.roomTubes.Count; i++)
        {
            if (tubeManager.roomTubes[i] == this)
                return i + 1;
        }

        Debug.LogError("Failed to get tube " + this.name + " | Tube count = " + tubeManager.roomTubes.Count);
        return -1;
    }
}