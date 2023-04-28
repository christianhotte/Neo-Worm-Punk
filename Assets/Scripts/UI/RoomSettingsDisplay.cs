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
        string teamsMode = "Teams Mode: " + ((bool)GetRoom().CustomProperties["TeamMode"]? "On" : "Off");
        string hazardsActive = "Hazards Active: " + ((bool)GetRoom().CustomProperties["HazardsActive"] ? "On" : "Off");
        string upgradesActive = "Upgrades Active: " + ((bool)GetRoom().CustomProperties["UpgradesActive"] ? "On" : "Off");
        string upgradeFrequency = "Upgrade Frequency: ";

        switch (GameSettings.UpgradeFrequencyToInt((float)GetRoom().CustomProperties["UpgradeFrequency"]))
        {
            case 0:
                upgradeFrequency += "Low";
                break;
            case 1:
                upgradeFrequency += "Medium";
                break;
            case 2:
                upgradeFrequency += "High";
                break;
        }

        string upgradeLength = "Upgrade Length: ";

        switch (GameSettings.UpgradeLengthToInt((float)GetRoom().CustomProperties["UpgradeLength"]))
        {
            case 0:
                upgradeLength += "Short";
                break;
            case 1:
                upgradeLength += "Medium";
                break;
            case 2:
                upgradeLength += "Long";
                break;
        }

        roomSettingsText.text = roomType + "\n" + matchLength + "\n" + playerHP + "\n" + teamsMode + "\n" + hazardsActive + "\n" + upgradesActive + (((bool)GetRoom().CustomProperties["UpgradesActive"])? ("\n" + upgradeFrequency).ToString(): "") + (((bool)GetRoom().CustomProperties["UpgradesActive"]) ? ("\n" + upgradeLength).ToString() : "");
    }

    public Room GetRoom() => PhotonNetwork.CurrentRoom;
}
