using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("The animator for the door.")] private Animator DoorAnimator;
    public UnityEvent OnDoorClose;

    public void CloseDoor()
    {
        DoorAnimator.Play("Tube_Door_Up");
        Invoke("CallInvoke", 1f);
    }

    private void CallInvoke()
    {
        OnDoorClose.Invoke();
    }
}
