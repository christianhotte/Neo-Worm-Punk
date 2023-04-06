using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum GameMode { TimeAttack, CaptureTheFlag }

public class GameModeDisplay
{
    public static string DisplayGameMode(GameMode currentGameMode)
    {
        switch (currentGameMode)
        {
            case GameMode.TimeAttack:
                return "Time Attack";
            case GameMode.CaptureTheFlag:
                return "Capture The Flag";
            default:
                return "Game Mode";
        }
    }
}

public class HostSettingsController : MonoBehaviour
{
    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI matchLengthLabel;
    [SerializeField] private TextMeshProUGUI playerHPLabel;
    [Space(10)]

    [Header("Objects")]
    [SerializeField] private LeverController roomTypeController;
    [SerializeField] private DialRotationController matchDial;
    [SerializeField] private SliderController HPSlider;
    [Space(10)]

    [Header("Game Mode and Preset Settings")]
    [SerializeField] private TextMeshProUGUI gameModeLabel;
    [SerializeField] private GameObject[] gameModePanels;
    [SerializeField] private LockController gameModeArea;
    [SerializeField] private LockController presetsArea;
    [SerializeField] private Transform gameModeCapsuleSpawner;
    [SerializeField] private GameObject capsulePrefab;

    private bool isInitialized = false; //Checks to see if the current room settings are initialized on the room settings UI
    private GameMode currentGameMode = GameMode.TimeAttack;
    private GameObject currentGameModePanel;

    private void Awake()
    {
        currentGameModePanel = gameModePanels[(int)currentGameMode];
    }

    private void OnEnable()
    {
        //Wait a frame before calling this so that all of the objects can call their OnEnable functions first
        Invoke("InitiateRoomSettings", Time.deltaTime);
        gameModeArea.OnUnlocked.AddListener(SetGameMode);
        presetsArea.OnUnlocked.AddListener(SetPreset);
    }

    private void OnDisable()
    {
        isInitialized = false;
        gameModeArea.OnUnlocked.RemoveListener(SetGameMode);
        presetsArea.OnUnlocked.RemoveListener(SetPreset);
    }

    /// <summary>
    /// Moves the different objects to the correct values set in the room options and room settings.
    /// </summary>
    private void InitiateRoomSettings()
    {
        if (!isInitialized)
        {
            //Moves the handle to either the top or the bottom, depending on whether the room is public or not
            roomTypeController.GetLeverHandle().MoveToAngle(roomTypeController, GetRoom().IsVisible ? roomTypeController.GetMinimumAngle() : roomTypeController.GetMaximumAngle());

            //Moves the dial to the index of the match lengths array that the custom room setting is equal to
            matchDial.ResetDial();
            matchDial.MoveDial(Array.IndexOf(GameSettings.matchLengths, (int)GetRoom().CustomProperties["RoundLength"]));

            Debug.Log("Player HP: " + (int)GetRoom().CustomProperties["PlayerHP"]);

            //Moves the slider to the HP value
            HPSlider.MoveToValue((int)GetRoom().CustomProperties["PlayerHP"]);

            UpdateMatchLengthLabel();
            UpdatePlayerHPLabel();

            NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();

            isInitialized = true;
        }
    }

    /// <summary>
    /// Determines whether the room is public or private.
    /// </summary>
    /// <param name="isPublic">If true, the room is public. If false, the room is private.</param>
    public void IsPublicRoom(bool isPublic)
    {
        GetRoom().IsVisible = isPublic;
        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
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
        UpdateRoomSetting("RoundLength", GameSettings.matchLengths[Mathf.RoundToInt(currentMatchLength)]);
        UpdateMatchLengthLabel();
        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
    }

    /// <summary>
    /// Updates the number of hit points a player will have.
    /// </summary>
    /// <param name="playerHealth">The number of HP the player should have.</param>
    public void UpdatePlayerHealth(float playerHealth)
    {
        UpdateRoomSetting("PlayerHP", Mathf.RoundToInt(playerHealth));
        UpdatePlayerHPLabel();
        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
    }

    /// <summary>
    /// Updates the match length label text.
    /// </summary>
    public void UpdateMatchLengthLabel()
    {
        matchLengthLabel.text = "Match Length: ";

        Debug.Log("Match Length: " + GetRoom().CustomProperties["RoundLength"].ToString());

        int currentRoundLength = (int)GetRoom().CustomProperties["RoundLength"];

        if (currentRoundLength < 60)
            matchLengthLabel.text += currentRoundLength.ToString() + " second" + (currentRoundLength > 1 ? "s" : "");
        else
            matchLengthLabel.text += (currentRoundLength / 60).ToString() + " minute" + (currentRoundLength / 60 > 1 ? "s" : "");
    }

    /// <summary>
    /// Updates the label of the Player HP text.
    /// </summary>
    public void UpdatePlayerHPLabel()
    {
        playerHPLabel.text = "Player HP: " + ((int)GetRoom().CustomProperties["PlayerHP"]).ToString();
    }

    /// <summary>
    /// Updates a room setting and sets it in the room's custom properties.
    /// </summary>
    /// <param name="key">The custom property to change.</param>
    /// <param name="value">The value of the custom property.</param>
    private void UpdateRoomSetting(string key, object value)
    {
        Hashtable currentRoomSettings = GetRoom().CustomProperties;
        currentRoomSettings[key] = value;
        GetRoom().SetCustomProperties(currentRoomSettings);
    }

    public void SpawnGameModeCapsule(int gameMode)
    {
        //Destroys any other existing game capsule
        foreach (var capsule in FindObjectsOfType<SettingsCapsule>())
            Destroy(capsule.gameObject);

        GameObject newGameMode = Instantiate(capsulePrefab, gameModeCapsuleSpawner.transform.position, Quaternion.identity);
        newGameMode.GetComponent<SettingsCapsule>().SetGameMode((GameMode)gameMode);
    }

    public void SetGameMode(GameObject gameModeCapsule, bool isUnlocked)
    {
        GameMode currentGameMode = gameModeCapsule.GetComponent<SettingsCapsule>().GetGameMode();
        gameModeLabel.text = "Game Mode: " + GameModeDisplay.DisplayGameMode(currentGameMode);
        SwitchGameModePanel(currentGameMode);
    }

    public void SwitchGameModePanel(GameMode newGameMode)
    {
        GameObject newGameModePanel = gameModePanels[(int)newGameMode];

        currentGameModePanel.SetActive(false);
        newGameModePanel.SetActive(true);
        currentGameModePanel = newGameModePanel;
    }

    public void SetPreset(GameObject gameModeCapsule, bool isUnlocked)
    {

    }

    private Room GetRoom() => PhotonNetwork.CurrentRoom;
}
