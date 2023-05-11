using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CreateRoomController : MonoBehaviour
{
    private RoomOptions currentRoomOptions;
    private Hashtable customRoomSettings;

    [SerializeField] private TextMeshProUGUI roomTypeLabel;
    [SerializeField] private TextMeshProUGUI matchLengthLabel;
    [SerializeField] private TextMeshProUGUI playerHPLabel;

    private void Start()
    {
        DefaultRoomSettings();
    }

    private void OnEnable()
    {
        DefaultRoomSettings();
    }

    private void DefaultRoomSettings()
    {
        currentRoomOptions = new RoomOptions();
        customRoomSettings = new Hashtable();

        currentRoomOptions.IsVisible = true;
        customRoomSettings.Add("RoundLength", GameSettings.matchLengths[0]);
        customRoomSettings.Add("PlayerHP", GameSettings.HPDefault);

        UpdateRoomTypeLabel();
        UpdateMatchLengthLabel();
    }

    /// <summary>
    /// Determines whether the room is public or private.
    /// </summary>
    /// <param name="isPublic">If true, the room is public. If false, the room is private.</param>
    public void IsPublicRoom(bool isPublic)
    {
        currentRoomOptions.IsVisible = isPublic;
        UpdateRoomTypeLabel();
    }

    /// <summary>
    /// Determines the match length.
    /// </summary>
    /// <param name="currentMatchLength">The index of the match length array.</param>
    public void UpdateMatchLength(float currentMatchLength)
    {
        Debug.Log("Match Length: " + currentMatchLength);
        Debug.Log("Match Index: " + Mathf.RoundToInt(currentMatchLength));
        Debug.Log("Setting Match Length To " + GameSettings.matchLengths[Mathf.RoundToInt(currentMatchLength)]);
        customRoomSettings["RoundLength"] = GameSettings.matchLengths[Mathf.RoundToInt(currentMatchLength)];
        UpdateMatchLengthLabel();
    }

    public void UpdateRoomTypeLabel()
    {
        roomTypeLabel.text = "Room Type: " + (currentRoomOptions.IsVisible ? "Public" : "Private");
    }

    public void UpdateMatchLengthLabel()
    {
        matchLengthLabel.text = "Match Length: ";

        int currentRoundLength = (int)customRoomSettings["RoundLength"];

        if (currentRoundLength < 60)
        {
            matchLengthLabel.text += currentRoundLength.ToString() + " second" + (currentRoundLength > 1? "s": "");
        }
        else
            matchLengthLabel.text += (currentRoundLength / 60).ToString() + " minute" + (currentRoundLength / 60 > 1 ? "s" : "");
    }

    private string[] wormSymbols = { "OO", "EE", "G", "S", "W", "U", "M", "N" };

    /// <summary>
    /// Generates a random room code that is a set amount of letters long.
    /// </summary>
    /// <returns>A random string that serves as a room code.</returns>
    public string GenerateRoomCode()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        string roomCode = "";
        for(int i = 0; i < GameSettings.roomCodeLength; i++)
        {
            string currentLetter = wormSymbols[Random.Range(0, wormSymbols.Length)];
            roomCode += currentLetter;
        }

        return roomCode;
    }

    public RoomOptions GetRoomOptions() => currentRoomOptions;
    public Hashtable GetCustomRoomSettings() => customRoomSettings;

}
