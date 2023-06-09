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
    [SerializeField, Tooltip("Decides whether or not to display the frequency bands of the spectrum.")] private bool displayFrequencyBands;
    [Space(10)]

    [Header("Visual Settings")]
    [SerializeField, Tooltip("The spectrum line prefab.")] private GameObject spectrumLinePrefab;
    [SerializeField, Tooltip("The starting / minimum scale for the spectrum.")] private float startingScale = 1f;
    [SerializeField, Tooltip("The maximum scaling for the spectrum.")] private float maxScale;
    [SerializeField, Tooltip("The width of the spectrum lines.")] private float spectrumWidth;
    [SerializeField, Tooltip("The color of the spectrum.")] private Color spectrumColor = Color.white;
    [SerializeField, Tooltip("Determines whether the audio visualizer uses the buffer for smoother spectrum transition.")] private bool useBuffer;
    [SerializeField, Tooltip("The space between each spectrum line.")] private float spacing;
    [SerializeField, Tooltip("Determines whether the spectrum is centered in the RectTransform or not.")] private bool isCentered;
    [Space(10)]

    [Header("Rainbow Settings")]
    [SerializeField, Tooltip("A rainbow option, because why not? Ignores the spectrum color.")] private bool enableRainbowSpectrum;
    [SerializeField, Tooltip("If true, the colors in the rainbow are cycled in a random order.")] private bool randomizeRainbowColors;
    [SerializeField, Tooltip("The speed of the rainbow color change.")] private float rainbowSpeed = 1f;

    private float[] samples;    //The array where the spectrum data is stored for each sample
    private GameObject[] sampleLines;   //The physical sample lines that visualize the spectrum data
    private float[] frequencyBands;     //The frequency bands of the visualizer
    private float[] spectrumBuffer;     //The buffer for the visualizer for smoother visualization
    private float[] spectrumDecrease;   //The speed at which each spectrum line should decrease

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
        frequencyBands = new float[8];

        if (displayFrequencyBands)
        {
            samples = new float[512];
            sampleLines = new GameObject[frequencyBands.Length];
        }
        else
        {
            sampleLines = new GameObject[numberOfSamples];
        }

        spectrumBuffer = new float[sampleLines.Length];
        spectrumDecrease = new float[sampleLines.Length];

        CreateSpectrum();
    }

    /// <summary>
    /// Generates the initial spectrum.
    /// </summary>
    private void CreateSpectrum()
    {
        float startingX = 0f;

        if (isCentered)
        {
            startingX = GetComponent<RectTransform>().rect.width / 2f;

            for(int i = (sampleLines.Length / 2) - 1; i >= 0; i--)
                startingX -= spectrumWidth + spacing;
        }

        for(int i = 0; i < sampleLines.Length; i++)
        {
            GameObject currentLine = Instantiate(spectrumLinePrefab, transform);
            RectTransform lineTransform = currentLine.GetComponent<RectTransform>();

            //Adjusts the width of the sample line
            lineTransform.sizeDelta = new Vector2(spectrumWidth, lineTransform.sizeDelta.y);

            //Adjusts the position based on the sample index
            float xPos = startingX + (i * spectrumWidth) + (spacing * i);

            lineTransform.anchoredPosition = new Vector2(xPos, 0f);

            //If the rainbow spectrum is not in use, set the color of the spectrum
            if(!enableRainbowSpectrum)
                currentLine.GetComponent<Image>().color = spectrumColor;

            currentLine.name = "SpectrumLine" + i.ToString("000");
            sampleLines[i] = currentLine;   //Adds to list of sample lines
        }

        //Start the rainbow color animation if active
        if (enableRainbowSpectrum)
            StartCoroutine(CycleRainbow());
    }

    void Update()
    {
        GetSpectrumAudioSource();

        if (displayFrequencyBands)
            MakeFrequencyBands();
        if (useBuffer)
            UpdateBuffer();

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
        float[] currentData = displayFrequencyBands ? frequencyBands : samples;

        for (int i = 0; i < sampleLines.Length; i++)
        {
            if(useBuffer)
                sampleLines[i].transform.localScale = new Vector3(transform.localScale.x, startingScale + (spectrumBuffer[i] * maxScale), transform.localScale.z);
            else
                sampleLines[i].transform.localScale = new Vector3(transform.localScale.x, startingScale + (currentData[i] * maxScale), transform.localScale.z);
        }
    }

    /// <summary>
    /// Makes data for the frequency bands.
    /// </summary>
    private void MakeFrequencyBands()
    {

        int count = 0;

        for(int i = 0; i < frequencyBands.Length; i++)
        {
            float average = 0f;

            /*
             * Gets the average of the current frequency band (represents a group of hertz).
             * This is done in a geometric sequence (2, 4, 8, 16, etc.) to get the full range of hertz in our spectrum data.
             */

            int sampleCount = (int)Mathf.Pow(2, i) * 2;

            if (i == 7)
                sampleCount += 2;

            //For each frequency band section, get the sample data and sum it up
            for (int j = 0; j < sampleCount; j++)
            {
                average += samples[count] * (count + 1);
                count++;
            }

            //Get the average of the spectrum data and save it
            average /= count;

            frequencyBands[i] = average * 10f;
        }
    }

    /// <summary>
    /// Updates the spectrum data with a buffer so that the data decreases with some smoothness to it.
    /// </summary>
    private void UpdateBuffer()
    {
        float[] currentData = displayFrequencyBands? frequencyBands : samples;

        for (int i = 0; i < currentData.Length; i++)
        {
            //If the current data is greater than the spectrum buffer, set it as the new buffer and reset its decrease speed
            if (currentData[i] > spectrumBuffer[i])
            {
                spectrumBuffer[i] = currentData[i];
                spectrumDecrease[i] = 0.005f;
            }
            //If the current data is less than the spectrum buffer, decrease the spectrum with a small amount of acceleration as it continues to be smaller
            if (currentData[i] < spectrumBuffer[i])
            {
                spectrumBuffer[i] -= spectrumDecrease[i];
                spectrumDecrease[i] *= 1.2f;
            }
        }
    }

    public void SetAudioSource(AudioSource newSource) => audioSource = newSource;
}
