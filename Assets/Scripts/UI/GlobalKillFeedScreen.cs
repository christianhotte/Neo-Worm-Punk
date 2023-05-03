using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalKillFeedScreen : MonoBehaviour
{
    [SerializeField, Tooltip("The kill feed death information prefab.")] private DeathInfo deathInfoPrefab;
    [SerializeField, Tooltip("The container for the information that displays the player stats information.")] private Transform deathInfoContainer;
    [SerializeField, Tooltip("The maximum amount of kills to show on the board.")] private int maxDeathsShown = 6;

    [Header("Debug Options:")]
    [SerializeField] private bool debugAddToFeed;

    private List<DeathInfo> killFeedList = new List<DeathInfo>();

    /// <summary>
    /// Add information to the death board and display it.
    /// </summary>
    /// <param name="killer">The name of the person who performed a kill.</param>
    /// <param name="victim">The name of the person killed.</param>
    /// <param name="causeOfDeath">An icon that indicates the cause of death.</param>
    public void AddToKillFeed(string killer, string victim, DeathCause causeOfDeath = DeathCause.UNKNOWN)
    {
        DeathInfo mostRecentDeath = Instantiate(deathInfoPrefab, deathInfoContainer);
        mostRecentDeath.UpdateDeathInformation(killer, victim, causeOfDeath);
        mostRecentDeath.gameObject.transform.localScale = Vector3.zero;

        //Scale the death info gameObject from 0 to 1 with an EaseOutCirc ease type.
        LeanTween.scale(mostRecentDeath.gameObject, Vector3.one, 0.5f).setEaseOutCirc();
        killFeedList.Add(mostRecentDeath);

        if(killFeedList.Count > maxDeathsShown)
        {
            DeathInfo deprecatedDeath = killFeedList[maxDeathsShown];
            killFeedList.Remove(deprecatedDeath);
            Destroy(deprecatedDeath.gameObject);
        }
    }

    private void Update()
    {
        if (debugAddToFeed)
        {
            debugAddToFeed = false;
            AddToKillFeed("Player 1", "Player 2", DeathCause.UNKNOWN);
        }
    }
}
