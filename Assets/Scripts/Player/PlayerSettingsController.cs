using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

/// <summary>
/// Defines runtime information about a player which needs to / can be sent over the network.
/// </summary>;
public class CharacterData
{
    //Settings:
    public int playerID;                            //Unique number differentiating this player from others on the network
    public string playerName;                       //Name chosen by user to be displayed for other players on the network
    public Color testColor = new Color(1, 1, 1, 1); //DEMO SETTING: Color chosen by player to be seen by all other players over the network (defaults to white)
}

public class PlayerStats
{
    public bool isReady = false;

    public int numOfKills = 0;
    public int numOfDeaths = 0;
}