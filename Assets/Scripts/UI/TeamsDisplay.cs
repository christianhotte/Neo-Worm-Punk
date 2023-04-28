using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TeamsDisplay : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private RectTransform teamColorDisplay;

    [Header("Containers")]
    [SerializeField] private Transform teamColorContainer;

    private List<TeamListItem> teamLists = new List<TeamListItem>();

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
            StartCoroutine(SpawnTeamsAfterPlayerColorInit());
    }

    private void OnEnable()
    {
        RefreshTeamLists();
    }

    private IEnumerator SpawnTeamsAfterPlayerColorInit()
    {
        yield return new WaitUntil(() => NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["Color"] != null);
        SpawnTeamNames();
    }

    private void SpawnTeamNames()
    {
        for(int i = 0; i < PlayerSettingsController.NumberOfPlayerColors(); i++)
        {
            TeamListItem newTeam = Instantiate(teamColorDisplay, teamColorContainer).GetComponent<TeamListItem>();
            newTeam.ChangeBackgroundColor(i);
            newTeam.ChangeLabel(i);
            teamLists.Add(newTeam);
        }

        RefreshTeamLists();
    }

    public void RefreshTeamLists()
    {
        for(int i = 0; i < teamLists.Count; i++)
        {
            GetPlayerTeams();
            teamLists[i].RefreshPlayerList(GetPlayerTeams());
        }
    }

    public Dictionary<string, int> GetPlayerTeams()
    {
        Dictionary <string, int> playerTeams = new Dictionary<string, int>();
        foreach(var player in NetworkManagerScript.instance.GetPlayerList())
        {
            if(player.CustomProperties["Color"] != null)
                playerTeams.Add(player.NickName, (int)player.CustomProperties["Color"]);
        }

        return playerTeams;
    }
}
