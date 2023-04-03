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
            "Kills:\t\t\t" + PlayerPrefs.GetInt("LifetimeKills") + "\n" +
            "Deaths:\t\t" + PlayerPrefs.GetInt("LifetimeDeaths");
        lifetimeStats.text = stats;
    }
}
