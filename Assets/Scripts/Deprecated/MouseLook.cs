using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script was used from https://www.youtube.com/watch?v=_QajrabyTJc

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 180f;
    public Transform playerBody;
    float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Gets user input to get the camera to look where the mouse is moving.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Clamping the xRotation so we cannot overrotate and look behind the player.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // Rotates the player's body to where you are looking.
        playerBody.Rotate(Vector3.up * mouseX);
    }
}