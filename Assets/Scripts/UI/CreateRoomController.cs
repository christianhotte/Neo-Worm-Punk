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

    private int[] matchLengths = { 30, 60, 120, 300, 420 };

    [SerializeField] private TextMeshProUGUI roomTypeLabel;
    [SerializeField] private TextMeshProUGUI matchLengthLabel;

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
        customRoomSettings.Add("RoundLength", matchLengths[0]);

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
    /// <param name="currentMatchLength">The index</param>
    public void UpdateMatchLength(float currentMatchLength)
    {
        Debug.Log("Match Length: " + currentMatchLength);
        Debug.Log("Match Index: " + Mathf.RoundToInt(currentMatchLength));
        Debug.Log("Setting Match Length To " + matchLengths[Mathf.RoundToInt(currentMatchLength)]);
        customRoomSettings["RoundLength"] = matchLengths[Mathf.RoundToInt(currentMatchLength)];
        UpdateMatchLengthLabel();
    }

    public void UpdateRoomTypeLabel()
    {
        roomTypeLabel.text = "Room Type: " + (currentRoomOptions.IsVisible ? "Public" : "Private");
    }

    public void UpdateMatchLengthLabel()
    {
        matchLengthLabel.text = "Match Length: ";

        Debug.Log("Match Length: " + customRoomSettings["RoundLength"].ToString());

        int currentRoundLength = (int)customRoomSettings["RoundLength"];

        if (currentRoundLength < 60)
        {
            matchLengthLabel.text += currentRoundLength.ToString() + " second" + (currentRoundLength > 1? "s": "");
        }
        else
            matchLengthLabel.text += (currentRoundLength / 60).ToString() + " minute" + (currentRoundLength / 60 > 1 ? "s" : "");
    }

    /// <summary>
    /// Generates a random room code that is a set amount of letters long.
    /// </summary>
    /// <returns>A random string that serves as a room code.</returns>
    public string GenerateRoomCode()
    {
        string roomCode = "";
        for(int i = 0; i < GameSettings.roomCodeLength; i++)
        {
            char currentLetter = (char)('A' + Random.Range(0, 26));
            roomCode += currentLetter;
        }

        return roomCode;
    }

    public RoomOptions GetRoomOptions() => currentRoomOptions;
    public Hashtable GetCustomRoomSettings() => customRoomSettings;

}
