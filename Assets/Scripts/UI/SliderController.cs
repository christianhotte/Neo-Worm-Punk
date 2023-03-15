using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class SliderController : MonoBehaviour
{
    public enum SliderRestriction { NONE, LOCKED, UP, DOWN }    //Types of slider restrictions

    private HandleController sliderHandle;

    [SerializeField, Tooltip("The bounds for the slider.")] private Transform boundLeft, boundRight;
    [SerializeField, Tooltip("The minimum and maximum value for the slider.")] private Vector2 outputRange;
    [SerializeField, Tooltip("The starting value for the slider.")] private float startValue;

    [SerializeField, Tooltip("Dummy models to show when grabbing the dial.")] private GameObject leftHandModel, rightHandModel;
    [SerializeField, Tooltip("Determine if a dummy model is shown when rotating the dial.")] private bool useDummyHands;

    public UnityEvent<float> OnValueChanged;    //The action performed when the slider changes

    private bool isGrabbed; //Whether the slider is currently grabbed by something
    private float offsetOnGrab; //The offset of the grabber when grabbing the slider
    private Vector3 handleLocalStartPos;    //The start position of the handle when grabbed

    [SerializeField, Tooltip("The type of restriction the slider has with its movement.")] private SliderRestriction sliderRestriction;

    private Transform activeGrabber;    //The active object grabbing the slider
    private float handMovedSinceGrab;   //The distance moved since grabbing the slider
    private Vector3 handInLocalSpace;   //The hand position relative to the slider

    private XRBaseInteractor interactor;    //The object interacting with the dial
    private XRGrabInteractable grabInteractor => GetComponentInChildren<XRGrabInteractable>();

    private void OnEnable()
    {
        sliderHandle = GetComponentInChildren<HandleController>();

        //Subscribe events for when the player grabs and releases the slider
        if (grabInteractor != null)
        {
            grabInteractor.selectEntered.AddListener(GrabStart);
            grabInteractor.selectExited.AddListener(GrabEnd);
        }
    }

    private void OnDisable()
    {
        //Removes events for when the player grabs and releases the slider
        if (grabInteractor != null)
        {
            grabInteractor.selectEntered.RemoveListener(GrabStart);
            grabInteractor.selectExited.RemoveListener(GrabEnd);
        }
    }

    /// <summary>
    /// The function that calls when something grabs the slider.
    /// </summary>
    /// <param name="args"></param>
    private void GrabStart(SelectEnterEventArgs args)
    {
        //Get the earliest interactor that is grabbing the slider
        interactor = (XRBaseInteractor)args.interactorObject;
        Debug.Log(args.interactorObject);
        Debug.Log(interactor);
        interactor.GetComponent<XRBaseControllerInteractor>().hideControllerOnSelect = true;

        //Scale the slider so that it matches back to the parent after being unparented
        Vector3 newScale = ReturnToScale(sliderHandle.transform.localScale);
        sliderHandle.transform.SetParent(transform);
        sliderHandle.transform.GetChild(0).localScale = newScale;
        //sliderHandle.StartGrabbing(interactor.transform); //Call the start grabbing function
    }

    /// <summary>
    /// The function that calls when something lets go of the slider.
    /// </summary>
    /// <param name="args"></param>
    private void GrabEnd(SelectExitEventArgs args)
    {
        Debug.Log("Slider Grab End");

        //sliderHandle.StopGrabbing();  //Call the stop grabbing function
    }

    private Vector3 ReturnToScale(Vector3 localScale)
    {
        Vector3 newScale = localScale;

        newScale.x = 1f / localScale.x;
        newScale.y = 1f / localScale.y;
        newScale.z = 1f / localScale.x;

        return newScale;
    }

    private void StartGrabbing(Transform grabber)
    {
        //Get the grabber and let the slider know that it is grabbed
        activeGrabber = grabber;
        isGrabbed = true;

        //Get handle start position
        handleLocalStartPos = sliderHandle.transform.localPosition;

        //Get position of grabber as if it were a child of the handle
        handInLocalSpace = sliderHandle.transform.parent.InverseTransformPoint(grabber.position);

        offsetOnGrab = handInLocalSpace.x;
    }

    // Update is called once per frame
    void Update()
    {
        //If the slider is grabbed, check for hand movement
        if (isGrabbed)
        {
            MoveToHand();
        }
        else if (GameSettings.debugMode)
        {
            DebugSliderMovement();
        }
    }

    /// <summary>
    /// Tests for slider movement when debugging the program.
    /// </summary>
    private void DebugSliderMovement()
    {
        //Clamp the handle inside of the slider
        Vector3 currentPos = sliderHandle.transform.localPosition;
        currentPos.x = Mathf.Clamp(currentPos.x, boundLeft.localPosition.x, boundRight.localPosition.x);
        sliderHandle.transform.localPosition = currentPos;

        //Get the percent completed of the slider and give it a value based on the output range
        float percentOfRange = Mathf.InverseLerp(boundLeft.localPosition.x, boundRight.localPosition.x, sliderHandle.transform.localPosition.x);
        float sliderValue = Mathf.Lerp(outputRange.x, outputRange.y, percentOfRange);

        OnValueChanged.Invoke(sliderValue);
    }

    /// <summary>
    /// Checks for the hand's position and moves the slider handle accordingly.
    /// </summary>
    public void MoveToHand()
    {
        //If there is an active level transition, don't do anything
        if (GameManager.Instance.levelTransitionActive)
            return;

        //If the slider is locked, don't move the hand
        if (sliderRestriction == SliderRestriction.LOCKED)
            return;

        float newHandPosition = sliderHandle.transform.parent.InverseTransformPoint(activeGrabber.position).x;
        handMovedSinceGrab = newHandPosition - offsetOnGrab;

        //Check the old position and the expected position of the handle
        float newX = Mathf.Clamp(handleLocalStartPos.x + handMovedSinceGrab, boundLeft.localPosition.x, boundRight.localPosition.x);
        float oldX = sliderHandle.transform.localPosition.x;

        //If the slider is locked either up or down and the player tries to move the slider, don't move it
        if (sliderRestriction == SliderRestriction.UP && newX < oldX)
            return;
        else if (sliderRestriction == SliderRestriction.DOWN && newX > oldX)
            return;

        sliderHandle.transform.localPosition = new Vector3(newX, handleLocalStartPos.y, handleLocalStartPos.z);

        //Get the percent completed of the slider and give it a value based on the output range
        float percentOfRange = Mathf.InverseLerp(boundLeft.localPosition.x, boundRight.localPosition.x, sliderHandle.transform.localPosition.x);
        float sliderValue = Mathf.Lerp(outputRange.x, outputRange.y, percentOfRange);

        OnValueChanged.Invoke(sliderValue);
    }

    private void StopGrabbing()
    {
        isGrabbed = false;
    }
}
