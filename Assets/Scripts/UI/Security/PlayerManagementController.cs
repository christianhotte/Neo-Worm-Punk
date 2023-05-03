using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerManagementController : MonoBehaviour
{
    [SerializeField] private Transform playerManagementContainer;
    [SerializeField] private PlayerManagementDisplay playerManagementPrefab;

    private void OnEnable()
    {
        UpdatePlayerList();
    }

    public void UpdatePlayerList()
    {
        foreach (Transform trans in playerManagementContainer)
            Destroy(trans.gameObject);

        foreach(var player in NetworkManagerScript.instance.GetPlayerList())
        {
            //If the player is not the master client, show the players on the screen
            if (!player.IsMasterClient)
            {
                PlayerManagementDisplay newPlayerDisplay = Instantiate(playerManagementPrefab, playerManagementContainer);
                newPlayerDisplay.InitializePlayerData(player);
            }
        }
    }

    /// <summary>
    /// Forcefully closes the connection of the player.
    /// </summary>
    /// <param name="kickPlayer">The current player to kick.</param>
    public void KickPlayer(Player kickPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Kicking " + kickPlayer.NickName + "...");
            PhotonNetwork.CloseConnection(kickPlayer);
        }
    }
}
