using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoJoinRoom : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        // For debugging purposes. Drag & drop this script to anything in the scene to auto join a room.
        NetworkManagerScript.instance.joinRoomOnLoad = true;
    }
}