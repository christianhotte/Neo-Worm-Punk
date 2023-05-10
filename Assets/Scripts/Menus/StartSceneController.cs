using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSceneController : MonoBehaviour
{
    [SerializeField, Tooltip("The Wormpunk SFX.")] private AudioClip wormPunkSFX;

    public void StartGame()
    {
        PlayerController.instance.inverteboy.PlayOneShotSound(wormPunkSFX);
        NetworkManagerScript.instance.LoadSceneWithFade(GameSettings.titleScreenScene, 1.5f);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
