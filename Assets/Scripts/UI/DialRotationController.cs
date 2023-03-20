using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class DialRotationController : GrabbableUI
{
    [SerializeField, Tooltip("The number of degrees to rotate the dial on one click.")] private int snapRotationAmount = 25;
    [SerializeField, Tooltip("The minimum amount that the player must rotate the dial to start registering input.")] private float angleTolerance;

    [SerializeField] private float minVal;
    [SerializeField] private float maxVal;

    [SerializeField, Tooltip("Dummy models to show when grabbing the dial.")] private GameObject leftHandModel, rightHandModel;
    [SerializeField, Tooltip("Determine if a dummy model is shown when rotating the dial.")] private bool useDummyHands;

    [SerializeField, Tooltip("The event called when the dial has been rotated, sends the angle rotation.")] private UnityEvent<float> OnValueChanged;
    [SerializeField, Tooltip("The sound that plays when the dial clicks into a position.")] private AudioClip onSnapSoundEffect;

    private Transform dialTransform;    //The dial transform, what needs to be rotated
    private float startAngle;   //The starting angle for the dial
    private bool requiresStartAngle = true; //Requires the dial to be at the start angle to rotate
    private bool shouldGetHandRotation = false; //If the dial should be checking for hand rotation

    // Start is called before the first frame update
    void Start()
    {
        dialTransform = transform.Find("Dial");
    }

    public override void OnGrab()
    {
        base.OnGrab();

        if (isGrabbed)
        {
            //Start checking for hand rotation
            shouldGetHandRotation = true;
            startAngle = 0f;

            HandModelVisibility(true);  //Show the dummy hand model if applicable
        }
    }

    public override void OnRelease()
    {
        base.OnRelease();

        if (!isGrabbed)
        {
            //Stop checking for hand rotation
            shouldGetHandRotation = false;
            requiresStartAngle = true;

            HandModelVisibility(false); //Hide the dummy hand model if applicable
        }
    }

    /// <summary>
    /// Determine whether to show hands when turning the dial.
    /// </summary>
    /// <param name="visibilityState">Whether the dummy hands should be visible when grabbing the dial.</param>
    private void HandModelVisibility(bool visibilityState)
    {
        if (!useDummyHands)
            return;
/*
        //Show a different dummy hand depending on the player hand being used
        if (interactor.CompareTag("RightHand"))
            rightHandModel.SetActive(visibilityState);
        else
            leftHandModel.SetActive(visibilityState);*/
    }

    // Update is called once per frame
    void Update()
    {
        //If the dial should be getting rotation
        if (shouldGetHandRotation && followObject != null)
        {
            float rotationAngle = GetInteractorRotation();
            GetRotationDistance(rotationAngle);
        }
    }

    /// <summary>
    /// Gets current rotation of our controller.
    /// </summary>
    /// <returns></returns>
    public float GetInteractorRotation() => (followObject.eulerAngles.z > 180) ? followObject.eulerAngles.z - 360 : followObject.eulerAngles.z;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentAngle">Current controller rotation.</param>
    private void GetRotationDistance(float currentAngle)
    {
        if (!requiresStartAngle)
        {
            Debug.Log("Start Angle - " + startAngle + " | Current Angle - " + currentAngle);

            float angleDifference = Mathf.Abs(startAngle - currentAngle);   //The angle difference between the start angle and the current angle to see how much it's moved

            if(angleDifference > angleTolerance)
            {
                if(angleDifference > 270f) //Check to see if the user has gone from 0 to 360 degrees
                {
                    float angleCheck;

                    //Checking clockwise movement
                    if(startAngle < currentAngle)
                    {
                        angleCheck = CheckAngle(currentAngle, startAngle);

                        if (angleCheck < angleTolerance)
                            return;
                        else
                        {
                            RotateDialClockwise();
                            startAngle = currentAngle;
                        }
                    }
                    //Checking counter-clockwise movement
                    else if(startAngle > currentAngle)
                    {
                        angleCheck = CheckAngle(currentAngle, startAngle);
                        if (angleCheck < angleTolerance)
                            return;
                        else
                        {
                            RotateDialCounterClockwise();
                            startAngle = currentAngle;
                        }
                    }
                }
                //If not, just check the angle normally
                else
                {
                    //Clockwise movement
                    if(startAngle < currentAngle)
                    {
                        RotateDialCounterClockwise();
                        startAngle = currentAngle;
                    }
                    //Counter-clockwise movement
                    else if(startAngle > currentAngle)
                    {
                        RotateDialClockwise();
                        startAngle = currentAngle;
                    }
                }
            }
        }
        else
        {
            requiresStartAngle = false;
            startAngle = currentAngle;
        }
    }

    private float CheckAngle(float currentAngle, float startAngle) => (360f - currentAngle) + startAngle;

    /// <summary>
    /// Function to call event when the dial is rotated clockwise.
    /// </summary>
    private void RotateDialClockwise()
    {
        //If the dial is not grabbed, don't do anything
        if (!isGrabbed)
            return;

        //If there is an active level transition, don't do anything
        if (GameManager.Instance.levelTransitionActive)
            return;

        //Snap rotation of dial
        dialTransform.localEulerAngles = new Vector3(dialTransform.localEulerAngles.x, dialTransform.localEulerAngles.y + snapRotationAmount, dialTransform.localEulerAngles.z);

        Debug.Log("Rotating " + gameObject.name + " Clockwise.");
        OnValueChanged.Invoke(GetDialValue(dialTransform.localEulerAngles.y));
        if (onSnapSoundEffect != null)
            GetComponent<AudioSource>().PlayOneShot(onSnapSoundEffect, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
    }

    /// <summary>
    /// Function to call event when the dial is rotated counter-clockwise.
    /// </summary>
    private void RotateDialCounterClockwise()
    {
        //If the dial is not grabbed, don't do anything
        if (!isGrabbed)
            return;

        //If there is an active level transition, don't do anything
        if (GameManager.Instance.levelTransitionActive)
            return;

        //Snap rotation of dial
        dialTransform.localEulerAngles = new Vector3(dialTransform.localEulerAngles.x, dialTransform.localEulerAngles.y - snapRotationAmount, dialTransform.localEulerAngles.z);

        Debug.Log("Rotating " + gameObject.name + " Counter-Clockwise.");

        OnValueChanged.Invoke(GetDialValue(dialTransform.localEulerAngles.y));
        if (onSnapSoundEffect != null)
            GetComponent<AudioSource>().PlayOneShot(onSnapSoundEffect, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
    }

    private float GetDialValue(float angle)
    {
        return minVal + (angle / 360f * maxVal);
    }
}
