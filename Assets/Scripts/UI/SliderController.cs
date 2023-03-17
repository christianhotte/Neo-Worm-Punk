using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class SliderController : MonoBehaviour
{
    public enum SliderRestriction { NONE, LOCKED, UP, DOWN }    //Types of slider restrictions

    private SliderHandleController sliderHandle;

    [SerializeField, Tooltip("The bounds for the slider.")] private Transform boundLeft, boundRight;
    [SerializeField, Tooltip("The minimum and maximum value for the slider.")] private Vector2 outputRange;
    [SerializeField, Tooltip("The starting value for the slider.")] private float startValue;

    [SerializeField, Tooltip("Dummy models to show when grabbing the dial.")] private GameObject leftHandModel, rightHandModel;
    [SerializeField, Tooltip("Determine if a dummy model is shown when rotating the dial.")] private bool useDummyHands;

    public UnityEvent<float> OnValueChanged;    //The action performed when the slider changes
    [SerializeField, Tooltip("The sound that plays when the slider is moved.")] private AudioClip onMoveSoundEffect;

    private float offsetOnGrab; //The offset of the grabber when grabbing the slider
    private Vector3 handleLocalStartPos;    //The start position of the handle when grabbed

    [SerializeField, Tooltip("The type of restriction the slider has with its movement.")] private SliderRestriction sliderRestriction;

    private float handMovedSinceGrab;   //The distance moved since grabbing the slider
    private Vector3 handInLocalSpace;   //The hand position relative to the slider

    private void OnEnable()
    {
        sliderHandle = GetComponentInChildren<SliderHandleController>();
    }

    // Update is called once per frame
    void Update()
    {
        //If the slider is grabbed, check for hand movement
        if (sliderHandle.IsGrabbed())
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
        if (onMoveSoundEffect != null)
        {
            if (!GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().PlayOneShot(onMoveSoundEffect, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f));
            }
        }
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

        float newHandPosition = sliderHandle.transform.parent.InverseTransformPoint(sliderHandle.GetFollowObject().position).x;
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

        //If the old position is different from the new position, call the OnValueChanged function
        if(oldX != newX)
        {
            OnValueChanged.Invoke(sliderValue);
            if (onMoveSoundEffect != null)
            {
                if (!GetComponent<AudioSource>().isPlaying)
                {
                    GetComponent<AudioSource>().PlayOneShot(onMoveSoundEffect, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f));
                }
            }
        }
    }
}
