using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthIndicator : MonoBehaviour
{
    [SerializeField, Tooltip("The health image prefab.")] private GameObject healthImagePrefab;
    [SerializeField, Tooltip("The color of the health pips.")] private Color healthPipColor;
    [SerializeField, Tooltip("The transparency of the health pips when health is lost.")] private float healthDamageAlpha;

    /// <summary>
    /// Generates all of the health icons based on the player's health.
    /// </summary>
    public void GenerateHealthIcons()
    {
        for (int i = 0; i < PlayerController.instance.MaxHealth; i++)
        {
            GameObject newPip = Instantiate(healthImagePrefab, transform);
            newPip.GetComponent<Image>().color = healthPipColor;
        }
    }

    /// <summary>
    /// Updates the health icon's alpha based on the player's current health.
    /// </summary>
    /// <param name="currentHealth">The player's current health.</param>
    public void UpdateHealth(float currentHealth)
    {
        CanvasGroup[] healthImages = GetComponentsInChildren<CanvasGroup>();
        for(int i = 0; i < healthImages.Length; i++)
        {
            if (i < currentHealth)
                healthImages[i].alpha = 1f;
            else
                healthImages[i].alpha = healthDamageAlpha;
        }
    }
}
