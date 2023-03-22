using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ColorData
{
    public ColorOptions colorOption;
    public Color playerColor;
}

/// <summary>
/// Contains settings object (relevant to local player) and functionality for sending it over the network.
/// </summary>
public class PlayerSettingsController : MonoBehaviour
{
    //Objects & Components:
    public static PlayerSettingsController Instance; //Singleton reference to this script at runtime (each player uses only their own local version)
    public CharacterData charData;         //Object where player settings are set, stored and sent from

    //RUNTIME METHODS:
    private void OnEnable()
    {
        Instance = this;                //Create static reference to this settings container
        charData = new CharacterData(); //Create and store a fresh settings object upon instantiation
    }

    //UTILITY METHODS:
    public string CharDataToString() => JsonUtility.ToJson(charData); //Sends player settings to a string (transmissible over RPC)
    public static string PlayerStatsToString(PlayerStats playerStats) => JsonUtility.ToJson(playerStats); //Sends player stats to a string (transmissible over RPC)

    public static Color ColorOptionsToColor(ColorOptions colorOption)
    {
        switch (colorOption)
        {
            case ColorOptions.RED:
                return new Color(197f / 255f, 17f / 255f, 17f / 255f);
            case ColorOptions.ORANGE:
                return new Color(232f / 255f, 131f / 255f, 23f / 255f);
            case ColorOptions.YELLOW:
                return new Color(253f / 255f, 253f / 255f, 150f / 255f);
            case ColorOptions.GREEN:
                return Color.green;
            case ColorOptions.BLUE:
                return Color.blue;
            case ColorOptions.TEAL:
                return new Color(46f / 255f, 200f / 255f, 209f / 255f);
            case ColorOptions.VIOLET:
                return new Color(52f / 255f, 31f / 255f, 224f / 255f);
            case ColorOptions.MAGENTA:
                return Color.magenta;
            case ColorOptions.BLACK:
                return Color.black;
            default:
                return new Color(255f / 255f, 128f / 255f, 128f / 255f);
        }
    }
}

/// <summary>
/// Defines runtime information about a player which needs to / can be sent over the network.
/// </summary>;
public class CharacterData
{
    //Settings:
    public int playerID;                            //Unique number differentiating this player from others on the network
    public string playerName;                       //Name chosen by user to be displayed for other players on the network
    public Color playerColor = PlayerSettingsController.ColorOptionsToColor(ColorOptions.DEFAULT); //DEMO SETTING: Color chosen by player to be seen by all other players over the network (defaults to flesh)
}

public class PlayerStats
{
    public bool isReady = false;

    public int numOfKills = 0;
    public int numOfDeaths = 0;
}