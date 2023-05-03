using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatHUDController : MonoBehaviour
{
    [SerializeField, Tooltip("The container for the information that displays the player stats information.")] private Transform playerStatsContainer;
    [SerializeField, Tooltip("The container for the information that displays the death information.")] private Transform deathInfoContainer;
    [SerializeField, Tooltip("The death information prefab.")] private DeathInfo deathInfoPrefab;
    [SerializeField, Tooltip("The game HUD combat container.")] private GameObject combatInfoTransform;
    [SerializeField, Tooltip("The game HUD kill feed container.")] private Transform killFeedTransform;
    [SerializeField, Tooltip("The prefab for the kill message.")] private GameObject killMessagePrefab;
    [SerializeField, Tooltip("The image of the helmet overlay.")] private Image helmetOverlay;
    [SerializeField, Tooltip("The image of the helmet outline.")] private Image helmetOutline;
    [SerializeField, Tooltip("The background of the speedometer.")] private Image speedometerBackground;
    [SerializeField, Tooltip("The container for the upgrade notifications.")] private Transform upgradeContainer;
    [SerializeField, Tooltip("The upgrade info prefab.")] private UpgradeDisplay upgradeInfoPrefab;
    [SerializeField, Tooltip("The ammo indicators.")] private Transform[] ammoIndicators;
    [SerializeField, Tooltip("The ammo pip prefab.")] private Image ammoPip;

    /// <summary>
    /// Changes the color of the helmet.
    /// </summary>
    /// <param name="newColor">The new color of the helmet.</param>
    public void ChangeHelmetColor(Color newColor)
    {
        helmetOverlay.color = newColor;
        helmetOutline.color = newColor;

        float speedometerAlpha = speedometerBackground.color.a;
        speedometerBackground.color = new Color(newColor.r, newColor.g, newColor.b, speedometerAlpha);
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
    /// Adds to the upgrade info container.
    /// </summary>
    /// <param name="powerUpType">The type of power up.</param>
    /// <param name="powerUpTime">The amount of time for the powerup.</param>
    public void AddToUpgradeInfo(PowerUp.PowerUpType powerUpType, float powerUpTime)
    {
        Debug.Log("Adding Upgrade To Combat HUD Info...");
        UpgradeDisplay currentUpgrade = Instantiate(upgradeInfoPrefab, upgradeContainer);
        currentUpgrade.StartUpgradeTimer(powerUpType, powerUpTime);
    }

    public void InitializeAmmoIndicators(CustomEnums.Handedness handedness, int ammoCount)
    {
        Transform currentAmmoIndicator = ammoIndicators[handedness == CustomEnums.Handedness.Left ? 0 : 1];

        for (int i = 0; i < ammoCount; i++)
            Instantiate(ammoPip, currentAmmoIndicator);
    }

    public void UpdateAmmoIndicator(CustomEnums.Handedness handedness, int currentAmmo)
    {
        Transform currentAmmoIndicator = ammoIndicators[handedness == CustomEnums.Handedness.Left ? 0 : 1];

        foreach (Transform trans in currentAmmoIndicator)
            trans.Find("AmmoIndicator").gameObject.SetActive(false);

        for (int i = 0; i < currentAmmoIndicator.childCount; i++)
        {
            if(i < currentAmmo)
                currentAmmoIndicator.GetChild(i).Find("AmmoIndicator").gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Add information to the death board and display it.
    /// </summary>
    /// <param name="killer">The name of the person who performed a kill.</param>
    /// <param name="victim">The name of the person killed.</param>
    /// <param name="causeOfDeath">An icon that indicates the cause of death.</param>
    public void AddToDeathInfoBoard(string killer, string victim, DeathCause causeOfDeath = DeathCause.UNKNOWN)
    {
        DeathInfo deathInfo = Instantiate(deathInfoPrefab, deathInfoContainer);
        deathInfo.UpdateDeathInformation(killer, victim, causeOfDeath);
        deathInfo.gameObject.transform.localScale = Vector3.zero;

        //Scale the death info gameObject from 0 to 1 with an EaseOutCirc ease type. Call the DestroyWithDelay after the animation is complete
        LeanTween.scale(deathInfo.gameObject, Vector3.one, 0.5f).setEaseOutCirc().setOnComplete(() => DestroyWithDelay(deathInfo.gameObject, 3, 2));

        GameObject currentKill = Instantiate(killMessagePrefab, killFeedTransform);
        currentKill.GetComponentInChildren<TextMeshProUGUI>().text = "Killed " + victim;

        PlayerController.instance.inverteboy.Flash();
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

    public void EnableCombatHUD(bool enableCombatHUD) => combatInfoTransform.SetActive(enableCombatHUD);
}
