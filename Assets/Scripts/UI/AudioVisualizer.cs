using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int numberOfSamples = 256;
    [SerializeField] private GameObject spectrumLinePrefab;
    [SerializeField] private float spacing;
    [SerializeField] private float maxScale;

    private float[] samples;
    private GameObject[] sampleLines;

    void Start()
    {
        samples = new float[numberOfSamples];
        sampleLines = new GameObject[numberOfSamples];
        CreateSpectrum();
    }

    private void CreateSpectrum()
    {
        for(int i = 0; i < sampleLines.Length; i++)
        {
            GameObject currentLine = Instantiate(spectrumLinePrefab);
            currentLine.transform.position = transform.position;
            currentLine.transform.parent = transform;
            Vector3 pos = currentLine.transform.localPosition;
            pos.x = pos.x + (spacing * i);
            currentLine.transform.localPosition = pos;
            currentLine.name = "SpectrumLine" + i.ToString("000");
            sampleLines[i] = currentLine;
        }
    }

    void Update()
    {
        GetSpectrumAudioSource();
        UpdateSpectrum();
    }

    private void GetSpectrumAudioSource()
    {
        // Get the audio data from the audio source
        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
    }

    private void UpdateSpectrum()
    {
        for(int i = 0; i < sampleLines.Length; i++)
            sampleLines[i].transform.localScale = new Vector3(1, 1 + (samples[i] * maxScale), 1);
    }
}
