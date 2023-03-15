using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class SettingsController : MonoBehaviour
{
    public enum ColorOptions { WHITE, RED, BLUE, GREEN, PURPLE, BLACK}

    [SerializeField] private TextMeshProUGUI colorSettingsObject;

    public void ChangeHandColor(float colorOption)
    {
        Color newColor;
        string newColorText;

        Debug.Log((int)colorOption);

        switch ((int)colorOption)
        {
            case (int)ColorOptions.RED:
                newColor = Color.red;
                newColorText = "RED";
                break;
            case (int)ColorOptions.BLUE:
                newColor = Color.blue;
                newColorText = "BLUE";
                break;
            case (int)ColorOptions.GREEN:
                newColor = Color.green;
                newColorText = "GREEN";
                break;
            case (int)ColorOptions.PURPLE:
                newColor = Color.magenta;
                newColorText = "PURPLE";
                break;
            case (int)ColorOptions.BLACK:
                newColor = Color.black;
                newColorText = "BLACK";
                break;
            default:
                newColor = Color.white;
                newColorText = "DEFAULT";
                break;
        }

        PlayerSettingsController.Instance.charData.testColor = newColor;   //Set the player color in the character data
        PlayerController.instance.ApplyAndSyncSettings(); //Apply settings to player (NOTE TO PETER: Call this whenever you want to change a setting and sync it across the network)
        //FindObjectOfType<PlayerSetup>().SetColor(newColor);

        colorSettingsObject.text = "Player Color: " + newColorText;
    }
}
