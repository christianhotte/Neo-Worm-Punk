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
    [SerializeField] private string sceneToLoad = "DM_0.14_Arena";

    private const int MINIMUM_PLAYERS_NEEDED = 2;   // The minimum number of players needed for a round to start

    private int playersReady, playersInRoom;
    internal LockerTubeController localPlayerTube;

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); } else { instance = this; }
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        instance = null;
    }
    public void LeverStateChanged()
    {
        LeverController localLever = localPlayerTube.GetComponentInChildren<LeverController>();
        NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().isReady = (localLever.GetLeverState() == LeverController.HingeJointState.Max);
        NetworkManagerScript.localNetworkPlayer.SyncStats();
        UpdateStatus(localPlayerTube.tubeNumber);
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

        // The room becomes open to let more people come in.
/*        if (PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (PhotonNetwork.InRoom) PhotonNetwork.CurrentRoom.IsOpen = true;
        }*/
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
        LockerTubeController tube = LockerTubeController.GetTubeByNumber(tubeID);
        if (tube != null) tube.UpdateLights(updatedPlayerReady);

        // Get the number of players that have readied up
        playersReady = GetAllPlayersReady();
        playersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;

        UpdateReadyText();

        // If all players are ready, load the game scene
        if (playersReady == playersInRoom && (playersInRoom >= MINIMUM_PLAYERS_NEEDED || GameSettings.debugMode))
        {
            //Reset all players
            foreach (var player in NetworkPlayer.instances)
                player.networkPlayerStats = new PlayerStats();

            NetworkManagerScript.instance.LoadSceneWithFade(sceneToLoad);
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
    private void UpdateReadyText()
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
        foreach(var players in FindObjectsOfType<NetworkPlayer>())
        {
            if (players.GetNetworkPlayerStats().isReady)
                playersReady++;
        }

        return playersReady;
    }
}