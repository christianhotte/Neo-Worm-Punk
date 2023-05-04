using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class PlayerManagementDisplay : MonoBehaviour
{
    [SerializeField, Tooltip("The text for the player name.")] private TextMeshProUGUI playerNameText;
    [SerializeField, Tooltip("The confirm player kick text.")] private TextMeshProUGUI confirmPlayerKickText;
    [SerializeField] private GameObject kickPlayerDisplay;
    [SerializeField] private GameObject confirmKickPlayerDisplay;

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
        confirmPlayerKickText.text = "Are You Sure You Want To Kick " + playerData.NickName + "?";
    }

    public void ShowConfirmScreen(bool showConfirm)
    {
        kickPlayerDisplay.SetActive(!showConfirm);
        confirmKickPlayerDisplay.SetActive(showConfirm);
    }

    public void KickPlayer()
    {
        playerManagementController.KickPlayer(currentPlayerData);
        ShowConfirmScreen(false);
    }
}
