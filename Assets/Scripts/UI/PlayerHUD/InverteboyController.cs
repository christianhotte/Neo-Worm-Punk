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
    [SerializeField, Tooltip("The label text.")] private TextMeshProUGUI labelText;
    [SerializeField, Tooltip("The tutorial text.")] private TextMeshProUGUI tutorialText;

    [SerializeField, Tooltip("The list of the different menus on the inverteboy.")] private Canvas[] inverteboyCanvases;

    private Canvas currentCanvas;

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
    private void UpdateVolume() => audioSource.volume = PlayerPrefs.GetFloat("MusicVolume", GameSettings.defaultMusicSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound);

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
