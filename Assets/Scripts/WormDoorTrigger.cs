using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormDoorTrigger : MonoBehaviour
{
    private Animator WormDoor;

    //
    public void OnCompleteOpen()
    {
        WormDoor.SetBool("Activated", true);
    }
}
