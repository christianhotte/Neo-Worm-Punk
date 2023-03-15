using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class LockController : MonoBehaviour
{
    [SerializeField, Tooltip("The object required to unlock the lock.")] private GameObject keyToUnlock;
    [Tooltip("Event for when lock is unlocked.")] public UnityEvent<bool> OnUnlocked;

    private XRSocketInteractor socket => GetComponent<XRSocketInteractor>();

    private void OnEnable()
    {
        //Subscribe events to enter and exit listeners
        socket.selectEntered.AddListener(OnItemEntered);
        socket.selectExited.AddListener(OnItemExit);
    }

    private void OnDisable()
    {
        //Remove events to enter and exit listeners
        socket.selectEntered.RemoveListener(OnItemEntered);
        socket.selectExited.RemoveListener(OnItemExit);
    }

    /// <summary>
    /// Event for when an item enters the lock.
    /// </summary>
    /// <param name="args"></param>
    public void OnItemEntered(SelectEnterEventArgs args)
    {
        //If the current object inside of the lock is the key, unlock
        if (args.interactableObject.transform.gameObject == keyToUnlock)
        {

            OnUnlocked.Invoke(true);
        }
    }

    /// <summary>
    /// Event for when an item exits the lock.
    /// </summary>
    /// <param name="args"></param>
    public void OnItemExit(SelectExitEventArgs args)
    {
        //If the current object inside of the lock is the key, unlock
        if (args.interactableObject.transform.gameObject == keyToUnlock)
        {
            OnUnlocked.Invoke(false);
        }
    }
}
