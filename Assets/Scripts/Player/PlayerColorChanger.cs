using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum ColorOptions { DEFAULT, PINK, ORANGE, YELLOW, GREEN, CYAN, VIOLET, RAZZMATAZZ, WHITE }

public class PlayerColorChanger : MonoBehaviour
{
    private PhysicalButtonController[] colorButtons;

    private void Awake()
    {
        colorButtons = GetComponentsInChildren<PhysicalButtonController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Color Buttons Length: " + colorButtons.Length);
        for (int i = 0; i < colorButtons.Length; i++)
            AdjustButtonColor(colorButtons[i], i);

        if (PhotonNetwork.IsConnected)
            StartCoroutine(RefreshButtonsOnStart());
        else
            RefreshOfflineButtons();
    }

    /// <summary>
    /// Waits until the player has a color to refresh the buttons.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RefreshButtonsOnStart()
    {
        yield return new WaitUntil(() => NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["Color"] != null);
        RefreshButtons();
    }

    private void AdjustButtonColor(PhysicalButtonController currentButton, int colorOption)
    {
        Debug.Log("Changing Initial Button Color To: " + (ColorOptions)colorOption);

        Color newColor;

        newColor = PlayerSettingsController.playerColors[colorOption];

        currentButton.ChangeButtonColor(newColor, true);
    }

    /// <summary>
    /// Changes the player's color.
    /// </summary>
    /// <param name="colorOption">The color option for the player.</param>
    public void ChangePlayerColor(int colorOption)
    {
        string newColorText;

        switch (colorOption)
        {
            case (int)ColorOptions.PINK:
                newColorText = "PINK";
                // Change player stats to the color you change to (string)
                if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"] == true)
                {
                    NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties(NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().teamColor, "PINK");
                    NetworkManagerScript.localNetworkPlayer.SyncStats();
                }
                break;
            case (int)ColorOptions.ORANGE:
                newColorText = "ORANGE";
                if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"] == true)
                {
                    NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties(NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().teamColor, "ORANGE");
                    NetworkManagerScript.localNetworkPlayer.SyncStats();
                }
                break;
            case (int)ColorOptions.YELLOW:
                newColorText = "YELLOW";
                if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"] == true)
                {
                    NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties(NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().teamColor, "YELLOW");
                    NetworkManagerScript.localNetworkPlayer.SyncStats();
                }
                break;
            case (int)ColorOptions.GREEN:
                newColorText = "GREEN";
                if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"] == true)
                {
                    NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties(NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().teamColor, "GREEN");
                    NetworkManagerScript.localNetworkPlayer.SyncStats();
                }
                break;
            case (int)ColorOptions.CYAN:
                newColorText = "CYAN";
                if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"] == true)
                {
                    NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties(NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().teamColor, "CYAN");
                    NetworkManagerScript.localNetworkPlayer.SyncStats();
                }
                break;
            case (int)ColorOptions.VIOLET:
                newColorText = "VIOLET";
                if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"] == true)
                {
                    NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties(NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().teamColor, "VIOLET");
                    NetworkManagerScript.localNetworkPlayer.SyncStats();
                }
                break;
            case (int)ColorOptions.RAZZMATAZZ:
                newColorText = "RAZZMATAZZ";
                if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"] == true)
                {
                    NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties(NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().teamColor, "RAZZMATAZZ");
                    NetworkManagerScript.localNetworkPlayer.SyncStats();
                }
                break;
            case (int)ColorOptions.WHITE:
                newColorText = "WHITE";
                if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"] == true)
                {
                    NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties(NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().teamColor, "WHITE");
                    NetworkManagerScript.localNetworkPlayer.SyncStats();
                }
                break;
            default:
                newColorText = "DEFAULT";
                if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"] == true)
                {
                    NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties(NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().teamColor, "DEFAULT");
                    NetworkManagerScript.localNetworkPlayer.SyncStats();
                }
                break;
        }

        Color newColor = PlayerSettingsController.ColorOptionsToColor((ColorOptions)colorOption);

        PlayerSettingsController.Instance.charData.playerColor = newColor;   //Set the player color in the character data

        if(PhotonNetwork.IsConnected)
            NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties("Color", colorOption);
        else
        {
            PlayerPrefs.SetInt("PreferredColorOption", colorOption);
            RefreshOfflineButtons();
        }

        PlayerController.instance.ApplyAndSyncSettings(); //Apply settings to player (NOTE TO PETER: Call this whenever you want to change a setting and sync it across the network)
        Debug.Log("Changing Player Color To " + newColorText);
    }

    public void RefreshButtons()
    {
        foreach(var button in colorButtons)
        {
            button.ShowText(false);
            button.LockButton(false);
            button.EnableButton(true);
        }

        foreach (var player in NetworkManagerScript.instance.GetPlayerList())
        {
            colorButtons[(int)player.CustomProperties["Color"]].ShowText(true);
            colorButtons[(int)player.CustomProperties["Color"]].LockButton(true);
            colorButtons[(int)player.CustomProperties["Color"]].EnableButton(false);
        }
    }

    private void RefreshOfflineButtons()
    {
        foreach (var button in colorButtons)
        {
            button.ShowText(false);
            button.LockButton(false);
            button.EnableButton(true);
        }

        colorButtons[PlayerPrefs.GetInt("PreferredColorOption")].ShowText(true);
        colorButtons[PlayerPrefs.GetInt("PreferredColorOption")].LockButton(true);
        colorButtons[PlayerPrefs.GetInt("PreferredColorOption")].EnableButton(false);
    }
}
