using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class LockController : MonoBehaviour
{
    [SerializeField, Tooltip("The object required to unlock the lock.")] private GameObject keyToUnlock;
    [Tooltip("Event for when lock is unlocked.")] public UnityEvent<GameObject, bool> OnUnlocked;
    [SerializeField, Tooltip("If true, the key is destroyed upon unlocking the lock.")] private bool destroyOnLock = false;

    private XRSocketInteractor socket => GetComponent<XRSocketInteractor>();

    private void Awake()
    {
        if(socket.interactionManager == null)
            socket.interactionManager = PlayerController.instance.GetComponentInChildren<XRInteractionManager>();
    }

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
        Debug.Log("Trying To Unlock With: " + args.interactableObject.transform.gameObject.name);

        //If the current object inside of the lock is the key, unlock
        if (args.interactableObject.transform.gameObject.name.Contains(keyToUnlock.name))
        {
            Debug.Log("Unlocked.");
            OnUnlocked.Invoke(args.interactableObject.transform.gameObject, true);

            if (destroyOnLock)
                Destroy(args.interactableObject.transform.gameObject);
        }
    }

    /// <summary>
    /// Event for when an item exits the lock.
    /// </summary>
    /// <param name="args"></param>
    public void OnItemExit(SelectExitEventArgs args)
    {
/*        //If the current object inside of the lock is the key, unlock
        if (args.interactableObject.transform.gameObject.name == keyToUnlock.name)
        {
            OnUnlocked.Invoke(args.interactableObject.transform.gameObject, false);
        }*/
    }
}
