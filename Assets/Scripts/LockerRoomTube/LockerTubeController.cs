using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockerTubeController : MonoBehaviour
{
    public static List<LockerTubeController> tubes = new List<LockerTubeController>();

    [SerializeField, Tooltip("The parent that holds all of the ready lights.")] private Transform readyLights;
    internal int tubeNumber;
    internal bool occupied = false;
    internal Transform spawnPoint;

    private void Awake()
    {
        tubeNumber = int.Parse(name.Replace("TestTube", ""));
        tubes.Add(this);
        spawnPoint = transform.Find("Spawnpoint");
    }
    private void OnDestroy()
    {
        tubes.Remove(this);
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
    public static LockerTubeController GetTubeByNumber(int number)
    {
        foreach (LockerTubeController tube in tubes)
        {
            if (tube.tubeNumber == number) return tube;
        }
        Debug.LogError("Failed to get tube number " + number);
        return null;
    }
}