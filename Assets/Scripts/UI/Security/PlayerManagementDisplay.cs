using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class PlayerManagementDisplay : MonoBehaviour
{
    [SerializeField, Tooltip("The text for the player name.")] private TextMeshProUGUI playerNameText;

    private PlayerManagementController playerManagementController;
    private Player currentPlayerData;

    private void Awake()
    {
        playerManagementController = FindObjectOfType<PlayerManagementController>();
    }

    public void InitializePlayerData(Player playerData)
    {
        currentPlayerData = playerData;
        playerNameText.text = playerData.NickName;
    }

    public void KickPlayer()
    {
        playerManagementController.KickPlayer(currentPlayerData);
    }
}
