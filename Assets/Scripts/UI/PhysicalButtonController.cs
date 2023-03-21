using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicalButtonController : MonoBehaviour
{
    [SerializeField] private Canvas buttonCanvas;
    [SerializeField, Tooltip("Defines how far the button needs to be move to be registered as pressed. Higher number = more sensitivity.")] private float threshold = 0.1f;    //The threshold for the button that defines when the button is pressed

    [SerializeField, Tooltip("If true, locks the button in place after pressing the button.")] private bool lockOnPress;
    [SerializeField, Tooltip("If true, the player can press the button to perform an action.")] private bool isInteractable = true;

    [SerializeField, Tooltip("The color of the button when pressed.")] private Color pressColor = new Color(0, 0, 0, 0);

    [SerializeField, Tooltip("The time it takes for the button to fully press.")] private float buttonPressSeconds = 0.5f;
    [SerializeField, Tooltip("The animation movement for the button.")] private AnimationCurve buttonAniCurve;

    [SerializeField, Tooltip("The sound that plays when the button is pressed successfully.")] private AudioClip onPressedSoundEffect;
    [SerializeField, Tooltip("The sound that plays when the button is pressed but is disabled.")] private AudioClip onDisabledSoundEffect;

    public UnityEvent onPressed;    //The event that calls when the button is pressed

    private bool isPressing;    //Checks to make sure if the button is currently being pressed
    private bool isPressed;     //Checks to make sure pressed function isn't repeatedly called
    private bool isLocked;      //Checks to see if the button is locked in place
    private Vector3 startPos;   //Start position of button
    private Vector3 endPos;     //End position of button

    private Transform buttonTransform;  //The button transform that moves when the button is pressed
    private IEnumerator buttonCoroutine; //The button coroutine

    private Color defaultColor;

    private void Start()
    {
        buttonTransform = transform.Find("Button");
        defaultColor = buttonTransform.Find("Clicker").GetComponent<MeshRenderer>().material.color;
        startPos = buttonTransform.localPosition;
        endPos = startPos;
        endPos.z += threshold;
        isPressed = false;
    }

    private void OnEnable()
    {
        if(buttonTransform != null)
            buttonTransform.localPosition = startPos;
        isPressed = false;
    }

    private void OnDisable()
    {
        isPressed = false;
        isPressing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            //If the button is interactable, not currently being pressed, and is not locked, press the button
            if (isInteractable && !isPressing && !isLocked)
            {
                buttonCoroutine = PlayButtonAni();
                StartCoroutine(buttonCoroutine);
            }
            //If nothing applies, play the disabled sound effect
            else if(!isInteractable || isLocked)
            {
                if (onDisabledSoundEffect != null)
                    GetComponent<AudioSource>().PlayOneShot(onDisabledSoundEffect, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
            }
        }
    }


    /// <summary>
    /// Smoothly moves the button automatically over time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayButtonAni()
    {
        isPressing = true;
        float elapsedTime = 0;

        //Move the button during the animation or if the button is unlocked
        while (elapsedTime < buttonPressSeconds && !isLocked)
        {
            //Animation curve to make the button feel smoother
            float t = buttonAniCurve.Evaluate(elapsedTime / buttonPressSeconds);
            buttonTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            elapsedTime += Time.deltaTime;

            //If the button is currently not pressed and it's halfway through the animation, press the button
            if (!isPressed && elapsedTime >= buttonPressSeconds / 2f)
            {
                Pressed();
                if (pressColor != new Color(0, 0, 0, 0))
                    ChangeButtonColor(pressColor, false);
            }

            yield return null;
        }

        ChangeButtonColor(defaultColor, false);

        if (!isLocked)
            buttonTransform.localPosition = startPos;
        isPressed = false;
        isPressing = false;
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

            if (onPressedSoundEffect != null)
                GetComponent<AudioSource>().PlayOneShot(onPressedSoundEffect, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));

            onPressed.Invoke();
            Debug.Log(gameObject.name + " Pressed.");

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
        buttonTransform.Find("Clicker").GetComponent<MeshRenderer>().material.color = newColor;

        if (setToDefault)
            defaultColor = newColor;
    }

    public Color GetButtonColor() => buttonTransform.Find("Clicker").GetComponent<MeshRenderer>().material.color;

    /// <summary>
    /// Shows the text that is on top of the button.
    /// </summary>
    /// <param name="showText">If true, show the text on the button.</param>
    public void ShowText(bool showText)
    {
        buttonCanvas.gameObject.SetActive(showText);
    }

    /// <summary>
    /// Locks or unlocks the button's position.
    /// </summary>
    /// <param name="locked">If true, the position of the button is locked. If false, the button can be pressed freely.</param>
    private void LockButton(bool locked)
    {
        isLocked = locked;
    }

    /// <summary>
    /// Either enables or disables the button.
    /// </summary>
    /// <param name="isEnabled">If true, the button is enabled for interaction. If false, no interaction events can be called.</param>
    public void EnableButton(bool isEnabled)
    {
        isInteractable = isEnabled;

        if (!isInteractable)
            buttonTransform.localPosition = endPos;
        else
            buttonTransform.localPosition = startPos;
    }
}
