using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Jumbotron : MonoBehaviour
{
    [SerializeField, Tooltip("The container for the information that displays the player stats information.")] private Transform deathInfoContainer;
    [SerializeField, Tooltip("The death information prefab.")] private DeathInfo deathInfoPrefab;
    [SerializeField, Tooltip("The most recent kill text.")] private TextMeshProUGUI mostRecentDeathText;

    private DeathInfo mostRecentDeath;  //The most recent death recorded

    /// <summary>
    /// Add information to the death board and display it.
    /// </summary>
    /// <param name="killer">The name of the person who performed a kill.</param>
    /// <param name="victim">The name of the person killed.</param>
    /// <param name="causeOfDeath">An icon that indicates the cause of death.</param>
    public void AddToDeathInfoBoard(string killer, string victim, Image causeOfDeath = null)
    {
        //Destroy the second most recent death
        if(mostRecentDeath != null)
            Destroy(mostRecentDeath.gameObject);
        else
        {
            mostRecentDeathText.gameObject.SetActive(true);
            mostRecentDeathText.gameObject.transform.localScale = Vector3.zero;
            LeanTween.scale(mostRecentDeathText.gameObject, Vector3.one, 0.5f).setEaseOutCirc();
        }

        mostRecentDeath = Instantiate(deathInfoPrefab, deathInfoContainer);
        mostRecentDeath.UpdateDeathInformation(killer, victim, causeOfDeath);
        mostRecentDeath.gameObject.transform.localScale = Vector3.zero;

        //Scale the death info gameObject from 0 to 1 with an EaseOutCirc ease type.
        LeanTween.scale(mostRecentDeath.gameObject, Vector3.one, 0.5f).setEaseOutCirc();
    }
}
