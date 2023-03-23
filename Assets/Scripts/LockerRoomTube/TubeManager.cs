using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeManager : MonoBehaviour
{
    public List<LockerTubeController> roomTubes;

    public LockerTubeController GetTubeByNumber(int number)
    {
        if (roomTubes.Count > 0)
            return roomTubes[number - 1];

        Debug.LogError("Failed to get tube number " + number + " | Tube count = " + roomTubes.Count);
        return null;
    }

}
