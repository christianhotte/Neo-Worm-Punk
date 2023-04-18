using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutDoorTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("The animator for the top door.")] private Animator DoorUpperAnimator;
    [SerializeField, Tooltip("The animator for the low door.")] private Animator DoorLowerAnimator;

    private void OnTriggerEnter(Collider other)
    {
        //If the trigger collides with the player, raise the tube door
        if (other.CompareTag("Player"))
        {
            DoorUpperAnimator.SetBool("Activated", true);
            DoorLowerAnimator.SetBool("Activated", true);
        }
    }
}
