using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LeverController : MonoBehaviour
{
    public enum HingeJointState { Min, Max, None }

    [SerializeField, Tooltip("Angle Threshold If Limit Is Reached")] float angleBetweenThreshold = 8f;
    public HingeJointState hingeJointState = HingeJointState.None;  //The state of the hinge joint

    [SerializeField, Tooltip("If true, the lever locks when a limit is reached.")] private bool lockOnMinimumLimit, lockOnMaximumLimit;
    [SerializeField, Tooltip("If true, the lever snaps to each limit.")] private bool snapToLimit;
    [SerializeField, Tooltip("The speed in seconds in which the lever moves automatically.")] private float snapMovementSpeed = 0.5f;
    private bool leverAutomaticallyMoving = false;  //Checks to see if the lever is moving by itself
    private bool isLocked = false;  //Checks to see if the lever is locked in place

    [SerializeField, Tooltip("Minimum lever angle.")] private float minimumAngle = -45f;
    [SerializeField, Tooltip("Maximum lever angle.")] private float maximumAngle = 45f;

    [SerializeField, Tooltip("Starting angle.")] private float startingAngle = -45f;

    [SerializeField, Tooltip("The minimum numerical value of the lever.")] private float minimumValue = -1f;
    [SerializeField, Tooltip("The maximum numerical value of the lever.")] private float maximumValue = 1f;

    [Tooltip("The event called when the minimum limit of the lever is reached.")] public UnityEvent OnMinLimitReached;
    [Tooltip("The event called when the maximum limit of the lever is reached.")] public UnityEvent OnMaxLimitReached;
    [Tooltip("The event called when the lever is moved.")] public UnityEvent<float> OnValueChanged;
    [Tooltip("The event called when the lever is moved.")] public UnityEvent OnStateChanged;

    [SerializeField, Tooltip("The sound that plays when the lever is moved.")] private AudioClip onMoveSoundEffect;
    [SerializeField, Tooltip("The sound that plays when the lever reaches a limit.")] private AudioClip onClickSoundEffect;

    private float previousValue, currentValue;  //The previous and current frame's value of the lever
    private LeverHandleController handle;

    private IEnumerator leverAutoCoroutine;

    private Transform activeHandPos;

    public bool debugActivate;

    private float waitUntilAutoMoveTimer = 0.1f;
    private float currentMoveTimer;

    private bool firstCheck;    //Ignores the value check for when it's immediately spawned

    private void Start()
    {
        firstCheck = true;
        handle = GetComponentInChildren<LeverHandleController>();
        handle.SetStartVector();
    }

    private void OnEnable()
    {
        handle = GetComponentInChildren<LeverHandleController>();

        if (startingAngle != 0)
            handle.MoveToAngle(this, startingAngle);

        currentMoveTimer = waitUntilAutoMoveTimer;
    }

    private void SetHandParent(Transform hand)
    {
        activeHandPos = hand;
    }

    private void RemoveHandParent()
    {
        activeHandPos = null;
    }
    private void FixedUpdate()
    {
        if  (debugActivate)
        {
            debugActivate = false;
            handle.MoveToAngle(this, maximumAngle);
        }

        //If there is an active level transition, don't do anything
        if (GameManager.Instance != null && GameManager.Instance.levelTransitionActive)
            return;

        //If the lever is not locked, check its angle
        HingeJointState prevState = hingeJointState;
        if (!isLocked)
        {
            float angleWithMinLimit = Mathf.Abs(handle.GetAngle() - minimumAngle);
            float angleWithMaxLimit = Mathf.Abs(handle.GetAngle() - maximumAngle);

            //If the angle has hit the minimum limit and is not already at the limit
            if (angleWithMinLimit < angleBetweenThreshold)
            {
                if (hingeJointState != HingeJointState.Min && !firstCheck)
                {
                    Debug.Log(transform.name + " Minimum Limit Reached.");
                    OnMinLimitReached.Invoke();

                    //Move the hinge to the upper limit
                    handle.transform.localEulerAngles = new Vector3(minimumAngle, handle.transform.localEulerAngles.y, handle.transform.localEulerAngles.z);

                    if (onClickSoundEffect != null)
                        GetComponent<AudioSource>().PlayOneShot(onClickSoundEffect, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));

                    if (lockOnMinimumLimit)
                    {
                        LockLever(true);
                    }
                }

                hingeJointState = HingeJointState.Min;
            }
            //If the angle has hit the maximum limit and is not already at the limit
            else if (angleWithMaxLimit < angleBetweenThreshold)
            {
                if (hingeJointState != HingeJointState.Max && !firstCheck)
                {
                    Debug.Log(transform.name + " Maximum Limit Reached.");
                    OnMaxLimitReached.Invoke();

                    //Move the hinge to the lower limit
                    handle.transform.localEulerAngles = new Vector3(maximumAngle, handle.transform.localEulerAngles.y, handle.transform.localEulerAngles.z);

                    if (onClickSoundEffect != null)
                        GetComponent<AudioSource>().PlayOneShot(onClickSoundEffect, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));

                    if (lockOnMaximumLimit)
                    {
                        LockLever(true);
                    }
                }

                hingeJointState = HingeJointState.Max;
            }

            else
            {
                hingeJointState = HingeJointState.None;
            }
        }

        currentValue = GetLeverValue(); //Get the value of the lever

        //If the value has changed since the previous frame, call the OnValueChanged event
        if (currentValue != previousValue && !firstCheck)
        {
            OnValueChanged.Invoke(currentValue);
            previousValue = currentValue;
            currentMoveTimer = waitUntilAutoMoveTimer;

            if (onMoveSoundEffect != null)
            {
                if (!GetComponent<AudioSource>().isPlaying)
                {
                    GetComponent<AudioSource>().PlayOneShot(onMoveSoundEffect, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
                }
            }
        }
        else if (snapToLimit && !leverAutomaticallyMoving)
        {
            //If the hinge is not at a limit
            if(hingeJointState == HingeJointState.None)
            {
                Debug.Log("Time Until Auto Move: " + currentMoveTimer + " seconds...");
                currentMoveTimer -= Time.deltaTime;
                if(currentMoveTimer < 0)
                {
                    Debug.Log("Moving Lever...");
                    if (handle.GetAngle() < 0)
                        MoveToLimit(minimumAngle);
                    else
                        MoveToLimit(maximumAngle);
                }
            }
        }

        if (prevState != hingeJointState && !firstCheck)
        {
            Debug.Log("Lever State Changed Invoked.");
            OnStateChanged.Invoke();
        }

        if (firstCheck)
            firstCheck = false;
    }

    private void MoveToLimit(float limit)
    {
        if (leverAutoCoroutine != null)
            StopCoroutine(leverAutoCoroutine);

        leverAutoCoroutine = MoveLeverAutomatic(limit);
        StartCoroutine(leverAutoCoroutine);
    }

    private IEnumerator MoveLeverAutomatic(float newPos)
    {
        leverAutomaticallyMoving = true;
        float timeElapsed = 0f;

        while (timeElapsed < snapMovementSpeed)
        {
            float t = timeElapsed / snapMovementSpeed;

            handle.MoveToAngle(this, Mathf.Lerp(handle.GetAngle(), newPos, t));

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        handle.MoveToAngle(this, newPos);
        leverAutomaticallyMoving = false;
    }

    /// <summary>
    /// Gets the lever value based on the given minimum and maximum values.
    /// </summary>
    /// <returns>The current numerical value of the lever based on its position.</returns>
    public float GetLeverValue()
    {
        if (hingeJointState == HingeJointState.Min)
                return minimumValue;
            else if (hingeJointState == HingeJointState.Max)
                return maximumValue;

            float maxValueDistance = Mathf.Abs(minimumValue - maximumValue);
            float currentDistance = (maximumAngle - handle.GetAngle()) / (maximumAngle - minimumAngle);
            return minimumValue + (maxValueDistance * currentDistance);
    }

    /// <summary>
    /// Locks or unlocks the lever's movement.
    /// </summary>
    /// <param name="lockLever">If true, the lever is locked. If false, the lever is unlocked.</param>
    public void LockLever(bool lockLever)
    {
        isLocked = lockLever;
    }

    public float GetMinimumAngle() => minimumAngle;
    public float GetMaximumAngle() => maximumAngle;
    public HingeJointState GetLeverState() => hingeJointState;
}