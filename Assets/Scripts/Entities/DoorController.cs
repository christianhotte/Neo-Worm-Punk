using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField, Tooltip("The model for the door.")] private GameObject doorModel;
    [SerializeField, Tooltip("The position that the door moves to when open.")] private Transform openDoorPosition;
    [SerializeField, Tooltip("Is the door locked?")] private bool isLocked = false;
    [SerializeField, Tooltip("The amount of seconds it takes for the door to move.")] private float speed;
    private bool isOpen = false;    //Is the door currently open?

    private Vector3 originalPosition;   //The original position for the door
    private float timeElapsed;  //The time since the door has moved
    private IEnumerator currentDoorMovement;    //The active coroutine for the door's movement

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = doorModel.transform.localPosition;   //Get the position from the door's model
    }

    /// <summary>
    /// Opens the door.
    /// </summary>
    public void OpenDoor()
    {
        //If the door is unlocked, open the door
        if (!isLocked)
        {
            //If the door is not open, open the door
            if (!isOpen)
            {
                //Refresh the coroutine
                if (currentDoorMovement != null)
                    StopCoroutine(currentDoorMovement);

                //Reverse the time elapsed if the door is currently moving
                if (timeElapsed != 0)
                {
                    timeElapsed = speed - timeElapsed;
                }

                currentDoorMovement = DoorMovement(originalPosition, openDoorPosition.localPosition);
                StartCoroutine(currentDoorMovement);

                isOpen = true;
            }
        }
    }

    /// <summary>
    /// Closes the door.
    /// </summary>
    public void CloseDoor()
    {
        //If the door is unlocked, close the door
        if (!isLocked)
        {
            //If the door is open, close the door
            if (isOpen)
            {
                //Refresh the coroutine
                if (currentDoorMovement != null)
                    StopCoroutine(currentDoorMovement);

                //Reverse the time elapsed if the door is currently moving
                if (timeElapsed != 0)
                {
                    timeElapsed = speed - timeElapsed;
                }

                currentDoorMovement = DoorMovement(openDoorPosition.localPosition, originalPosition);
                StartCoroutine(currentDoorMovement);

                isOpen = false;
            }
        }
    }

    /// <summary>
    /// Locks or unlocks the door.
    /// </summary>
    /// <param name="locked">If true, locks the door. If false, unlocks the door.</param>
    public void LockDoor(bool locked)
    {
        isLocked = locked;
    }

    /// <summary>
    /// Move the door through a smooth lerp.
    /// </summary>
    /// <param name="startingPos">The starting position for the door.</param>
    /// <param name="endingPos">The desired position for the door.</param>
    /// <returns></returns>
    private IEnumerator DoorMovement(Vector3 startingPos, Vector3 endingPos)
    {
        while (timeElapsed < speed)
        {
            //Smooth lerp duration algorithm
            float t = timeElapsed / speed;
            t = t * t * (3f - 2f * t);

            doorModel.transform.localPosition = Vector3.Lerp(startingPos, endingPos, t);    //Lerp the door's movement

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        doorModel.transform.localPosition = endingPos;
        timeElapsed = 0;    //Reset the timer
    }

    public bool IsDoorOpen() => isOpen;
    public float GetDoorSpeed() => speed;
}
