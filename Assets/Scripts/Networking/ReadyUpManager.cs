using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;

public class ReadyUpManager : MonoBehaviourPunCallbacks
{
    // Can probably have a button that lights up green or red to show if a player is ready through the network.
    //[SerializeField] private GameObject readyButton;
    public static ReadyUpManager instance;

    [SerializeField] private TextMeshProUGUI playerReadyText;
    public bool debugReadyUpAll;

    private const int MINIMUM_PLAYERS_NEEDED = 2;   // The minimum number of players needed for a round to start

    private int playersReady, playersInRoom;
    internal LockerTubeController localPlayerTube;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        if (debugReadyUpAll)
        {
            debugReadyUpAll = false;
            if (!GameManager.Instance.levelTransitionActive)
            {
                //Reset all players
                foreach (var player in NetworkPlayer.instances)
                    player.networkPlayerStats = new PlayerStats();

                NetworkManagerScript.instance.LoadSceneWithFade(GameSettings.arenaScene);
            }
        }
    }
    public void LeverStateChanged()
    {
        LeverController localLever = localPlayerTube.GetComponentInChildren<LeverController>();
        NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().isReady = (localLever.GetLeverState() == LeverController.HingeJointState.Max);
        NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["IsReady"] = NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().isReady;
        NetworkManagerScript.localNetworkPlayer.SyncStats();
        UpdateStatus(localPlayerTube.GetTubeNumber());
    }

    // Once the room is joined.
    public override void OnJoinedRoom()
    {
        playersInRoom = NetworkManagerScript.instance.GetMostRecentRoom().PlayerCount;
        UpdateReadyText();

        // If the amount of players in the room is maxed out, close the room so no more people are able to join.
/*        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }*/
    }

    // When a player leaves the room
    public override void OnLeftRoom()
    {
        if(NetworkManagerScript.instance.GetMostRecentRoom() != null)
        {
            if (NetworkManagerScript.instance.GetMostRecentRoom().PlayerCount > 0)
            {
                playersInRoom = NetworkManagerScript.instance.GetMostRecentRoom().PlayerCount;
                UpdateReadyText();
            }
        }
        UpdateReadyText();
    }

    public void UpdateStatus(int tubeID)
    {
        Debug.Log("Updating RPC...");
        photonView.RPC("RPC_UpdateReadyStatus", RpcTarget.AllBuffered, tubeID, NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().isReady);
    }

    // Tells the master server the amount of players that are ready to start the match.
    [PunRPC]
    public void RPC_UpdateReadyStatus(int tubeID, bool updatedPlayerReady)
    {
        if(FindObjectOfType<TubeManager>() != null)
        {
            LockerTubeController tube = FindObjectOfType<TubeManager>().GetTubeByNumber(tubeID);
            if (tube != null) tube.UpdateLights(updatedPlayerReady);

            // Get the number of players that have readied up
            playersReady = GetAllPlayersReady();
            playersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;

            UpdateReadyText();

            bool forceLoadViaMasterClient = false;

            foreach (var player in NetworkPlayer.instances)
            {
                print("Player " + player.photonView.ViewID + " ready status: " + (player.networkPlayerStats.isReady ? "READY" : "NOT READY"));
                if(player.photonView.Owner.IsMasterClient && player.networkPlayerStats.isReady)
                {
                    forceLoadViaMasterClient = true;
                }
            }

            // If all players are ready, load the game scene
            if (forceLoadViaMasterClient || !GameManager.Instance.levelTransitionActive && (playersReady == playersInRoom && (playersInRoom >= MINIMUM_PLAYERS_NEEDED || GameSettings.debugMode)))
            {
                //Reset all players
                foreach (var player in NetworkPlayer.instances)
                    player.networkPlayerStats = new PlayerStats();

                NetworkManagerScript.instance.LoadSceneWithFade(GameSettings.arenaScene);
            }
        }
    }

    [PunRPC]
    public void RPC_UpdateTubeOccupation(bool[] tubeStates)
    {
        for (int i = 0; i < 6; i++)
        {

        }
    }


    /// <summary>
    /// Updates the text in the center of the room.
    /// </summary>
    public void UpdateReadyText()
    {
        if (playerReadyText == null)
        {
            foreach (TextMeshProUGUI tmp in FindObjectsOfType<TextMeshProUGUI>())
            {
                if (tmp.gameObject.name == "PlayerReadyText") { playerReadyText = tmp; break; }
            }
            if (playerReadyText == null) return;
        }

        string message = "Players Ready: " + playersReady.ToString() + "/" + playersInRoom;

        if (playersInRoom < MINIMUM_PLAYERS_NEEDED && !GameSettings.debugMode)
        {
            message += "\n<size=25>Not Enough Players To Start.</size>";
        }

        Debug.Log(message);
        playerReadyText.text = message; // Display the message in the scene
    }

    /// <summary>
    /// Check all of the lever values to see if everyone is ready.
    /// </summary>
    private int GetAllPlayersReady()
    {
        int playersReady = 0;

        // Gets the amount of players that have a readied lever at lowest state.
        foreach (var players in FindObjectsOfType<NetworkPlayer>())
        {
            if (players.GetNetworkPlayerStats().isReady)
                playersReady++;
        }

        return playersReady;
    }
}