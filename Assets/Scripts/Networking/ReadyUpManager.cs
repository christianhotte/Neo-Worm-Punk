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

    private int playersReady;
    internal LockerTubeController localPlayerTube;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    //Called when a scene is loaded
    private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
/*        //If the locker room was loaded, update the locker text and the player colors
        if(scene.name == GameSettings.roomScene)
        {
            playersInRoom = NetworkManagerScript.instance.GetMostRecentRoom().PlayerCount;
            UpdateReadyText();
        }*/
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
        NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties("IsReady", NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().isReady);
        NetworkManagerScript.localNetworkPlayer.SyncStats();
        UpdateStatus(localPlayerTube.GetTubeNumber());
    }

    // Once the room is joined.
    public override void OnJoinedRoom()
    {
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
            tubeID = tubeID - 1;
            LockerTubeController tube = FindObjectOfType<TubeManager>().GetTubeByNumber(tubeID);
            if (tube != null) tube.UpdateLights(updatedPlayerReady);

            // Get the number of players that have readied up
            int playersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;

            UpdateReadyText();

            bool forceLoadViaMasterClient = false;

            foreach (var player in NetworkPlayer.instances)
            {
                print("Player " + player.photonView.ViewID + " ready status: " + (player.networkPlayerStats.isReady ? "READY" : "NOT READY"));

                //If in debug mode, force the game to load when the master client readies up
                if(player.photonView.Owner.IsMasterClient && player.networkPlayerStats.isReady && GameSettings.debugMode)
                    forceLoadViaMasterClient = true;
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
        List<LockerTubeController> roomTubes = FindObjectOfType<TubeManager>().roomTubes;

        for (int i = 0; i < roomTubes.Count; i++)
        {
            //roomTubes[i] = 
        }
    }


    /// <summary>
    /// Updates the text in the center of the room.
    /// </summary>
    public void UpdateReadyText()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            playersReady = GetAllPlayersReady();
            if (playerReadyText == null)
            {
                foreach (TextMeshProUGUI tmp in FindObjectsOfType<TextMeshProUGUI>())
                {
                    if (tmp.gameObject.name == "PlayerReadyText") { playerReadyText = tmp; break; }
                }

                if (playerReadyText == null) return;
            }

            string message = "Players Ready: " + playersReady.ToString() + "/" + PhotonNetwork.CurrentRoom.PlayerCount;

            if (PhotonNetwork.CurrentRoom.PlayerCount < MINIMUM_PLAYERS_NEEDED && !GameSettings.debugMode)
            {
                message += "\n<size=25>Not Enough Players To Start.</size>";
            }

            Debug.Log(message);
            playerReadyText.text = message; // Display the message in the scene
        }
        else
            playerReadyText.text = "Not Connected To The Network";
    }

    /// <summary>
    /// Hides the host settings in all of the tubes.
    /// </summary>
    public void HideTubeHostSettings()
    {
        //Hide the host settings for all tubes
        foreach (var tube in FindObjectOfType<TubeManager>().roomTubes)
            tube.ShowHostSettings(false);
    }

    /// <summary>
    /// Check all of the lever values to see if everyone is ready.
    /// </summary>
    private int GetAllPlayersReady()
    {
        int playersReady = 0;

/*        // Gets the amount of players that have a readied lever at lowest state.
        foreach (var players in FindObjectsOfType<NetworkPlayer>())
        {
            if (players.GetNetworkPlayerStats().isReady)
                playersReady++;
        }*/

        foreach(var player in NetworkManagerScript.instance.GetPlayerList())
        {
            if ((bool)player.CustomProperties["IsReady"])
                playersReady++;
        }

        return playersReady;
    }
}