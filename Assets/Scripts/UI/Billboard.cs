using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform playerLookAt;

    // Start is called before the first frame update
    void Start()
    {
        playerLookAt = FindObjectOfType<PlayerController>().GetComponentInChildren<Camera>().transform;
    }

    // Follows the player's camera.
    private void LateUpdate()
    {
        transform.LookAt(playerLookAt, Vector3.up);
        transform.Rotate(0f, 180f, 0f);
    }
}