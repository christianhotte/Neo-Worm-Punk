using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartSceneController : MonoBehaviour
{
    [SerializeField, Tooltip("The Wormpunk SFX.")] private AudioClip wormPunkSFX;
    [SerializeField, Tooltip("The message that lets the player know that this is cannot be undone.")] private TextMeshProUGUI clearDataConfirmText;

    private bool clearPlayerDataConfirmed = false;  //Checks to ensure that the player wants to clear their player data.
    private bool playerDataCleared = false;         //If true, the player has already cleared their data

    public void StartGame()
    {
        PlayerController.instance.inverteboy.PlayOneShotSound(wormPunkSFX);
        NetworkManagerScript.instance.LoadSceneWithFade(GameSettings.titleScreenScene, 1.5f);
    }

    public void ClearPlayerData()
    {
        if (!clearPlayerDataConfirmed)
        {
            clearDataConfirmText.text = "Are You Sure You Want To Clear Your Player Data?\n\nThis Action Cannot Be Undone.";
            clearPlayerDataConfirmed = true;
        }
        else if(!playerDataCleared)
        {
            clearDataConfirmText.text = "Player Data Successfully Cleared.";
            ClearData();

            playerDataCleared = true;
            ClearConfirmText(3);        //Fade text after a delay
        }
    }

    private void ClearData()
    {
        PlayerPrefs.DeleteAll();
        AchievementListener.Instance.ClearAchievements();
        PlayerController.instance.inverteboy.UpdateVolume();
    }

    private void ClearConfirmText(float delay)
    {
        LeanTween.delayedCall(delay, () => LeanTween.alphaText(clearDataConfirmText.GetComponent<RectTransform>(), 0f, 2f).setEase(LeanTweenType.easeOutCirc));
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
