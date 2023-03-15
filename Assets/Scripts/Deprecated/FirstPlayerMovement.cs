using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Code was used from https://www.youtube.com/watch?v=_QajrabyTJc

public class FirstPlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    [SerializeField] float speed = 12f;
    private Vector2 dirInput;

    // Update is called once per frame
    void Update()
    {
        Vector3 move = transform.right * dirInput.x + transform.forward * dirInput.y;

        // Moves the player
        controller.Move(move * speed * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        dirInput = context.ReadValue<Vector2>();
    }
}