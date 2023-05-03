using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormDoorTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("The animator for the aperture.")] private Animator WormDoorAnimator;

    private void OnTriggerEnter(Collider other)
    {
        //If the trigger collides with the player, raise the tube door
        if (other.CompareTag("Player"))
        {
            WormDoorAnimator.SetBool("Locked", false);
        }
    }
}
