using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorOptions { DEFAULT, RED, ORANGE, YELLOW, GREEN, BLUE, TEAL, VIOLET, MAGENTA, BLACK }

public class PlayerColorChanger : MonoBehaviour
{
    private PhysicalButtonController[] colorButtons;
    private ColorOptions currentChosenColor = ColorOptions.DEFAULT;

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
        Color newColor;
        string newColorText;

        switch (colorOption)
        {
            case (int)ColorOptions.RED:
                newColor = new Color(197f / 255f, 17f / 255f, 17f / 255f);
                newColorText = "RED";
                break;
            case (int)ColorOptions.ORANGE:
                newColor = new Color(232f / 255f, 131f / 255f, 23f / 255f);
                newColorText = "ORANGE";
                break;
            case (int)ColorOptions.YELLOW:
                newColor = new Color(253f / 255f, 253f / 255f, 150f / 255f);
                newColorText = "YELLOW";
                break;
            case (int)ColorOptions.GREEN:
                newColor = Color.green;
                newColorText = "GREEN";
                break;
            case (int)ColorOptions.BLUE:
                newColor = Color.blue;
                newColorText = "BLUE";
                break;
            case (int)ColorOptions.TEAL:
                newColor = new Color(46f / 255f, 200f / 255f, 209f / 255f);
                newColorText = "TEAL";
                break;
            case (int)ColorOptions.VIOLET:
                newColor = new Color(52f / 255f, 31f / 255f, 224f / 255f);
                newColorText = "VIOLET";
                break;
            case (int)ColorOptions.MAGENTA:
                newColor = Color.magenta;
                newColorText = "MAGENTA";
                break;
            case (int)ColorOptions.BLACK:
                newColor = Color.black;
                newColorText = "BLACK";
                break;
            default:
                newColor = new Color(255f / 255f, 128f / 255f, 128f / 255f);
                newColorText = "DEFAULT";
                break;
        }

        NetworkManagerScript.instance.UpdateTakenColorList(currentChosenColor, (ColorOptions)colorOption);
        currentChosenColor = (ColorOptions)colorOption;

        PlayerSettingsController.Instance.charData.testColor = newColor;   //Set the player color in the character data

        PlayerController.instance.ApplyAndSyncSettings(); //Apply settings to player (NOTE TO PETER: Call this whenever you want to change a setting and sync it across the network)
        Debug.Log("Changing Player Color To " + newColorText);
    }

    public void RefreshButtons()
    {
        //Reset all buttons
        foreach(var button in colorButtons)
        {
            button.ShowText(false);
            button.EnableButton(true);
        }

        foreach (var color in NetworkManagerScript.instance.takenColors)
        {
            colorButtons[(int)color].ShowText(true);
            colorButtons[(int)color].EnableButton(false);
        }
    }
}
