using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;

public class RoomSettingsDisplay : MonoBehaviour
{
    private TextMeshProUGUI roomSettingsText;   //The room settings that are displayed

    private void Awake()
    {
        roomSettingsText = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// Updates the text showcasing the room's settings for everyone.
    /// </summary>
    public void UpdateRoomSettingsDisplay()
    {
        string roomType = "Room Type: " + (GetRoom().IsVisible ? "Public" : "Private");

        string matchLength = "Match Length: ";
        int currentRoundLength = (int)GetRoom().CustomProperties["RoundLength"];
        if (currentRoundLength < 60)
            matchLength += currentRoundLength.ToString() + " second" + (currentRoundLength > 1 ? "s" : "");
        else
            matchLength += (currentRoundLength / 60).ToString() + " minute" + (currentRoundLength / 60 > 1 ? "s" : "");

        string playerHP = "Player HP: " + ((int)GetRoom().CustomProperties["PlayerHP"]).ToString();

        roomSettingsText.text = roomType + "\n" + matchLength + "\n" + playerHP;
    }

    public Room GetRoom() => PhotonNetwork.CurrentRoom;
}
