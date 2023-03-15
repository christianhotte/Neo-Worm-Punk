using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

// Code was used from https://youtu.be/KGzMxalSqQE?t=675

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI text;
    Player player;
    private string playerListName;

    // Displays the names of players in the room.
    public void SetUp(string playerName)
    {
        text.text = playerName;
        playerListName = playerName;
    }

    // Compares to the player that has left the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Our player has left the room, so remove the name from the list.
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    // If we leave the room, we can't see the players anymore.
    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }

    public string GetName() => playerListName;
}