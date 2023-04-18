using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class InverteboyController : MonoBehaviour
{
    public enum InverteboyMainScreens { MAIN, ARENA }
    public enum InverteboyHologramScreens { MAIN, TUTORIAL }

    private AudioSource audioSource;
    private Transform wristTransform;

    [Header("Inverteboy Look Settings")]
    [SerializeField, Tooltip("The transform of the flip screen.")] private Transform flipScreenTransform;
    [SerializeField, Tooltip("The amount of time needed to look at the Inverteboy before it opens.")] private float lookBufferTime = 0.5f;
    [SerializeField, Tooltip("The angle between the player's head and the Inverteboy needed to register that the player is looking at the Inverteboy.")] private float lookAngleRange = 30f;
    [SerializeField, Tooltip("The amount of distance between the player's head and the Inverteboy needed to register that the player is looking at the Inverteboy.")] private float lookDistance;
    [SerializeField, Tooltip("The speed of the Inverteboy opening / closing.")] private float movementSpeed;
    [SerializeField, Tooltip("The maximum angle for the Inverteboy to open to.")] private float maxAngle;
    [SerializeField, Tooltip("The animation curve for the Inverteboy opening animation.")] private AnimationCurve openAnimationCurve;
    [Space(10)]

    [SerializeField] private GameObject hologramObject;
    [SerializeField] private float hologramHeight;
    [SerializeField, Tooltip("The animation curve for the hologram animation.")] private AnimationCurve hologramAnimationCurve;
    [SerializeField] private float hologramSpeed;

    [SerializeField, Tooltip("The Inverteboy popup GameObject.")] private GameObject popupPrefab;
    [SerializeField, Tooltip("The Inverteboy popup container.")] private Transform popupContainer;
    [SerializeField, Tooltip("The information window images.")] private Image[] infoWindow;
    [SerializeField, Tooltip("The label text.")] private TextMeshProUGUI labelText;
    [SerializeField, Tooltip("The tutorial text.")] private TextMeshProUGUI tutorialText;

    [SerializeField, Tooltip("The list of the different menus on the main inverteboy.")] private Canvas[] inverteboyMainCanvases;
    [SerializeField, Tooltip("The list of the different menus on the inverteboy hologram.")] private Canvas[] inverteboyHologramCanvases;

    [SerializeField, Tooltip("The strength of the Inverteboy haptics when flashing the screen.")] private float hapticsStrength = 0.25f;
    [SerializeField, Tooltip("The color the the window flashes when the player gets a kill.")] private Color flashInfoWindowColor;
    [SerializeField, Tooltip("The speed of the kill flash.")] private float flashSpeed;
    [SerializeField, Tooltip("The number of flashes.")] private int flashNumber;

    [Header("Music")]
    [SerializeField, Tooltip("Main menu music.")] private AudioClip mainMenuMusic;
    [SerializeField, Tooltip("Lobby music.")] private AudioClip lobbyMusic;
    [SerializeField, Tooltip("Arens music.")] private AudioClip arenaMusic;
    [Space(10)]

    private Canvas currentMainCanvas;
    private Canvas currentHologramCanvas;

    private bool hologramOpen = false;
    private bool hideHologram = false;
    
    private bool isOpen = false;
    private bool forceOpen = false;
    private float timeLookingAtInverteboy;

    private Color defaultInfoWindowColor;

    private IEnumerator inverteboyAnimation;
    private IEnumerator hologramAnimation;
    private IEnumerator flashAnimation;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        currentMainCanvas = inverteboyMainCanvases[(int)InverteboyMainScreens.MAIN];
        currentHologramCanvas = inverteboyHologramCanvases[(int)InverteboyHologramScreens.MAIN];
        defaultInfoWindowColor = infoWindow[0].color;
        ShowHologram(false);

        UpdateVolume();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameSettings.titleScreenScene)
            PlayMusic(mainMenuMusic);

        if (scene.name == GameSettings.roomScene)
            PlayMusic(lobbyMusic);

        if (scene.name == GameSettings.arenaScene)
        {
            PlayMusic(arenaMusic);
            SwitchMainCanvas((int)InverteboyMainScreens.ARENA);
            hideHologram = true;
        }

        if(scene.name == GameSettings.tutorialScene)
        {
            SwitchMainCanvas((int)InverteboyMainScreens.MAIN);
            SwitchHologramCanvas((int)InverteboyHologramScreens.TUTORIAL);
            ForceOpenInverteboy(true);
        }
    }

    /// <summary>
    /// Plays music from the Inverteboy.
    /// </summary>
    /// <param name="audioClip">The audio clip to play.</param>
    public void PlayMusic(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    private void Update()
    {
        Vector3 boyDirection = transform.position - PlayerController.instance.cam.transform.position;
        float boyAngle = Vector3.Angle(boyDirection, PlayerController.instance.cam.transform.forward);
        float boyDistance = Vector3.Distance(transform.position, PlayerController.instance.cam.transform.position);

        InverteboyLookCheck(boyDistance, boyAngle);
    }

    private void InverteboyLookCheck(float distance, float angle)
    {
        if (forceOpen)
            return;
        
        //If the player is looking at the inverteboy and is close enough to it
        if(angle < lookAngleRange && distance < lookDistance)
        {
            //If the player has been looking at the inverteboy for enough time and is open, open the inverteboy
            if(timeLookingAtInverteboy > lookBufferTime && !isOpen)
            {
                OpenInverteboy(true);
                OpenHologramMenu(true);
            }

            timeLookingAtInverteboy += Time.deltaTime;  //Increment timer
        }
        else
        {
            timeLookingAtInverteboy = 0f;   //Reset the timer

            //Close the inverteboy if open
            if (isOpen)
            {
                OpenInverteboy(false);
                OpenHologramMenu(false);
            }
        }
    }

    /// <summary>
    /// Forces the Inverteboy to open and to stay open.
    /// </summary>
    /// <param name="open">If true, the Inverteboy is forced open.</param>
    public void ForceOpenInverteboy(bool open)
    {
        forceOpen = open;

        if (forceOpen && !isOpen)
        {
            OpenInverteboy(true);
            OpenHologramMenu(true);
        }
    }

    private void OpenInverteboy(bool openInverteboy)
    {
        if (inverteboyAnimation != null)
            StopCoroutine(inverteboyAnimation);

        inverteboyAnimation = InverteboyMovementAnimation(openInverteboy);
        StartCoroutine(inverteboyAnimation);
    }

    private IEnumerator InverteboyMovementAnimation(bool open)
    {
        isOpen = open;

        float timeElapsed = 0;

        float startingAngle, endingAngle;

        if (isOpen)
        {
            startingAngle = 90f;
            endingAngle = maxAngle;
        }
        else
        {
            startingAngle = maxAngle;
            endingAngle = 90f;
        }


        while (timeElapsed < movementSpeed)
        {
            float inverteboyAngle = Mathf.Lerp(startingAngle, endingAngle, openAnimationCurve.Evaluate(timeElapsed / movementSpeed));
            flipScreenTransform.localEulerAngles = new Vector3(inverteboyAngle, 0f, 0f);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        flipScreenTransform.localEulerAngles = new Vector3(endingAngle, 0f, 0f);
    }

    public void OpenHologramMenu(bool openHologram)
    {
        if (hideHologram)
            return;

        if (hologramAnimation != null)
            StopCoroutine(hologramAnimation);

        hologramAnimation = HologramMovementAnimation(openHologram);
        StartCoroutine(hologramAnimation);
    }

    private IEnumerator HologramMovementAnimation(bool open)
    {
        hologramOpen = open;

        float timeElapsed = 0;

        float startingY, endingY;
        Vector3 startingScale, endingScale;

        if (hologramOpen)
        {
            startingY = 0f;
            endingY = hologramHeight;

            startingScale = Vector3.zero;
            endingScale = Vector3.one;
        }
        else
        {
            startingY = hologramHeight;
            endingY = 0f;

            startingScale = Vector3.one;
            endingScale = Vector3.zero;
        }


        while (timeElapsed < movementSpeed)
        {
            float hologramY = Mathf.Lerp(startingY, endingY, hologramAnimationCurve.Evaluate(timeElapsed / hologramSpeed));
            Vector3 hologramScale = Vector3.Lerp(startingScale, endingScale, hologramAnimationCurve.Evaluate(timeElapsed / hologramSpeed));

            hologramObject.transform.localPosition = new Vector3(hologramObject.transform.localPosition.x, hologramY, hologramObject.transform.localPosition.z);
            hologramObject.transform.localScale = hologramScale;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        hologramObject.transform.localPosition = new Vector3(hologramObject.transform.localPosition.x, endingY, hologramObject.transform.localPosition.z);
        hologramObject.transform.localScale = endingScale;
    }

    private void ShowHologram(bool showHologram)
    {
        if (showHologram)
        {
            hologramObject.transform.localPosition = new Vector3(hologramObject.transform.localPosition.x, hologramHeight, hologramObject.transform.localPosition.z);
            hologramObject.transform.localScale = Vector3.one;
        }
        else
        {
            hologramObject.transform.localPosition = new Vector3(hologramObject.transform.localPosition.x, 0f, hologramObject.transform.localPosition.z);
            hologramObject.transform.localScale = Vector3.zero;
        }
    }

    /// <summary>
    /// Updates the tutorial menu text.
    /// </summary>
    /// <param name="message">The message for the tutorial.</param>
    /// <param name="label">The label of the tutorial.</param>
    public void UpdateTutorialText(string message, string label = "")
    {
        labelText.text = label;
        tutorialText.text = message;
    }

    /// <summary>
    /// Switches the canvas of the main Inverteboy screen.
    /// </summary>
    /// <param name="canvasIndex">the index of the new canvas.</param>
    public void SwitchMainCanvas(int canvasIndex)
    {
        Canvas newCanvas = inverteboyMainCanvases[canvasIndex];

        Debug.Log("Switching To " + newCanvas.name);
        Debug.Log("Current Canvas: " + currentMainCanvas.name);

        currentMainCanvas.enabled = false;
        currentMainCanvas = newCanvas;
        currentMainCanvas.enabled = true;
    }

    /// <summary>
    /// Switches the canvas of the Inverteboy hologram.
    /// </summary>
    /// <param name="canvasIndex">the index of the new canvas.</param>
    public void SwitchHologramCanvas(int canvasIndex)
    {
        Canvas newCanvas = inverteboyHologramCanvases[canvasIndex];

        currentHologramCanvas.enabled = false;
        currentHologramCanvas = newCanvas;
        currentHologramCanvas.enabled = true;
    }

    /// <summary>
    /// Switches the canvas of the main Inverteboy screen.
    /// </summary>
    /// <param name="canvasIndex">the index of the new canvas.</param>
    public void SwitchMainCanvas(InverteboyMainScreens canvasIndex)
    {
        Canvas newCanvas = inverteboyMainCanvases[(int)canvasIndex];

        currentMainCanvas.enabled = false;
        currentMainCanvas = newCanvas;
        currentMainCanvas.enabled = true;
    }

    /// <summary>
    /// Switches the canvas of the Inverteboy hologram.
    /// </summary>
    /// <param name="canvasIndex">the index of the new canvas.</param>
    public void SwitchHologramCanvas(InverteboyHologramScreens canvasIndex)
    {
        Canvas newCanvas = inverteboyHologramCanvases[(int)canvasIndex];

        currentHologramCanvas.enabled = false;
        currentHologramCanvas = newCanvas;
        currentHologramCanvas.enabled = true;
    }

    /// <summary>
    /// Shows a canvas without hiding any others.
    /// </summary>
    /// <param name="canvasIndex">The index of the canvas.</param>
    /// <param name="showCanvas">If true, the canvas is enabled. If false, the canvas is hidden.</param>
    public void ShowMainCanvas(int canvasIndex, bool showCanvas)
    {
        inverteboyMainCanvases[canvasIndex].enabled = showCanvas;
    }

    /// <summary>
    /// Shows a canvas without hiding any others.
    /// </summary>
    /// <param name="canvasIndex">The index of the canvas.</param>
    /// <param name="showCanvas">If true, the canvas is enabled. If false, the canvas is hidden.</param>
    public void ShowMainCanvas(InverteboyMainScreens canvasIndex, bool showCanvas)
    {
        inverteboyMainCanvases[(int)canvasIndex].enabled = showCanvas;
    }

    /// <summary>
    /// Shows a popup on the player's HUD.
    /// </summary>
    /// <param name="sender">The sender to show on the popup.</param>
    public void ShowInverteboyPopup(string sender = "")
    {
        GameObject newPopup = Instantiate(popupPrefab, popupContainer);

        if (sender == "")
            newPopup.GetComponentInChildren<TextMeshProUGUI>().text = "Incoming Message On Inverteboy";
        else
            newPopup.GetComponentInChildren<TextMeshProUGUI>().text = "Incoming Message From:\n" + sender;
    }

    /// <summary>
    /// Flashes the information window and has it vibrate.
    /// </summary>
    public void Flash()
    {
        //Flashes the information window
        if (flashAnimation != null)
            StopCoroutine(flashAnimation);

        flashAnimation = FlashInfoWindow();
        StartCoroutine(flashAnimation);
    }

    private IEnumerator FlashInfoWindow()
    {
        for (int i = 0; i < flashNumber * 2; i++)
        {
            foreach(var window in infoWindow)
                window.color = window.color == defaultInfoWindowColor ? flashInfoWindowColor : defaultInfoWindowColor;

            if (infoWindow[0].color == flashInfoWindowColor)
                PlayerController.instance.SendHapticImpulse(InputDeviceRole.LeftHanded, new Vector2(hapticsStrength, flashSpeed));
            yield return new WaitForSeconds(flashSpeed);
        }
    }

    public void StopMusic() => audioSource.Stop();
    public void UpdateVolume() => audioSource.volume = PlayerPrefs.GetFloat("MusicVolume", GameSettings.defaultMusicSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound);

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopMusic();
    }
}
