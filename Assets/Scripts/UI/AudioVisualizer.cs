using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioVisualizer : MonoBehaviour
{
    [Header("Spectrum Data Settings")]
    [SerializeField, Tooltip("The AudioSource to get the spectrum data from.")] private AudioSource audioSource;
    [SerializeField, Tooltip("The type of spectrum to show.")] private FFTWindow spectrumType;
    [SerializeField, Tooltip("The audio channel to get the spectrum data from.")] private int audioChannel = 0;
    [SerializeField, Tooltip("The number of samples to take from the spectrum (Note: they must be in multiples of 64 to work properly.)")] private int numberOfSamples = 256;
    [Space(10)]

    [Header("Visual Settings")]
    [SerializeField, Tooltip("The spectrum line prefab.")] private GameObject spectrumLinePrefab;
    [SerializeField, Tooltip("The maximum scaling for the spectrum.")] private float maxScale;
    [SerializeField, Tooltip("The color of the spectrum.")] private Color spectrumColor = Color.white;
    [Space(10)]

    [Header("Rainbow Settings")]
    [SerializeField, Tooltip("A rainbow option, because why not? Ignores the spectrum color.")] private bool enableRainbowSpectrum;
    [SerializeField, Tooltip("If true, the colors in the rainbow are cycled in a random order.")] private bool randomizeRainbowColors;
    [SerializeField, Tooltip("The speed of the rainbow color change.")] private float rainbowSpeed = 1f;

    private float spacing;  //The spacing between each spectrum line
    private float[] samples;    //The array where the spectrum data is stored for each sample
    private GameObject[] sampleLines;   //The physical sample lines that visualize the spectrum data

    //A list of colors in the rainbow
    private Color32[] rainbowColors = new Color32[7]
    {
        new Color32(255, 0, 0, 255),        //Red
        new Color32(255, 165, 0, 255),      //Orange
        new Color32(255, 255, 0, 255),      //Yellow
        new Color32(0, 255, 0, 255),        //Green
        new Color32(0, 0, 255, 255),        //Blue
        new Color32(75, 0, 130, 255),       //Indigo
        new Color32(238, 130, 238, 255),    //Violet
    };

    void Start()
    {
        samples = new float[numberOfSamples];
        sampleLines = new GameObject[numberOfSamples];
        CreateSpectrum();
    }

    /// <summary>
    /// Generates the initial spectrum.
    /// </summary>
    private void CreateSpectrum()
    {
        spacing = GetComponent<RectTransform>().rect.width / sampleLines.Length;    //Creates spacing based on the container's width.

        for(int i = 0; i < sampleLines.Length; i++)
        {
            GameObject currentLine = Instantiate(spectrumLinePrefab);
            RectTransform lineTransform = currentLine.GetComponent<RectTransform>();
            currentLine.transform.parent = transform;

            //Zeroes out position and rotation of the sample line RectTransform
            lineTransform.localPosition = Vector3.zero;
            lineTransform.anchoredPosition = Vector3.zero;
            lineTransform.localRotation = Quaternion.identity;

            //Adjusts the position based on the sample index
            Vector3 pos = lineTransform.anchoredPosition;
            pos.x = pos.x + (spacing * i);
            lineTransform.anchoredPosition = pos;

            if(!enableRainbowSpectrum)
                currentLine.GetComponent<Image>().color = spectrumColor;

            currentLine.name = "SpectrumLine" + i.ToString("000");
            sampleLines[i] = currentLine;   //Adds to list of sample lines
        }

        if (enableRainbowSpectrum)
            StartCoroutine(CycleRainbow());
    }

    void Update()
    {
        GetSpectrumAudioSource();
        UpdateSpectrum();
    }

    /// <summary>
    /// Gets the spectrum data from the audio source, storing the data in the samples array.
    /// </summary>
    private void GetSpectrumAudioSource()
    {
        // Get the audio data from the audio source
        audioSource.GetSpectrumData(samples, audioChannel, spectrumType);
    }

    /// <summary>
    /// Cycles through rainbow colors and changes the spectrum accordingly.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CycleRainbow()
    {
        //Random rainbow colors
        if (randomizeRainbowColors)
        {
            int startColor = Random.Range(0, rainbowColors.Length);
            int endColor = Random.Range(0, rainbowColors.Length);

            while (true)
            {
                for (float t = 0; t < 1f; t += 0.001f * rainbowSpeed)
                {
                    for (int sample = 0; sample < sampleLines.Length; sample++)
                        sampleLines[sample].GetComponent<Image>().color = Color.Lerp(rainbowColors[startColor], rainbowColors[endColor], t);
                    yield return null;
                }
                startColor = endColor;
                endColor = Random.Range(0, rainbowColors.Length);
            }
        }

        //Linear rainbow colors
        else
        {
            int i = 0;

            while (true)
            {
                for (float t = 0; t < 1f; t += 0.001f * rainbowSpeed)
                {
                    for (int sample = 0; sample < sampleLines.Length; sample++)
                        sampleLines[sample].GetComponent<Image>().color = Color.Lerp(rainbowColors[i % 7], rainbowColors[(i + 1) % 7], t);
                    yield return null;
                }
                i++;
            }
        }
    }

    /// <summary>
    /// Updates the spectrum based on the data.
    /// </summary>
    private void UpdateSpectrum()
    {
        for(int i = 0; i < sampleLines.Length; i++)
            sampleLines[i].transform.localScale = new Vector3(1, 1 + (samples[i] * maxScale), 1);
    }

    public void SetAudioSource(AudioSource newSource) => audioSource = newSource;
}
