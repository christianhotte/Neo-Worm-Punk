using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorOptions { DEFAULT, RED, ORANGE, YELLOW, GREEN, BLUE, TEAL, VIOLET, MAGENTA, BLACK }

public class PlayerColorChanger : MonoBehaviour
{
    private PhysicalButtonController[] colorButtons;
    private ColorOptions currentColorOptionSelected = ColorOptions.DEFAULT;

    // Start is called before the first frame update
    void Start()
    {
        colorButtons = GetComponentsInChildren<PhysicalButtonController>();
        for (int i = 0; i < colorButtons.Length; i++)
            AdjustButtonColor(colorButtons[i], i);
    }

    private void AdjustButtonColor(PhysicalButtonController currentButton, int colorOption)
    {
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
            case (int)ColorOptions.RED:
                newColorText = "RED";
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
            case (int)ColorOptions.BLUE:
                newColorText = "BLUE";
                break;
            case (int)ColorOptions.TEAL:
                newColorText = "TEAL";
                break;
            case (int)ColorOptions.VIOLET:
                newColorText = "VIOLET";
                break;
            case (int)ColorOptions.MAGENTA:
                newColorText = "MAGENTA";
                break;
            case (int)ColorOptions.BLACK:
                newColorText = "BLACK";
                break;
            default:
                newColorText = "DEFAULT";
                break;
        }

        Color newColor = PlayerSettingsController.ColorOptionsToColor((ColorOptions)colorOption);

        NetworkManagerScript.instance.UpdateTakenColorList(currentColorOptionSelected, (ColorOptions)colorOption);

        PlayerSettingsController.Instance.charData.playerColor = newColor;   //Set the player color in the character data
        NetworkManagerScript.localNetworkPlayer.photonView.Owner.CustomProperties["Color"] = (int)PlayerSettingsController.ColorToColorOptions(PlayerSettingsController.Instance.charData.playerColor);
        currentColorOptionSelected = (ColorOptions)colorOption;

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

        foreach (var color in NetworkManagerScript.instance.takenColors)
        {
            colorButtons[color].ShowText(true);
            colorButtons[color].LockButton(true);
            colorButtons[color].EnableButton(false);
        }
    }
}
