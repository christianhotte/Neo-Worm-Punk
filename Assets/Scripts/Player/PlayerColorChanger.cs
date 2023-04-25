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
                break;
            case (int)ColorOptions.ORANGE:
                newColorText = "ORANGE";
                break;
            case (int)ColorOptions.YELLOW:
                newColorText = "YELLOW";
                break;
            case (int)ColorOptions.GREEN:
                newColorText = "GREEN";
                break;
            case (int)ColorOptions.CYAN:
                newColorText = "CYAN";
                break;
            case (int)ColorOptions.VIOLET:
                newColorText = "VIOLET";
                break;
            case (int)ColorOptions.RAZZMATAZZ:
                newColorText = "RAZZMATAZZ";
                break;
            case (int)ColorOptions.WHITE:
                newColorText = "WHITE";
                break;
            default:
                newColorText = "DEFAULT";
                break;
        }

        Color newColor = PlayerSettingsController.ColorOptionsToColor((ColorOptions)colorOption);

        PlayerSettingsController.Instance.charData.playerColor = newColor;   //Set the player color in the character data

        if (PhotonNetwork.IsConnected)
        {
            NetworkManagerScript.localNetworkPlayer.SetNetworkPlayerProperties("Color", colorOption);
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"])
                RefreshButtons();
        }
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

        //If not on teams, make the player colors exclusive
        if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"])
        {
            foreach (var player in NetworkManagerScript.instance.GetPlayerList())
            {
                colorButtons[(int)player.CustomProperties["Color"]].ShowText(true);
                colorButtons[(int)player.CustomProperties["Color"]].LockButton(true);
                colorButtons[(int)player.CustomProperties["Color"]].EnableButton(false);
            }
        }
        //If on teams, make the player colors inclusive
        else
        {
            colorButtons[(int)NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["Color"]].ShowText(true);
            colorButtons[(int)NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["Color"]].LockButton(true);
            colorButtons[(int)NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["Color"]].EnableButton(false);
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
