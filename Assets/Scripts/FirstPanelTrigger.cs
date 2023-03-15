using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPanelTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("The animator for 1st Panel.")] private Animator Panel1Animator;
    [SerializeField, Tooltip("The animator for 1st Panel.")] private Animator Panel2Animator;
    [SerializeField, Tooltip("The animator for 1st Panel.")] private Animator Panel3Animator;

    private void OnTriggerEnter(Collider other)
    {
        //If the trigger collides with the player, raise the tube door
        if (other.CompareTag("Player"))
        {
            Panel1Animator.Play("Panel_1");
            Panel2Animator.Play("Panel_2");
            Panel3Animator.Play("Panel_3");
        }
    }
}
