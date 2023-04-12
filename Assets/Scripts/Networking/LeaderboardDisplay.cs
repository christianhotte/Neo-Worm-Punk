using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;

public class LeaderboardDisplay : MonoBehaviour
{
    [SerializeField, Tooltip("The leaderboard item prefab.")] private LeaderboardItem leaderboardItemPrefab;
    [SerializeField, Tooltip("The container for the leaderboard items.")] private RectTransform leaderboardContainer;
    [SerializeField, Tooltip("The spacing between each leaderboard item.")] private float spacing;
    [SerializeField, Tooltip("The speed that the leaderboard items move when updating.")] private float itemMoveSpeed;
    [SerializeField, Tooltip("The ease type for the speed of the items moving.")] private LeanTweenType easeType;

    private float leaderBoardHeight;
    private List<LeaderboardItem> leaderBoardList = new List<LeaderboardItem>();

    private void Awake()
    {
        leaderBoardHeight = leaderboardItemPrefab.GetComponent<RectTransform>().sizeDelta.y;
    }

    private void Start()
    {
        InitializeLeaderboard();
    }

    /// <summary>
    /// Initializes the leaderboard with empty stats.
    /// </summary>
    private void InitializeLeaderboard()
    {
        for(int i = 0; i < NetworkPlayer.instances.Count; i++)
        {
            LeaderboardItem newLeaderboardItem = Instantiate(leaderboardItemPrefab, leaderboardContainer);
            Vector3 currentPosition = newLeaderboardItem.GetComponent<RectTransform>().anchoredPosition;
            currentPosition.y = GetPosition(i);
            newLeaderboardItem.GetComponent<RectTransform>().anchoredPosition = currentPosition;

            newLeaderboardItem.SetLeaderboardInformation(1, NetworkPlayer.instances[i].photonView.Owner.NickName, 0, 0, 0);
            leaderBoardList.Add(newLeaderboardItem);
        }
    }

    /// <summary>
    /// Updates the player's kills on the leaderboard.
    /// </summary>
    /// <param name="playerName">The player's name to search for in the list.</param>
    /// <param name="kills">The new number of deaths for the player.</param>
    /// <param name="streak">The new streak number for the player.</param>
    public void UpdatePlayerKills(string playerName, int kills, int streak)
    {
        foreach(var player in leaderBoardList)
        {
            if(player.GetWormName() == playerName)
            {
                player.UpdateKills(kills, streak);
                SortLeaderboard();  //Sort the leaderboard
                break;
            }
        }
    }

    /// <summary>
    /// Updates the player's deaths on the leaderboard.
    /// </summary>
    /// <param name="playerName">The player's name to search for in the list.</param>
    /// <param name="deaths">The new number of deaths for the player.</param>
    public void UpdatePlayerDeaths(string playerName, int deaths)
    {
        foreach (var player in leaderBoardList)
        {
            if (player.GetWormName() == playerName)
            {
                player.UpdateDeaths(deaths);
                SortLeaderboard();  //Sort the leaderboard
                break;
            }
        }
    }

    /// <summary>
    /// Sorts the leaderboard by kills first and deaths afterwards.
    /// </summary>
    private void SortLeaderboard()
    {
        for(int i = 1; i < leaderBoardList.Count; i++)
        {
            LeaderboardItem currentPlayer = leaderBoardList[i];
            LeaderboardItem previousPlayer = leaderBoardList[i-1];

            //If the current player's rank is not equal to the previous player's rank
            if (!CurrentPlayerEqualRank(currentPlayer, previousPlayer))
            {
                //If the current player is higher rank
                if(CurrentPlayerHigherRank(currentPlayer, previousPlayer))
                {
                    //If the current player is lower ranked than the previous one, raise the current player's rank
                    if(currentPlayer.GetRanking() > previousPlayer.GetRanking())
                        currentPlayer.UpdateRank(currentPlayer.GetRanking() - 1);

                    //If the current player is equal rank to the previous one, lower the previous player's rank
                    if (currentPlayer.GetRanking() == previousPlayer.GetRanking())
                        previousPlayer.UpdateRank(previousPlayer.GetRanking() + 1);
                }

                //If the current player is lower rank
                else
                {
                    //If the current player's rank is less than or equal to the previous player's rank, lower the previous player's rank
                    if (currentPlayer.GetRanking() <= previousPlayer.GetRanking())
                        currentPlayer.UpdateRank(currentPlayer.GetRanking() + 1);
                }
            }
            //If the current player's rank is equal to the previous player, but their rank numbers are not equal, take the previous player's rank number
            else if (currentPlayer.GetRanking() != previousPlayer.GetRanking())
            {
                currentPlayer.UpdateRank(previousPlayer.GetRanking());
            }
        }

        leaderBoardList.OrderBy(player => player.GetRanking()).ToList();    //Sort the list by the ranking
        UpdateListPositions();
    }

    /// <summary>
    /// Update the physical list positions of the leaderboard items.
    /// </summary>
    private void UpdateListPositions()
    {
        for (int i = 0; i < leaderBoardList.Count; i++)
        {
            //If the leaderboard position is not in the right position, move it smoothly to the right position
            if(leaderBoardList[i].GetComponent<RectTransform>().anchoredPosition.y != GetPosition(i))
                LeanTween.moveY(leaderBoardList[i].GetComponent<RectTransform>(), GetPosition(i), itemMoveSpeed).setEase(easeType);
        }
    }

    private bool CurrentPlayerEqualRank(LeaderboardItem current, LeaderboardItem other) => (current.GetKills() == other.GetKills() && current.GetDeaths() == other.GetDeaths());
    private bool CurrentPlayerHigherRank(LeaderboardItem current, LeaderboardItem other) => !(current.GetKills() < other.GetKills() && current.GetDeaths() > other.GetDeaths());

    private float GetPosition(int index) => -(leaderBoardHeight + spacing) * index;
}
