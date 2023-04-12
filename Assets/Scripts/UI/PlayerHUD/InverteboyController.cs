using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class InverteboyController : MonoBehaviour
{
    public enum InverteboyScreens { MAIN, ARENA, TUTORIAL, VISUALIZER, MENU }

    private AudioSource audioSource;
    private Transform wristTransform;

    [Header("Inverteboy Look Settings")]
    [SerializeField, Tooltip("The transform of the flip screen.")] private Transform flipScreenTransform;
    [SerializeField, Tooltip("The amount of time needed to look at the Inverteboy before it opens.")] private float lookBufferTime = 0.5f;
    [SerializeField, Tooltip("The angle between the player's head and the Inverteboy needed to register that the player is looking at the Inverteboy.")] private float lookAngleRange = 30f;
    [SerializeField, Tooltip("The amount of distance between the player's head and the Inverteboy needed to register that the player is looking at the Inverteboy.")] private float lookDistance;
    [SerializeField, Tooltip("The speed of the Inverteboy opening / closing.")] private float movementSpeed;
    [SerializeField, Tooltip("The maximum angle for the Inverteboy to open to.")] private float maxAngle;
    [SerializeField, Tooltip("The animation curve for the Inverteboy opening animation.")] private AnimationCurve animationCurve;
    [Space(10)]

    [SerializeField, Tooltip("The label text.")] private TextMeshProUGUI labelText;
    [SerializeField, Tooltip("The tutorial text.")] private TextMeshProUGUI tutorialText;

    [SerializeField, Tooltip("The list of the different menus on the inverteboy.")] private Canvas[] inverteboyCanvases;

    [Header("Music")]
    [SerializeField, Tooltip("Main menu music.")] private AudioClip mainMenuMusic;
    [SerializeField, Tooltip("Lobby music.")] private AudioClip lobbyMusic;
    [SerializeField, Tooltip("Arens music.")] private AudioClip arenaMusic;

    private Canvas currentCanvas;

    private bool isOpen = false;
    private bool forceOpen = false;
    private float timeLookingAtInverteboy;
    private IEnumerator inverteboyAnimation;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        currentCanvas = inverteboyCanvases[(int)InverteboyScreens.MAIN];
        UpdateVolume();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If we are loaded into the tutorial scene, show the tutorial canvas
        if (scene.name == GameSettings.tutorialScene)
        {
            SwitchCanvas(InverteboyScreens.TUTORIAL);
        }

        if (scene.name == GameSettings.titleScreenScene)
            PlayMusic(mainMenuMusic);

        if (scene.name == GameSettings.roomScene)
            PlayMusic(lobbyMusic);

        if (scene.name == GameSettings.arenaScene)
            PlayMusic(arenaMusic);
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
                OpenInverteboy(true);

            timeLookingAtInverteboy += Time.deltaTime;  //Increment timer
        }
        else
        {
            timeLookingAtInverteboy = 0f;   //Reset the timer

            //Close the inverteboy if open
            if (isOpen)
                OpenInverteboy(false);
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
            OpenInverteboy(true);
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
            float inverteboyAngle = Mathf.Lerp(startingAngle, endingAngle, animationCurve.Evaluate(timeElapsed / movementSpeed));
            flipScreenTransform.localEulerAngles = new Vector3(inverteboyAngle, 0f, 0f);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        flipScreenTransform.localEulerAngles = new Vector3(endingAngle, 0f, 0f);
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
    /// Switches the canvas of the Inverteboy.
    /// </summary>
    /// <param name="canvasIndex">the index of the new canvas.</param>
    public void SwitchCanvas(int canvasIndex)
    {
        Canvas newCanvas = inverteboyCanvases[canvasIndex];

        currentCanvas.enabled = false;
        currentCanvas = newCanvas;
        currentCanvas.enabled = true;
    }

    /// <summary>
    /// Switches the canvas of the Inverteboy.
    /// </summary>
    /// <param name="canvasIndex">the index of the new canvas.</param>
    public void SwitchCanvas(InverteboyScreens canvasIndex)
    {
        Canvas newCanvas = inverteboyCanvases[(int)canvasIndex];

        currentCanvas.enabled = false;
        currentCanvas = newCanvas;
        currentCanvas.enabled = true;
    }

    /// <summary>
    /// Shows a canvas without hiding any others.
    /// </summary>
    /// <param name="canvasIndex">The index of the canvas.</param>
    /// <param name="showCanvas">If true, the canvas is enabled. If false, the canvas is hidden.</param>
    public void ShowCanvas(int canvasIndex, bool showCanvas)
    {
        inverteboyCanvases[canvasIndex].enabled = showCanvas;
    }

    /// <summary>
    /// Shows a canvas without hiding any others.
    /// </summary>
    /// <param name="canvasIndex">The index of the canvas.</param>
    /// <param name="showCanvas">If true, the canvas is enabled. If false, the canvas is hidden.</param>
    public void ShowCanvas(InverteboyScreens canvasIndex, bool showCanvas)
    {
        inverteboyCanvases[(int)canvasIndex].enabled = showCanvas;
    }

    public void StopMusic() => audioSource.Stop();
    public void UpdateVolume() => audioSource.volume = PlayerPrefs.GetFloat("MusicVolume", GameSettings.defaultMusicSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound);

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopMusic();
    }
}
