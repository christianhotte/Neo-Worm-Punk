using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSceneController : MonoBehaviour
{
    public void StartGame()
    {
        NetworkManagerScript.instance.LoadSceneWithFade(PlayerPrefs.GetInt("FirstRun") == 0? GameSettings.creditsScene: GameSettings.titleScreenScene);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
