using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform playerLookAt;
    [SerializeField] private bool onlyYRotation = false;

    // Start is called before the first frame update
    void Start()
    {
        playerLookAt = PlayerController.instance.GetComponentInChildren<Camera>().transform;
    }

    // Follows the player's camera.
    private void LateUpdate()
    {
        transform.LookAt(playerLookAt, Vector3.up);
        transform.Rotate(0f, 180f, 0f);
        if (onlyYRotation)
            transform.localRotation = Quaternion.Euler(0f, transform.localRotation.eulerAngles.y, 0f);
    }
}