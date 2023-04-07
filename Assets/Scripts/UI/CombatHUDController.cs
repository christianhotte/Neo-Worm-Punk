using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatHUDController : MonoBehaviour
{
    [SerializeField, Tooltip("The information window image.")] private Image infoWindow;
    [SerializeField, Tooltip("The container for the information that displays the player stats information.")] private Transform playerStatsContainer;
    [SerializeField, Tooltip("The container for the information that displays the death information.")] private Transform deathInfoContainer;
    [SerializeField, Tooltip("The death information prefab.")] private DeathInfo deathInfoPrefab;

    private Color defaultInfoWindowColor;
    [SerializeField, Tooltip("The color the the window flashes when the player gets a kill.")] private Color flashInfoWindowColor;
    [SerializeField, Tooltip("The speed of the kill flash.")] private float flashSpeed;
    [SerializeField, Tooltip("The number of flashes.")] private int flashNumber;

    private IEnumerator flashAnimation;

    private void Awake()
    {
        defaultInfoWindowColor = infoWindow.color;
    }

    /// <summary>
    /// Update the player stats on the combat HUD.
    /// </summary>
    /// <param name="playerStats">The player's stats.</param>
    public void UpdatePlayerStats(PlayerStats playerStats)
    {
        playerStatsContainer.Find("PlayerDeaths").GetComponent<TextMeshProUGUI>().text = "D: " + playerStats.numOfDeaths;
        playerStatsContainer.Find("PlayerKills").GetComponent<TextMeshProUGUI>().text = "K: " + playerStats.numOfKills;
    }

    /// <summary>
    /// Add information to the death board and display it.
    /// </summary>
    /// <param name="killer">The name of the person who performed a kill.</param>
    /// <param name="victim">The name of the person killed.</param>
    /// <param name="causeOfDeath">An icon that indicates the cause of death.</param>
    public void AddToDeathInfoBoard(string killer, string victim, Image causeOfDeath = null)
    {
        DeathInfo deathInfo = Instantiate(deathInfoPrefab, deathInfoContainer);
        deathInfo.UpdateDeathInformation(killer, victim, causeOfDeath);
        deathInfo.gameObject.transform.localScale = Vector3.zero;

        //Scale the death info gameObject from 0 to 1 with an EaseOutCirc ease type. Call the DestroyWithDelay after the animation is complete
        LeanTween.scale(deathInfo.gameObject, Vector3.one, 0.5f).setEaseOutCirc().setOnComplete(() => DestroyWithDelay(deathInfo.gameObject, 3, 2));

        //Flashes the information window
        if (flashAnimation != null)
            StopCoroutine(flashAnimation);

        flashAnimation = FlashInfoWindow();
        StartCoroutine(flashAnimation);
    }

    private IEnumerator FlashInfoWindow()
    {
        for(int i = 0; i < flashNumber * 2; i++)
        {
            infoWindow.color = infoWindow.color == defaultInfoWindowColor ? flashInfoWindowColor : defaultInfoWindowColor;
            yield return new WaitForSeconds(flashSpeed);
        }
    }

    /// <summary>
    /// Destroys the death board object with a slight delay.
    /// </summary>
    /// <param name="deathInfoGameObject">The current death info gameObject.</param>
    /// <param name="waitDelay">The amount of time to display the death info for.</param>
    /// <param name="endAnimationDelay">The amount of time to play the exit animation.</param>
    private void DestroyWithDelay(GameObject deathInfoGameObject, float waitDelay, float endAnimationDelay)
    {
        //After a delay, fade out the death info canvas and then destroy it.
        LeanTween.delayedCall(waitDelay, () => LeanTween.alphaCanvas(deathInfoGameObject.GetComponent<CanvasGroup>(), 0f, endAnimationDelay).setDestroyOnComplete(true));
    }
}
