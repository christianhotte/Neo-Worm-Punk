using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("The animator for the door.")] private Animator DoorAnimator;
    public UnityEvent OnDoorClose;

    private void OnTriggerEnter(Collider other)
    {
        //If the trigger collides with the player, raise the tube door
        if (other.CompareTag("Player"))
        {
            DoorAnimator.Play("Tube_Door_Up");
            OnDoorClose.Invoke();
        }
    }
}
