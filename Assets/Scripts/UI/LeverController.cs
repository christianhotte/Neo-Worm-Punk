using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LeverController : MonoBehaviour
{
    private HotteInputActions inputActions;

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

    [SerializeField, Tooltip("Lever movement speed.")] private float leverMovementSpeed = 5f;

    [SerializeField, Tooltip("The minimum numerical value of the lever.")] private float minimumValue = -1f;
    [SerializeField, Tooltip("The maximum numerical value of the lever.")] private float maximumValue = 1f;

    [Tooltip("The event called when the minimum limit of the lever is reached.")] public UnityEvent OnMinLimitReached;
    [Tooltip("The event called when the maximum limit of the lever is reached.")] public UnityEvent OnMaxLimitReached;
    [Tooltip("The event called when the lever is moved.")] public UnityEvent<float> OnValueChanged;

    private Transform pivot;

    private float previousValue, currentValue;  //The previous and current frame's value of the lever
    private HandleController handle;

    private Transform activeHandPos;

    private void Awake()
    {
        inputActions = new HotteInputActions();
        inputActions.XRILeftHandInteraction.Grip.performed += _ => GrabLever();
        inputActions.XRIRightHandInteraction.Grip.performed += _ => GrabLever();
        inputActions.XRILeftHandInteraction.Grip.canceled += _ => ReleaseLever();
        inputActions.XRIRightHandInteraction.Grip.canceled += _ => ReleaseLever();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        handle = GetComponentInChildren<HandleController>();
        handle.MoveToAngle(startingAngle);
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void SetHandParent(Transform hand)
    {
        activeHandPos = hand;
    }

    private void RemoveHandParent()
    {
        activeHandPos = null;
    }

    private void GrabLever() => handle.StartGrabLever();
    private void ReleaseLever() => handle.StopGrabLever();

    private void FixedUpdate()
    {
        //If there is an active level transition, don't do anything
        if (GameManager.Instance != null && GameManager.Instance.levelTransitionActive)
            return;

        //If the lever is not locked, check its angle
        if (!isLocked)
        {
            float angleWithMinLimit = Mathf.Abs(handle.GetAngle() - minimumAngle);
            float angleWithMaxLimit = Mathf.Abs(handle.GetAngle() - maximumAngle);

            //If the angle has hit the minimum limit and is not already at the limit
            if (angleWithMinLimit < angleBetweenThreshold)
            {
                if (hingeJointState != HingeJointState.Min)
                {
                    Debug.Log(transform.name + " Minimum Limit Reached.");
                    OnMinLimitReached.Invoke();

                    //Move the hinge to the upper limit
                    handle.transform.localEulerAngles = new Vector3(minimumAngle, handle.transform.localEulerAngles.y, handle.transform.localEulerAngles.z);

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
                if (hingeJointState != HingeJointState.Max)
                {
                    Debug.Log(transform.name + " Maximum Limit Reached.");
                    OnMaxLimitReached.Invoke();

                    //Move the hinge to the lower limit
                    handle.transform.localEulerAngles = new Vector3(maximumAngle, handle.transform.localEulerAngles.y, handle.transform.localEulerAngles.z);


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
        if (currentValue != previousValue)
        {
            OnValueChanged.Invoke(currentValue);
            previousValue = currentValue;
        }
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
    public float GetLeverMovementSpeed() => leverMovementSpeed;
}
