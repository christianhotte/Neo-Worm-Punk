using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inverteboy : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        UpdateVolume();
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

    public void StopMusic() => audioSource.Stop();
    private void UpdateVolume() => audioSource.volume = PlayerPrefs.GetFloat("MusicVolume", GameSettings.defaultMusicSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound);
}
