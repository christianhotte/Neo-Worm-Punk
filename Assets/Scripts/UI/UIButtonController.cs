using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class UIButtonController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;

    [SerializeField, Tooltip("If true, locks the button in place after pressing the button.")] private bool lockOnPress;
    [SerializeField, Tooltip("If true, the player can press the button to perform an action.")] private bool isInteractable = true;

    [SerializeField, Tooltip("The color of the button when pressed.")] private Color pressColor = new Color(0, 0, 0, 0);
    [SerializeField, Tooltip("The color of the button when pressed while disabled.")] private Color disabledColor = new Color(0, 0, 0, 0);

    [SerializeField, Tooltip("The time it takes for the button to fully press.")] private float buttonPressSeconds = 0.5f;
    [SerializeField, Tooltip("The time it takes for the button to be able to be pressed after being enabled.")] private float onEnableDelay = 0.2f;

    [SerializeField, Tooltip("The sound that plays when the button is pressed successfully.")] private AudioClip onPressedSoundEffect;
    [SerializeField, Tooltip("The sound that plays when the button is pressed but is disabled.")] private AudioClip onDisabledSoundEffect;

    public UnityEvent onPressed;    //The event that calls when the button is pressed

    private bool isPressing;    //Checks to make sure if the button is currently being pressed
    private bool isDisabled;      //Checks to see if the button is locked in place

    private Image buttonImage;  //The image of the button
    private Color defaultColor;

    private float currentEnableDelay;
    private bool enableDelayActive;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        defaultColor = buttonImage.color;
        isPressing = false;
        enableDelayActive = true;
    }

    private void OnEnable()
    {
        ChangeButtonColor(defaultColor, false);
        enableDelayActive = true;
    }

    private void OnDisable()
    {
        isPressing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            //If the button is interactable, not currently being pressed, and is not locked, press the button
            if (isInteractable && !isPressing && !isDisabled && !enableDelayActive)
            {
                PressButton(false);
            }
            //If nothing applies, play the disabled sound effect
            else if (!isInteractable || isDisabled)
            {
                PressButton(true);
            }
        }
    }

    private void Update()
    {
        if (enableDelayActive)
        {
            if(currentEnableDelay > onEnableDelay)
            {
                enableDelayActive = false;
                currentEnableDelay = 0f;
            }
            else
                currentEnableDelay += Time.deltaTime;
        }
    }

    /// <summary>
    /// Presses the button and acts depending on whether it is disabled or not.
    /// </summary>
    /// <param name="isDisabled">If true, the button is disabled.</param>
    private void PressButton(bool isDisabled)
    {
        isPressing = true;

        if (!isDisabled)
        {
            if (pressColor != new Color(0, 0, 0, 0))
                ChangeButtonColor(pressColor, false);
            Pressed();
        }
        else
        {
            if (disabledColor != new Color(0, 0, 0, 0))
                ChangeButtonColor(disabledColor, false);

            if (onDisabledSoundEffect != null)
                GetComponent<AudioSource>().PlayOneShot(onDisabledSoundEffect, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
        }

        Invoke("EndPressButton", buttonPressSeconds);
    }

    /// <summary>
    /// Resets the button's appearance.
    /// </summary>
    private void EndPressButton()
    {
        ChangeButtonColor(defaultColor, false);
        isPressing = false;
    }

    /// <summary>
    /// Function to call UnityEvent for pressed button.
    /// </summary>
    private void Pressed()
    {
        //If there's no transition active, press the button
        if (!GameManager.Instance.levelTransitionActive)
        {
            if (onPressedSoundEffect != null)
                GetComponent<AudioSource>().PlayOneShot(onPressedSoundEffect, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));

            onPressed.Invoke();
            //Debug.Log(gameObject.name + " Pressed.");

            if (lockOnPress)
                LockButton(true);
        }
    }

    /// <summary>
    /// Changes the color of the button.
    /// </summary>
    /// <param name="newColor">The new color of the button.</param>
    /// <param name="setToDefault">If true, this sets the button's default color as well.</param>
    public void ChangeButtonColor(Color newColor, bool setToDefault = true)
    {
        buttonImage.color = newColor;

        if (setToDefault)
            defaultColor = newColor;
    }

    public Color GetButtonColor() => GetComponent<Image>().color;

    /// <summary>
    /// Shows the text that is on top of the button.
    /// </summary>
    /// <param name="showText">If true, show the text on the button.</param>
    public void ShowText(bool showText)
    {
        buttonText.gameObject.SetActive(showText);
    }

    /// <summary>
    /// Locks or unlocks the button's position.
    /// </summary>
    /// <param name="locked">If true, the position of the button is locked. If false, the button can be pressed freely.</param>
    public void LockButton(bool locked)
    {
        isDisabled = locked;
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
