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
            "Kills:\t\t\t\t\t\t" + PlayerPrefs.GetInt("LifetimeKills") + "\n" +
            "Deaths:\t\t\t\t\t" + PlayerPrefs.GetInt("LifetimeDeaths") + "\n" +
            "Best Kill Streak:\t\t\t\t" + PlayerPrefs.GetInt("BestStreak") + "\n" +
            "Highest Death Streak:\t\t\t" + PlayerPrefs.GetInt("HighestDeathStreak") + "\n" +
            "Best Tutorial Completion Time:\t" + (PlayerPrefs.GetFloat("BestTutorialTime") == 0f ? "N/A" : DisplayTutorialCompletionTime());
        lifetimeStats.text = stats;
    }

    private string DisplayTutorialCompletionTime()
    {
        float tutorialTime = PlayerPrefs.GetFloat("BestTutorialTime");
        string minutes = Mathf.FloorToInt(tutorialTime / 60f < 0 ? 0 : tutorialTime / 60f).ToString();
        string seconds = Mathf.FloorToInt(tutorialTime % 60f < 0 ? 0 : tutorialTime % 60f).ToString("00");
        string centiseconds = Mathf.FloorToInt((tutorialTime * 100f) % 100f < 0 ? 0 : (tutorialTime * 100f) % 100f).ToString("00");

        return minutes + ":" + seconds + ":" + centiseconds;
    }
}
