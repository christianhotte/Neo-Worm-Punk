using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FunnyScreen : MonoBehaviour
{
    [SerializeField, Tooltip("The offline screen.")] private GameObject offlineBackground;
    [SerializeField, Tooltip("The funny screen.")] private GameObject funnyBackground;
    [Space(10)]

    [SerializeField, Tooltip("If true, the screen can turn into a funny screen.")] private bool allowFunnyScreen;
    [SerializeField, Tooltip("The chance for the screen to be funny."), Range(0, 100)] private float chanceForFunnyScreen;

    private void Start()
    {
        CheckForFunnyScreen();
    }

    /// <summary>
    /// Randomly adds a funny screen if the player is not connected to the network and is allowed.
    /// </summary>
    private void CheckForFunnyScreen()
    {
        if (!PhotonNetwork.IsConnected && allowFunnyScreen)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
            float chanceForFunny = Random.Range(0, 100);
            if (chanceForFunny <= chanceForFunnyScreen)
            {
                offlineBackground.SetActive(false);
                funnyBackground.SetActive(true);

                if (!AchievementListener.Instance.IsAchievementUnlocked(7))
                    AchievementListener.Instance.UnlockAchievement(7);
            }
        }
    }
}
