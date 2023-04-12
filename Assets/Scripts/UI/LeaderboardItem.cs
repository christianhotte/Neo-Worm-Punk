using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardItem : MonoBehaviour
{
    [SerializeField, Tooltip("The text to display the rank of the player.")] private TextMeshProUGUI ranking;
    [SerializeField, Tooltip("The text to display the worm name.")] private TextMeshProUGUI wormName;
    [SerializeField, Tooltip("The text to display the kills.")] private TextMeshProUGUI kills;
    [SerializeField, Tooltip("The text to display the deaths.")] private TextMeshProUGUI deaths;
    [SerializeField, Tooltip("The text to display the streak.")] private TextMeshProUGUI streak;

    /// <summary>
    /// Sets the information for the leaderboard item.
    /// </summary>
    /// <param name="rank">The rank of the player.</param>
    /// <param name="name">The name of the player.</param>
    /// <param name="k">The player's total kills.</param>
    /// <param name="d">The player's total deaths.</param>
    /// <param name="s">The player's current kill streak.</param>
    public void SetLeaderboardInformation(int rank, string name, int k, int d, int s)
    {
        wormName.text = name;
        UpdateRank(rank);
        UpdateKills(k, s);
        UpdateDeaths(d);
    }

    public void UpdateKills(int k, int s)
    {
        kills.text = k.ToString();
        streak.text = s.ToString();
    }

    public void UpdateDeaths(int d)
    {
        deaths.text = d.ToString();
        streak.text = "0";
    }

    public void UpdateRank(int rank)
    {
        ranking.text = rank.ToString();
    }

    public int GetRanking() => int.Parse(ranking.text);
    public string GetWormName() => wormName.text;
    public int GetKills() => int.Parse(kills.text);
    public int GetDeaths() => int.Parse(deaths.text);
    public int GetStreak() => int.Parse(streak.text);
}
