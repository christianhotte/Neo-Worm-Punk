using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicalButtonController : MonoBehaviour
{
    [SerializeField, Tooltip("Defines how far the button needs to be move to be registered as pressed. Higher number = more sensitivity.")] private float threshold = 0.1f;    //The threshold for the button that defines when the button is pressed
    [SerializeField, Tooltip("The margin of error when the button is idle so it doesn't detect incredibly small movement.")] private float deadzone = 0.025f;   // Deadzone to ensure the button doesn't rapidly press and release 

    [SerializeField, Tooltip("If true, locks the button in place after pressing the button.")] private bool lockOnPress;
    [SerializeField, Tooltip("If true, the player can press the button to perform an action.")] private bool isInteractable = true;

    [SerializeField] private AnimationCurve buttonAniCurve;

    [SerializeField] private AudioClip onPressedSoundEffect;
    [SerializeField] private AudioClip onReleasedSoundEffect;
    [SerializeField] private AudioClip onDisabledSoundEffect;

    public UnityEvent onPressed, onReleased;

    private bool isPressed;     //Checks to make sure pressed function isn't repeatedly called
    private bool isLocked;      //Checks to see if the button is locked in place
    private Vector3 startPos;   //Start position of button
    private ConfigurableJoint joint;    //Joint to move button
    private RigidbodyConstraints buttonConstraints; //The original constraints of the button

    // Start is called before the first frame update
    void Start()
    {
        joint = GetComponentInChildren<ConfigurableJoint>();
        startPos = joint.transform.localPosition;
        buttonConstraints = joint.GetComponent<Rigidbody>().constraints;
    }

    private void OnDisable()
    {
        //If the button is unlocked, reset the position when disabling
        if (!isLocked && joint != null)
        {
            joint.transform.localPosition = startPos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //If button is not pressed and the button is past the threshold
        if (!isPressed && GetValue() + threshold >= 1)
            Pressed();

        //If the button is pressed and the button is not past the threshold
        if (isPressed && GetValue() - threshold <= 0)
            Released();

        Vector3 buttonPos = joint.transform.localPosition;
        buttonPos.z = Mathf.Clamp(buttonPos.z, startPos.z, startPos.z + joint.linearLimit.limit);
        joint.transform.localPosition = buttonPos;
    }

    private float GetValue()
    {
        //Get the distance between the starting position and the current position of the button
        float value = Vector3.Distance(startPos, joint.transform.localPosition) / joint.linearLimit.limit;

        //If the value is less than the deadzone, reset to 0
        if (Mathf.Abs(value) < deadzone)
            value = 0;

        //Clamp to prevent weird numbers
        return Mathf.Clamp(value, -1, 1);
    }

    /// <summary>
    /// Function to call UnityEvent for pressed button.
    /// </summary>
    private void Pressed()
    {
        //If the button is not locked and there's no transition active, press the button
        if (!isLocked && !GameManager.Instance.levelTransitionActive)
        {
            isPressed = true;

            //If the button is interactable
            if (isInteractable)
            {
                if (onPressedSoundEffect != null)
                    GetComponent<AudioSource>().PlayOneShot(onPressedSoundEffect, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f));

                onPressed.Invoke();
                Debug.Log(gameObject.name + " Pressed.");

                if (lockOnPress)
                    LockButton(true);
            }
            //If the button cannot be interacted with, just play a sound
            else
            {
                if (onDisabledSoundEffect != null)
                    GetComponent<AudioSource>().PlayOneShot(onDisabledSoundEffect, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f));
            }
        }
    }

    /// <summary>
    /// Locks or unlocks the button's position.
    /// </summary>
    /// <param name="locked">If true, the position of the button is locked. If false, the button can be pressed freely.</param>
    private void LockButton(bool locked)
    {
        isLocked = locked;

        if (isLocked)
        {
            //Freeze the position of the button
            joint.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ;

            //Debug.Log(gameObject.name + " Locked.");
        }
        else
        {
            //Unlock the position of the button
            joint.GetComponent<Rigidbody>().constraints = buttonConstraints;

            //Debug.Log(gameObject.name + " Unlocked.");
        }
    }

    /// <summary>
    /// Function to call UnityEvent for released button.
    /// </summary>
    private void Released()
    {
        isPressed = false;

        //If the button is interactable and level transition is not active
        if (isInteractable && !GameManager.Instance.levelTransitionActive)
        {
            if (onReleasedSoundEffect != null)
                GetComponent<AudioSource>().PlayOneShot(onReleasedSoundEffect, PlayerPrefs.GetFloat("SFXVolume", 0.5f) * PlayerPrefs.GetFloat("MasterVolume", 0.5f));

            onReleased.Invoke();
            //Debug.Log(gameObject.name + " Released.");
        }
    }

    /// <summary>
    /// Either enables or disables the button.
    /// </summary>
    /// <param name="isEnabled">If true, the button is enabled for interaction. If false, no interaction events can be called.</param>
    public void EnableButton(bool isEnabled)
    {
        isInteractable = isEnabled;
    }
}
