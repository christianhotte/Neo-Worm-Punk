using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


[System.Serializable]
public class WordStructure
{
    public string word;
    public int[] wordSounds;

    public WordStructure(string word, int[] wordSounds)
    {
        this.word = word;
        this.wordSounds = wordSounds;
    }
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
    private void Awake()
    {
        Instance = this;                //Create static reference to this settings container
        charData = new CharacterData(); //Create and store a fresh settings object upon instantiation
        charData.playerColor = playerColors[PlayerPrefs.GetInt("PreferredColorOption")];    //Set the player's color to their ideal color at the start
    }

    //UTILITY METHODS:
    public string CharDataToString() => JsonUtility.ToJson(charData); //Sends player settings to a string (transmissible over RPC)
    public static string PlayerStatsToString(PlayerStats playerStats) => JsonUtility.ToJson(playerStats); //Sends player stats to a string (transmissible over RPC)

    public static Color[] playerColors = { 
        new Color(247f / 255f, 128f / 255f, 128f / 255f),
        new Color(255f / 255f, 99f / 255f, 255f / 255f),
        new Color(232f / 255f, 131f / 255f, 23f / 255f),
        Color.yellow,
        Color.green,
        Color.cyan,
        new Color(52f / 255f, 31f / 255f, 224f / 255f),
        new Color(215f / 255f, 36f / 255f,  77f / 255f),
        Color.white,
    };

    public static int NumberOfPlayerColors() => playerColors.Length;

    public static Color ColorOptionsToColor(ColorOptions colorOption)
    {
        return playerColors[(int)colorOption];
    }

    public static ColorOptions ColorToColorOptions(Color currentColor)
    {
        for(int i = 0; i < playerColors.Length; i++)
        {
            if (playerColors[i] == currentColor)
                return (ColorOptions)i;
        }

        return ColorOptions.DEFAULT;
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
    public WordStructure playerAdjective;           //The player's adjective
    public WordStructure playerNoun;                //The player's noun
    public Color playerColor = PlayerSettingsController.ColorOptionsToColor(ColorOptions.DEFAULT); //DEMO SETTING: Color chosen by player to be seen by all other players over the network (defaults to flesh)
}

public class PlayerStats
{
    public bool isReady = false;

    public int numOfKills = 0;
    public int numOfDeaths = 0;
    public int killStreak = 0;
    public int deathStreak = 0;
    public string teamColor = "";
    // In damage calculation, if other player is the same color, you do no damage

    public static string[] streakMessages =
    {
        "",
        "",
        "Double Kill.",
        "Triple Kill.",
        "Quadruple Kill.",
        "Quintuple Kill.",
    };
}