using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Voice.PUN;
using Photon.Voice.Unity;

public class AudioSettingsController : MonoBehaviour
{
    [SerializeField, Tooltip("The slider for the master volume.")] private Slider masterSlider;
    [SerializeField, Tooltip("The slider for the music volume.")] private Slider musicSlider;
    [SerializeField, Tooltip("The slider for the sfx volume.")] private Slider sfxSlider;
    [SerializeField, Tooltip("The slider for the voice chat volume.")] private Slider voiceChatSlider;
    [SerializeField, Tooltip("The toggle for mute mic.")] private Toggle muteMicToggle;

    private void OnEnable()
    {
        UpdateSettings();
    }

    /// <summary>
    /// Updates the values of the selectables on the menu.
    /// </summary>
    private void UpdateSettings()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound) * 10f;
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", GameSettings.defaultMusicSound) * 10f;
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * 10f;
        voiceChatSlider.value = PlayerPrefs.GetFloat("VoiceChatVolume", GameSettings.defaultVoiceSound) * 10f;
        muteMicToggle.isOn = PlayerPrefs.GetInt("MuteMic", 0) == 1? true: false;
    }

    /// <summary>
    /// Adjusts the master volume PlayerPref.
    /// </summary>
    /// <param name="newVolume">The new volume.</param>
    public void AdjustMasterVolume(float newVolume)
    {
        PlayerPrefs.SetFloat("MasterVolume", newVolume / 10f);
        PlayerController.instance.inverteboy.UpdateVolume();
    }

    /// <summary>
    /// Adjusts the music volume PlayerPref.
    /// </summary>
    /// <param name="newVolume">The new volume.</param>
    public void AdjustMusicVolume(float newVolume)
    {
        PlayerPrefs.SetFloat("MusicVolume", newVolume / 10f);
        PlayerController.instance.inverteboy.UpdateVolume();
    }

    /// <summary>
    /// Adjusts the SFX volume PlayerPref.
    /// </summary>
    /// <param name="newVolume">The new volume.</param>
    public void AdjustSFXVolume(float newVolume)
    {
        PlayerPrefs.SetFloat("SFXVolume", newVolume / 10f);
    }

    /// <summary>
    /// Adjusts the voice chat volume PlayerPref.
    /// </summary>
    /// <param name="newVolume">The new volume.</param>
    public void AdjustVoiceChatVolume(float newVolume)
    {
        PlayerPrefs.SetFloat("VoiceChatVolume", newVolume / 10f);
        NetworkManagerScript.instance.AdjustVoiceVolume();
    }

    /// <summary>
    /// Adjusts the mute mic PlayerPref.
    /// </summary>
    /// <param name="isOn">If true, mute the user's microphone. If false, let them talk.</param>
    public void ToggleMuteMic(bool isOn)
    {
        PlayerPrefs.SetInt("MuteMic", isOn? 1: 0);
        UpdateMuteMic();
    }

    private void UpdateMuteMic()
    {
        FindObjectOfType<Recorder>().TransmitEnabled = PlayerPrefs.GetInt("MuteMic") != 1;
    }
}
