using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;

public class PhysicalToggleController : MonoBehaviour
{
    [SerializeField, Tooltip("If true, the toggle is on. If false, the toggle is off.")] private bool isOn;

    [SerializeField, Tooltip("The canvas on top of the toggle.")] private Canvas toggleCanvas;
    [SerializeField, Tooltip("The toggle transform that moves when the toggle is pressed.")] private Transform toggleTransform;

    [SerializeField, Tooltip("The angle of the toggle when it is on.")] private float toggleOnAngle;
    [SerializeField, Tooltip("The angle of the toggle when it is off.")] private float toggleOffAngle;
    [SerializeField, Tooltip("If true, locks the button in place after pressing the button.")] private bool lockOnPress;
    [SerializeField, Tooltip("If true, the player can press the button to perform an action.")] private bool isInteractable = true;

    [SerializeField, Tooltip("The cooldown for pressing the toggle.")] private float toggleCooldown;

    [SerializeField, Tooltip("The sound that plays when the toggle is pressed successfully.")] private AudioClip onPressedSoundEffect;
    [SerializeField, Tooltip("The sound that plays when the button is pressed but is disabled.")] private AudioClip onDisabledSoundEffect;

    public UnityEvent<bool> onPressed;    //The event that calls when the toggle is pressed
    private bool isPressed;     //Checks to make sure pressed function isn't repeatedly called
    private bool isPressing;
    private Collider playerPressing;

    private float currentCooldownTimer;

    private HotteInputActions inputActions;

    private void Awake()
    {
        inputActions = new HotteInputActions();
        inputActions.XRILeftHandInteraction.Grip.started += _ => OnTogglePress(InputDeviceRole.LeftHanded);
        inputActions.XRIRightHandInteraction.Grip.started += _ => OnTogglePress(InputDeviceRole.RightHanded);
        inputActions.XRILeftHandInteraction.Trigger.started += _ => OnTogglePress(InputDeviceRole.LeftHanded);
        inputActions.XRIRightHandInteraction.Trigger.started += _ => OnTogglePress(InputDeviceRole.RightHanded);
    }

    private void Start()
    {
        UpdateToggleAngle();
        isPressed = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();

        isPressed = false;
        isPressing = false;
    }

    private void OnDisable()
    {
        inputActions.Disable();

        isPressed = false;
        isPressing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            playerPressing = other;
            isPressing = true;
            OnTogglePress(playerPressing.name.Contains("LeftHand") ? InputDeviceRole.LeftHanded : InputDeviceRole.RightHanded);

            //If nothing applies, play the disabled sound effect
            if (!isInteractable)
            {
                if (onDisabledSoundEffect != null)
                    GetComponent<AudioSource>().PlayOneShot(onDisabledSoundEffect, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            isPressing = false;
            playerPressing = null;
        }
    }

    private void OnTogglePress(InputDeviceRole currentHand)
    {
        //If the button is interactable, not currently being pressed, and is not locked, press the toggle
        if (isInteractable && !isPressed && playerPressing != null)
        {
            //If the correct hand is pressing the toggle
            if(currentHand == (playerPressing.name.Contains("LeftHand") ? InputDeviceRole.LeftHanded : InputDeviceRole.RightHanded))
            {
                PlayerController.instance.SendHapticImpulse(currentHand, new Vector2(0.5f, 0.1f));
                Pressed();
            }
        }
    }

    /// <summary>
    /// Function to call UnityEvent for pressed toggle.
    /// </summary>
    private void Pressed()
    {
        //If the button is not locked and there's no transition active, press the toggle
        if (isInteractable && !GameManager.Instance.levelTransitionActive)
        {
            isPressed = true;
            isOn = !isOn;

            UpdateToggleAngle();

            if (onPressedSoundEffect != null)
                GetComponent<AudioSource>().PlayOneShot(onPressedSoundEffect, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));

            onPressed.Invoke(isOn);
            //Debug.Log(gameObject.name + " Pressed.");
            currentCooldownTimer = 0f;

            if (lockOnPress)
                LockToggle(true);
        }
    }

    private void Update()
    {
        //Cooldown timer to unpress the toggle when the toggle is pressed
        if (isPressed)
        {
            if (currentCooldownTimer > toggleCooldown)
                isPressed = false;

            currentCooldownTimer += Time.deltaTime;
        }
    }

    /// <summary>
    /// Updates the toggle variable manually and updates the angle.
    /// </summary>
    /// <param name="toggleOn">The new state of the toggle.</param>
    private void UpdateToggle(bool toggleOn)
    {
        isOn = toggleOn;
        UpdateToggleAngle();
    }

    /// <summary>
    /// Updates the angle of the toggle based on whether the toggle is on or off.
    /// </summary>
    private void UpdateToggleAngle()
    {
        if (isOn)
            toggleTransform.localEulerAngles = new Vector3(toggleOnAngle, 0f, 0f);
        else
            toggleTransform.localEulerAngles = new Vector3(toggleOffAngle, 0f, 0f);
    }

    /// <summary>
    /// Shows the text that is on top of the toggle.
    /// </summary>
    /// <param name="showText">If true, show the text on the toggle.</param>
    public void ShowText(bool showText) => toggleCanvas.gameObject.SetActive(showText);

    /// <summary>
    /// Locks or unlocks the toggle.
    /// </summary>
    /// <param name="locked">If true, the position of the toggle is locked. If false, the toggle can be pressed freely.</param>
    public void LockToggle(bool locked) => isInteractable = locked;
}
