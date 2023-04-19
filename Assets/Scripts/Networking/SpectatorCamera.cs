using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This code was used from https://forum.unity.com/threads/free-third-person-camera-script.500496/

[RequireComponent(typeof(Camera))]
public class SpectatorCamera : MonoBehaviour
{
    private Camera cam;
    public bool autoLockCursor = true;

    // Start is called before the first frame update
    private void Awake()
    {
        cam = this.gameObject.GetComponent<Camera>();

        // Locks the mouse if you want
        Cursor.lockState = (autoLockCursor) ? CursorLockMode.Locked : CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}