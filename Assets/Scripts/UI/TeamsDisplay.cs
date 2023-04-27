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

    private Dictionary<string, int> playerTeams = new Dictionary<string, int>();

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
            StartCoroutine(SpawnTeamsAfterPlayerColorInit());
    }

    private void OnEnable()
    {
        SpawnTeamNames();
    }

    private void OnDisable()
    {
        RemoveTeamNames();
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

            InitializePlayerTeams();
            newTeam.RefreshPlayerList(playerTeams);
        }
    }

    private void RemoveTeamNames()
    {
        foreach (Transform trans in teamColorContainer)
            Destroy(trans.gameObject);
    }

    public void InitializePlayerTeams()
    {
        foreach(var player in NetworkManagerScript.instance.GetPlayerList())
        {
            if (!playerTeams.ContainsKey(player.NickName))
                playerTeams.Add(player.NickName, (int)player.CustomProperties["Color"]);
        }
    }

    public void UpdatePlayerTeams(string nickname, int newColor)
    {
        if (playerTeams.TryGetValue(nickname, out int currentPlayerColor))
        {
            if(newColor != currentPlayerColor)
            {
                playerTeams[nickname] = newColor;
                teamColorContainer.GetChild(currentPlayerColor).GetComponent<TeamListItem>().RefreshPlayerList(playerTeams);
                if (!teamColorContainer.GetChild(newColor).gameObject.activeInHierarchy)
                {
                    teamColorContainer.GetChild(newColor).gameObject.SetActive(true);
                    teamColorContainer.GetChild(newColor).GetComponent<TeamListItem>().RefreshPlayerList(playerTeams);
                }
            }
        }
    }
}
