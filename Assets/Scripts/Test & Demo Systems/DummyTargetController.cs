using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls objects used to test projectile hit detection and effects.
/// </summary>
public class DummyTargetController : Targetable
{
    //Objects & Components:
    private AudioSource audioSource; //Audio source component this object uses to play sounds from

    //Settings:
    [Header("Settings:")]
    [SerializeField, Tooltip("Array of points which target moves between.")]     private Transform[] movePoints;
    [SerializeField, Tooltip("Speed of target movement (in units per second).")] private float moveSpeed;

    //Runtime Variables:
    private int currentMoveIndex = 0; //Index of current move point target is moving from

    //RUNTIME METHODS:
    private protected override void Awake()
    {
        //Get objects & components:
        if (!TryGetComponent(out audioSource)) audioSource = gameObject.AddComponent<AudioSource>(); //Make sure target has an audio source component
        base.Awake();                                                                                //Call base Awake method
    }
    private void Update()
    {
        //Target movement:
        if (movePoints.Length > 1 && moveSpeed > 0) //Target is set to move on rails
        {
            //Initialization:
            float distToMove = moveSpeed * Time.deltaTime; //Get distance for target to move

            //Move to new position:
            Vector3 newPosition = transform.position; //Initialize tracker for new position at current position
            while (distToMove > 0) //Iterate until target has completed its designated distance to travel this frame
            {
                int targetMoveIndex = currentMoveIndex + 1;                                          //Initialize target index as that of next move point
                if (targetMoveIndex >= movePoints.Length) targetMoveIndex = 0;                       //Wrap index around if it has overflowed
                Transform targetPoint = movePoints[targetMoveIndex];                                 //Get target move point

                float distanceToTarget = Vector3.Distance(newPosition, targetPoint.position);
                if (distanceToTarget > distToMove) { newPosition = Vector3.MoveTowards(newPosition, targetPoint.position, distToMove); break; } //Move toward target if movement range is within current points
                else //Dummy will have to move past target point to complete motion
                {
                    currentMoveIndex++;                                              //Increment current move index
                    if (currentMoveIndex >= movePoints.Length) currentMoveIndex = 0; //Overflow current move index if necessary
                    newPosition = targetPoint.position;                              //Move position marker up to target point
                    distToMove -= distanceToTarget;                                  //Indicate that dummy has moved to target position
                }
            }
            transform.position = newPosition; //Move to new position
        }
    }

    //FUNCTIONALITY METHODS:
    public override void IsHit(int damage)
    {
        audioSource.PlayOneShot((AudioClip)Resources.Load("Sounds/Default_Hurt_Sound"), PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f)); //Play hurt sound
        print("Target hit!");                                                            //Indicate that target was hit
    }
}
