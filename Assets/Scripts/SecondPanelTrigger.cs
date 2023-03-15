using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondPanelTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("The animator for 4th Panel.")] private Animator Panel4Animator;

    private void OnTriggerEnter(Collider other)
    {
        //If the trigger collides with the player, raise the tube door
        if (other.CompareTag("Player"))
        {
            Panel4Animator.Play("Panel_4");

        }
    }
}
