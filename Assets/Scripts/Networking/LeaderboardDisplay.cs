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
    [SerializeField, Tooltip("The background for the leaderboard when not connected to the network.")] private RectTransform notConnectedBackground;
    [SerializeField, Tooltip("The background for the leaderboard items.")] private RectTransform leaderboardBackground;
    [SerializeField, Tooltip("The container for the leaderboard items.")] private RectTransform leaderboardContainer;
    [SerializeField, Tooltip("The spacing between each leaderboard item.")] private float spacing;
    [SerializeField, Tooltip("The speed that the leaderboard items move when updating.")] private float itemMoveSpeed;
    [SerializeField, Tooltip("The ease type for the speed of the items moving.")] private LeanTweenType easeType;

    private float leaderBoardHeight;
    private List<LeaderboardItem> leaderBoardList = new List<LeaderboardItem>();

    private bool teamLeaderboard;


    private void Awake()
    {
        notConnectedBackground.gameObject.SetActive(!PhotonNetwork.IsConnected);
        leaderboardBackground.gameObject.SetActive(PhotonNetwork.IsConnected);
        leaderBoardHeight = leaderboardItemPrefab.GetComponent<RectTransform>().sizeDelta.y;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            InitializeLeaderboard();
        }
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

            newLeaderboardItem.SetTextColor(PlayerSettingsController.ColorOptionsToColor((ColorOptions)NetworkPlayer.instances[i].photonView.Owner.CustomProperties["Color"]));

            //Show a background on the local player's leaderboard stats
            if (NetworkPlayer.instances[i] == NetworkManagerScript.localNetworkPlayer)
                newLeaderboardItem.SetBackgroundOpacity(1f);

            leaderBoardList.Add(newLeaderboardItem);
        }
    }

    /// <summary>
    /// Updates the player's kills on the leaderboard.
    /// </summary>
    /// <param name="playerName">The player's name to search for in the list.</param>
    /// <param name="deaths">The new number of deaths for the player.</param>
    /// <param name="kills">The new number of deaths for the player.</param>
    /// <param name="streak">The new streak number for the player.</param>
    public void UpdatePlayerStats(string playerName, int deaths, int kills, int streak)
    {
        foreach(var player in leaderBoardList)
        {
            if(player.GetWormName() == playerName)
            {
                if(kills != player.GetKills())
                    player.UpdateKills(kills, streak);

                else if(deaths != player.GetDeaths())
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
        //Sort the list by the kills in descending order, and then the deaths in ascending order
        leaderBoardList = leaderBoardList.OrderByDescending(player => player.GetKills()).ThenBy(player => player.GetDeaths()).ToList();
        UpdateListPositions();
    }

    /// <summary>
    /// Update the physical list positions of the leaderboard items.
    /// </summary>
    private void UpdateListPositions()
    {
        int rank = 1;

        for (int i = 0; i < leaderBoardList.Count; i++)
        {
            if(i > 0)
            {
                //If the current leaderboard item is not equal to the one above it, give it the next rank
                if(!CurrentPlayerEqualRank(leaderBoardList[i], leaderBoardList[i - 1]))
                {
                    rank++;
                    leaderBoardList[i].UpdateRank(rank);
                }
            }
            //Always give the first leaderboard item the first rank
            else
                leaderBoardList[i].UpdateRank(rank);

            //If the leaderboard position is not in the right position, move it smoothly to the right position
            if (leaderBoardList[i].GetComponent<RectTransform>().anchoredPosition.y != GetPosition(i))
                LeanTween.moveY(leaderBoardList[i].GetComponent<RectTransform>(), GetPosition(i), itemMoveSpeed).setEase(easeType);
        }
    }

    private bool CurrentPlayerEqualRank(LeaderboardItem current, LeaderboardItem other) => (current.GetKills() == other.GetKills() && current.GetDeaths() == other.GetDeaths());

    private float GetPosition(int index) => -(leaderBoardHeight + spacing) * index;
}
