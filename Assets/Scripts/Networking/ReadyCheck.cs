using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class ReadyCheck : MonoBehaviour
{
    private bool playerReady = false;

    // Creates a hashtable containing info on if all players are ready on the network.
    private ExitGames.Client.Photon.Hashtable playerCustomProperties = new ExitGames.Client.Photon.Hashtable();
    private bool allPlayersReady = false;
    //private bool allPlayersReady => PhotonNetwork.PlayerList.All(player => player.CustomProperties["playerReady"] == true);
    

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.SetPlayerCustomProperties(playerCustomProperties);

        playerCustomProperties["playerReady"] = playerReady;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayerReady()
    {
        // You don't affect other players
        if (!PhotonNetwork.IsMasterClient) return;


    }

    // Master server has to check until everyone is ready.
    void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // you can even limit the check to make it only if "playerReady" is among the changed properties
        if (!changedProps.Contains("playerReady")) return;

        // Loads everybody into the scene number in the build settings
        if (allPlayersReady)
        {
            PhotonNetwork.LoadLevel(3);
        }

        // Do nothing until everybody in the room is ready.
        else
        {
            return;
        }
    }
}