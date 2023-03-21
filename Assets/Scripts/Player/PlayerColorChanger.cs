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

        switch (colorOption)
        {
            case (int)ColorOptions.RED:
                newColor = new Color(197f / 255f, 17f / 255f, 17f / 255f);
                break;
            case (int)ColorOptions.ORANGE:
                newColor = new Color(232f / 255f, 131f / 255f, 23f / 255f);
                break;
            case (int)ColorOptions.YELLOW:
                newColor = new Color(253f / 255f, 253f / 255f, 150f / 255f);
                break;
            case (int)ColorOptions.GREEN:
                newColor = Color.green;
                break;
            case (int)ColorOptions.BLUE:
                newColor = Color.blue;
                break;
            case (int)ColorOptions.TEAL:
                newColor = new Color(46f / 255f, 200f / 255f, 209f / 255f);
                break;
            case (int)ColorOptions.VIOLET:
                newColor = new Color(52f / 255f, 31f / 255f, 224f / 255f);
                break;
            case (int)ColorOptions.MAGENTA:
                newColor = Color.magenta;
                break;
            case (int)ColorOptions.BLACK:
                newColor = Color.black;
                break;
            default:
                newColor = new Color(255f / 255f, 128f / 255f, 128f / 255f);
                break;
        }

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
        currentColorOptionSelected = (ColorOptions)colorOption;

        PlayerController.instance.ApplyAndSyncSettings(); //Apply settings to player (NOTE TO PETER: Call this whenever you want to change a setting and sync it across the network)
        Debug.Log("Changing Player Color To " + newColorText);
    }

    public void RefreshButtons()
    {
        foreach(var button in colorButtons)
        {
            button.ShowText(false);
            button.EnableButton(true);
        }

        foreach (var color in NetworkManagerScript.instance.takenColors)
        {
            colorButtons[color].ShowText(true);
            colorButtons[color].EnableButton(false);
        }
    }
}
