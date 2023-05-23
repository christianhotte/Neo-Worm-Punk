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
    public enum InverteboySecondaryScreens { MAIN, ROOMLOG, ROUNDLOG }
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

    [SerializeField, Tooltip("The container for the room log.")] private Transform roomLogContainer;
    [SerializeField, Tooltip("The prefab for the log entry.")] private GameObject logEntryPrefab;

    [Header("Hologram Settings")]
    [SerializeField, Tooltip("The label text.")] private TextMeshProUGUI labelText;
    [SerializeField, Tooltip("The tutorial text.")] private TextMeshProUGUI tutorialText;
    [SerializeField, Tooltip("The tutorial progress text.")] private TextMeshProUGUI tutorialProgressText;
    [SerializeField, Tooltip("The tutorial diagram image.")] private Image tutorialImage;
    [SerializeField, Tooltip("The tutorial stopwatch text.")] private TextMeshProUGUI tutorialStopwatch;
    [SerializeField, Tooltip("The tutorial stopwatch flash color.")] private Color tutorialFlashColor;
    [SerializeField, Tooltip("The speed of the tutorial stopwatch flash.")] private float tutorialFlashSpeed = 0.3f;
    [SerializeField, Tooltip("The number of tutorial stopwatch flashes.")] private int tutorialFlashNumber = 5;
    [Space(10)]
    [SerializeField, Tooltip("The speed that the diagram image shrinks.")] private float shrinkImageDuration;
    [SerializeField, Tooltip("The animation curve for the diagram shrink image animation.")] private LeanTweenType diagramShrinkEaseType;
    [SerializeField, Tooltip("The animation curve for the diagram grow image animation.")] private LeanTweenType diagramGrowEaseType;
    [SerializeField, Tooltip("The speed that the diagram image grows.")] private float growImageDuration;
    [Space(10)]

    [SerializeField, Tooltip("The list of the different menus on the main inverteboy.")] private Canvas[] inverteboyMainCanvases;
    [SerializeField, Tooltip("The list of the different menus on the secondary inverteboy.")] private Canvas[] inverteboySecondaryCanvases;
    [SerializeField, Tooltip("The list of the different menus on the inverteboy hologram.")] private Canvas[] inverteboyHologramCanvases;

    [SerializeField, Tooltip("The strength of the Inverteboy haptics when flashing the screen.")] private float hapticsStrength = 0.25f;
    [SerializeField, Tooltip("The color the the window flashes when the player gets a kill.")] private Color flashInfoWindowColor;
    [SerializeField, Tooltip("The speed of the kill flash.")] private float flashSpeed;
    [SerializeField, Tooltip("The number of flashes.")] private int flashNumber;

    [Header("Music")]
    [SerializeField, Tooltip("Start screen music.")] private AudioClip startScreenMusic;
    [SerializeField, Tooltip("Main menu music.")] private AudioClip mainMenuMusic;
    [SerializeField, Tooltip("Lobby music.")] private AudioClip lobbyMusic;
    [SerializeField, Tooltip("Arena music intro.")] private AudioClip arenaMusicIntro;
    [SerializeField, Tooltip("Arena music.")] private AudioClip arenaMusic;
    [Space(10)]

    private Canvas currentMainCanvas;
    private Canvas currentSecondaryCanvas;
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

    private Sprite defaultTutorialSprite;
    private float tutorialTime;
    private bool tutorialTimerActive = false;
    private Color tutorialStopwatchColor;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        currentMainCanvas = inverteboyMainCanvases[(int)InverteboyMainScreens.MAIN];
        currentSecondaryCanvas = inverteboySecondaryCanvases[(int)InverteboySecondaryScreens.MAIN];
        currentHologramCanvas = inverteboyHologramCanvases[(int)InverteboyHologramScreens.MAIN];
        defaultInfoWindowColor = infoWindow[0].color;
        ShowHologram(false);

        defaultTutorialSprite = tutorialImage.sprite;
        tutorialStopwatchColor = tutorialStopwatch.color;

        UpdateVolume();
        InitializeInverteboyScreens();
    }

    private void InitializeInverteboyScreens()
    {
        if (SceneManager.GetActiveScene().name == GameSettings.startScene)
        {
            PlayMusic(startScreenMusic);
        }

        if (SceneManager.GetActiveScene().name == GameSettings.titleScreenScene)
        {
            PlayMusic(mainMenuMusic);
        }

        if (SceneManager.GetActiveScene().name == GameSettings.roomScene)
        {
            PlayMusic(lobbyMusic);
            SwitchSecondaryCanvas(InverteboySecondaryScreens.ROOMLOG);
            hideHologram = true;

        }

        if (SceneManager.GetActiveScene().name == GameSettings.arenaScene)
        {
            StartCoroutine(PlayArenaMusic());
            SwitchMainCanvas(InverteboyMainScreens.ARENA);
            hideHologram = true;
        }

        if (SceneManager.GetActiveScene().name == GameSettings.tutorialScene)
        {
            SwitchMainCanvas(InverteboyMainScreens.MAIN);
            SwitchHologramCanvas(InverteboyHologramScreens.TUTORIAL);
            ForceOpenInverteboy(true);
        }
    }

    /// <summary>
    /// Plays music from the Inverteboy.
    /// </summary>
    /// <param name="audioClip">The audio clip to play.</param>
    public void PlayMusic(AudioClip audioClip)
    {
        if(audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Plays the arena music with an intro that only plays once.
    /// </summary>
    /// <returns></returns>
    public IEnumerator PlayArenaMusic()
    {
        double introTimer = 0;
        double introTime = 0;

        //Play the intro to the arena music
        if (arenaMusicIntro != null)
        {
            PlayMusic(arenaMusicIntro);
            audioSource.loop = false;
            yield return new WaitUntil(() => !audioSource.isPlaying);
        }

        //Play the arena music once the intro is over
        PlayMusic(arenaMusic);
        audioSource.loop = true;
    }

    /// <summary>
    /// Plays a sound from the Inverteboy.
    /// </summary>
    /// <param name="audioClip">The audio clip to play.</param>
    public void PlayOneShotSound(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
    }

    private void Update()
    {
        Vector3 boyDirection = transform.position - PlayerController.instance.cam.transform.position;
        float boyAngle = Vector3.Angle(boyDirection, PlayerController.instance.cam.transform.forward);
        float boyDistance = Vector3.Distance(transform.position, PlayerController.instance.cam.transform.position);

        InverteboyLookCheck(boyDistance, boyAngle);
        UpdateTutorialTimer();
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
                if(!hideHologram)
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
            if(!hideHologram)
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
    /// <param name="label">The label for the tutorial.</param>
    /// <param name="diagramSprite">The diagram for the tutorial.</param>
    public void UpdateTutorialText(string message, string label = "", Sprite diagramSprite = null)
    {
        labelText.text = label;
        tutorialText.text = message;

        Sprite newTutorialSprite;

        if (diagramSprite != null)
            newTutorialSprite = diagramSprite;
        else
            newTutorialSprite = defaultTutorialSprite;

        //If the new diagram is different, play an animation
        if (newTutorialSprite != tutorialImage.sprite)
            DiagramSpriteAnimation(newTutorialSprite);
    }

    private void DiagramSpriteAnimation(Sprite newSprite)
    {
        LeanTween.scale(tutorialImage.gameObject, Vector3.zero, shrinkImageDuration).setEase(diagramShrinkEaseType).setOnComplete(() => tutorialImage.sprite = newSprite);
        LeanTween.delayedCall(shrinkImageDuration, () => LeanTween.scale(tutorialImage.gameObject, Vector3.one, growImageDuration).setEase(diagramGrowEaseType));
    }

    public void UpdateTutorialProgress(string progressText)
    {
        tutorialProgressText.text = progressText;
    }

    public void StartTutorialTimer()
    {
        tutorialTimerActive = true;
    }

    public void StopTutorialTimer()
    {
        tutorialTimerActive = false;
        StartCoroutine(FlashTutorialTimer(tutorialFlashNumber, tutorialFlashSpeed));

        //Check to see if the time recorded is the best tutorial time so far. If so, record it
        if(PlayerPrefs.GetFloat("BestTutorialTime") == 0 || PlayerPrefs.GetFloat("BestTutorialTime") > tutorialTime)
            PlayerPrefs.SetFloat("BestTutorialTime", tutorialTime);

        //If the player has completed the tutorial in less than a certain amount of time, give them an achievement
        if (tutorialTime < 60)
        {
            if (!AchievementListener.Instance.IsAchievementUnlocked(27))
                AchievementListener.Instance.UnlockAchievement(27);
        }
    }

    /// <summary>
    /// Update the tutorial stopwatch timer when the timer is active.
    /// </summary>
    private void UpdateTutorialTimer()
    {
        if (tutorialTimerActive)
        {
            tutorialTime += Time.deltaTime; //Increment timer
            string minutes = Mathf.FloorToInt(tutorialTime / 60f < 0 ? 0 : tutorialTime / 60f).ToString();
            string seconds = Mathf.FloorToInt(tutorialTime % 60f < 0 ? 0 : tutorialTime % 60f).ToString("00");
            string centiseconds = Mathf.FloorToInt((tutorialTime * 100f) % 100f < 0 ? 0: (tutorialTime * 100f) % 100f).ToString("00");

            tutorialStopwatch.text = minutes + ":" + seconds + ":<size=4>" + centiseconds + "</size>";
        }
    }

    private IEnumerator FlashTutorialTimer(int numberOfFlashes, float speed)
    {
        for(int i = 0; i < numberOfFlashes; i++)
        {
            tutorialStopwatch.color = (tutorialStopwatch.color == tutorialStopwatchColor) ? tutorialFlashColor : tutorialStopwatchColor;
            yield return new WaitForSeconds(speed);
        }
    }

    /// <summary>
    /// Switches the canvas of the main Inverteboy screen.
    /// </summary>
    /// <param name="canvasIndex">the index of the new canvas.</param>
    public void SwitchMainCanvas(int canvasIndex)
    {
        Canvas newCanvas = inverteboyMainCanvases[canvasIndex];

        currentMainCanvas.enabled = false;
        inverteboyMainCanvases[canvasIndex].enabled = true;
        currentMainCanvas = newCanvas;
    }

    /// <summary>
    /// Switches the canvas of the secondary Inverteboy screen.
    /// </summary>
    /// <param name="canvasIndex">the index of the new canvas.</param>
    public void SwitchSecondaryCanvas(int canvasIndex)
    {
        Canvas newCanvas = inverteboySecondaryCanvases[canvasIndex];

        currentSecondaryCanvas.enabled = false;
        currentSecondaryCanvas = newCanvas;
        inverteboySecondaryCanvases[canvasIndex].enabled = true;
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
        inverteboyHologramCanvases[canvasIndex].enabled = true;
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
        inverteboyMainCanvases[(int)canvasIndex].enabled = true;
    }

    /// <summary>
    /// Switches the canvas of the secondary Inverteboy screen.
    /// </summary>
    /// <param name="canvasIndex">the index of the new canvas.</param>
    public void SwitchSecondaryCanvas(InverteboySecondaryScreens canvasIndex)
    {
        Canvas newCanvas = inverteboySecondaryCanvases[(int)canvasIndex];

        currentSecondaryCanvas.enabled = false;
        currentSecondaryCanvas = newCanvas;
        inverteboySecondaryCanvases[(int)canvasIndex].enabled = true;
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
        inverteboyHologramCanvases[(int)canvasIndex].enabled = true;
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

    public void AddToRoomLog(string message)
    {
        LogEntry newLogEntry = Instantiate(logEntryPrefab, roomLogContainer).GetComponent<LogEntry>();
        newLogEntry.UpdateLogText(message);
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
        StopMusic();
    }
}
