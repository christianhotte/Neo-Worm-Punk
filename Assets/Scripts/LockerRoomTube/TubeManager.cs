using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeManager : MonoBehaviour
{
    public List<LockerTubeController> roomTubes;

    // -1 because it goes by index
    public LockerTubeController GetTubeByNumber(int number)
    {
        if (roomTubes.Count > 0)
            return roomTubes[number];

        Debug.LogError("Failed to get tube number " + number + " | Tube count = " + roomTubes.Count);
        return null;
    }

    public void OnLeverStateChanged()
    {
        if (ReadyUpManager.instance != null)
            ReadyUpManager.instance.LeverStateChanged();
    }
}