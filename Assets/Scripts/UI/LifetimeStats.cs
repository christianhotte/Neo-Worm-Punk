using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LifetimeStats : MonoBehaviour
{
    private TextMeshProUGUI lifetimeStats;

    private void Start()
    {
        lifetimeStats = GetComponent<TextMeshProUGUI>();
        ShowLifeTimeStats();
    }

    /// <summary>
    /// Shows the lifetime stats on text.
    /// </summary>
    private void ShowLifeTimeStats()
    {
        string stats = "" +
            "Kills:\t\t\t\t" + PlayerPrefs.GetInt("LifetimeKills") + "\n" +
            "Deaths:\t\t\t" + PlayerPrefs.GetInt("LifetimeDeaths") + "\n" +
            "Best Kill Streak:\t\t" + PlayerPrefs.GetInt("BestStreak") + "\n" +
            "Highest Death Streak:\t" + PlayerPrefs.GetInt("HighestDeathStreak");
        lifetimeStats.text = stats;
    }
}
