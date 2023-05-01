using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

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
       //Debug.Log("Color Buttons Length: " + colorButtons.Length);
        for (int i = 0; i < colorButtons.Length; i++)
            AdjustButtonColor(colorButtons[i], i);

        if (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name != GameSettings.titleScreenScene)
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

    /// <summary>
    /// Adjusts the color of a button to reflect what color it gives the player.
    /// </summary>
    /// <param name="currentButton">The button color to change.</param>
    /// <param name="colorOption">The color to change the button to.</param>
    private void AdjustButtonColor(PhysicalButtonController currentButton, int colorOption)
    {
        //Debug.Log("Changing Initial Button Color To: " + (ColorOptions)colorOption);

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
        if (PhotonNetwork.IsConnected)
        {
            NetworkManagerScript.localNetworkPlayer.ChangePlayerColorData(colorOption);                     //Change the player color data on the Network Player

            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"])                               //Manually refresh buttons if on team mode to display the current color on the Network Player
                RefreshButtons();
        }
        else
        {
            Color newColor = PlayerSettingsController.ColorOptionsToColor((ColorOptions)colorOption);       //Converts the color option to a color object to store locally
            PlayerSettingsController.Instance.charData.playerColor = newColor;                              //Sets the player color in the character data
            PlayerPrefs.SetInt("PreferredColorOption", colorOption);                                        //Sets the preferred color option in PlayerPrefs so that they can 
            RefreshOfflineButtons();                                                                        //Refreshes the buttons according to the PlayerPref assigned
            PlayerController.instance.ApplyAndSyncSettings();                                               //Apply settings to player
        }

        //Debug.Log("Changing Player Color To " + PlayerSettingsController.ColorToString(colorOption));
    }

    /// <summary>
    /// Refreshes the buttons while connected to the network.
    /// </summary>
    public void RefreshButtons()
    {
        //Unlock all buttons
        foreach(var button in colorButtons)
        {
            button.ShowText(false);
            button.LockButton(false);
            button.EnableButton(true);
        }

        //If not on teams, make the player colors exclusive
        if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"])
        {
            //Lock the color options for all existing players in the room
            foreach (var player in NetworkManagerScript.instance.GetPlayerList())
            {
                if (player.CustomProperties["Color"] != null)
                {
                    colorButtons[(int)player.CustomProperties["Color"]].ShowText(true);
                    colorButtons[(int)player.CustomProperties["Color"]].LockButton(true);
                    colorButtons[(int)player.CustomProperties["Color"]].EnableButton(false);
                }
            }
        }
        //If on teams, make the player colors inclusive. This only shows the color that the local player has selected
        else
        {
            colorButtons[(int)NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["Color"]].ShowText(true);
            colorButtons[(int)NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["Color"]].LockButton(true);
            colorButtons[(int)NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["Color"]].EnableButton(false);
        }
    }

    /// <summary>
    /// Refreshes the appearance of the buttons when a player is offline.
    /// </summary>
    private void RefreshOfflineButtons()
    {
        //Unlock all buttons
        foreach (var button in colorButtons)
        {
            button.ShowText(false);
            button.LockButton(false);
            button.EnableButton(true);
        }

        //Lock the button for the current player's preferred color
        colorButtons[PlayerPrefs.GetInt("PreferredColorOption")].ShowText(true);
        colorButtons[PlayerPrefs.GetInt("PreferredColorOption")].LockButton(true);
        colorButtons[PlayerPrefs.GetInt("PreferredColorOption")].EnableButton(false);
    }
}
