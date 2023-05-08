using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
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
    [SerializeField] private TextMeshProUGUI upgradeFrequencyLabel;
    [SerializeField] private TextMeshProUGUI upgradeLengthLabel;
    [Space(10)]

    [Header("Objects")]
    [SerializeField] private LeverController roomTypeController;
    [SerializeField] private DialRotationController matchDial;
    [SerializeField] private SliderController HPSlider;
    [SerializeField] private PhysicalToggleController teamsModeToggle;
    [SerializeField] private PhysicalToggleController hazardsToggle;
    [SerializeField] private PhysicalToggleController upgradesToggle;
    [SerializeField] private SliderController upgradeFrequencySlider;
    [SerializeField] private SliderController upgradeLengthSlider;
    [Space(10)]

    [Header("Game Mode and Preset Settings")]
    [SerializeField] private TextMeshProUGUI gameModeLabel;
    [SerializeField] private GameObject[] gameModePanels;
    [SerializeField] private LockController gameModeArea;
    [SerializeField] private LockController presetsArea;
    [SerializeField] private TextMeshProUGUI presetsDataText;
    [SerializeField] private GameObject loadPresetButton;
    [SerializeField] private Transform gameModeCapsuleSpawner;
    [SerializeField] private GameObject capsulePrefab;

    private bool isInitialized = false; //Checks to see if the current room settings are initialized on the room settings UI
    private GameMode currentGameMode = GameMode.TimeAttack;
    private GameObject currentGameModePanel;

    private GamePreset currentSettings;
    private GamePreset displayedPreset;

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
            roomTypeController.GetLeverHandle().MoveToAngle(GetRoom().IsVisible ? roomTypeController.GetMinimumAngle() : roomTypeController.GetMaximumAngle());

            //Moves the dial to the index of the match lengths array that the custom room setting is equal to
            matchDial.ResetDial();
            matchDial.MoveDial(Array.IndexOf(GameSettings.matchLengths, (int)GetRoom().CustomProperties["RoundLength"]));

            //Debug.Log("Player HP: " + (int)GetRoom().CustomProperties["PlayerHP"]);

            //Moves the slider to the HP value
            HPSlider.MoveToValue((int)GetRoom().CustomProperties["PlayerHP"]);

            teamsModeToggle.ForceToggle((bool)GetRoom().CustomProperties["TeamMode"]);
            hazardsToggle.ForceToggle((bool)GetRoom().CustomProperties["HazardsActive"]);
            upgradesToggle.ForceToggle((bool)GetRoom().CustomProperties["UpgradesActive"]);

            upgradeFrequencySlider.MoveToValue(GameSettings.UpgradeFrequencyToInt((float)GetRoom().CustomProperties["UpgradeFrequency"]));
            upgradeLengthSlider.MoveToValue(GameSettings.UpgradeLengthToInt((float)GetRoom().CustomProperties["UpgradeLength"]));
            upgradeFrequencySlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);
            upgradeLengthSlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);

            UpdateMatchLengthLabel();
            UpdatePlayerHPLabel();
            UpdateUpgradeFrequencyLabel();
            UpdateUpgradeLengthLabel();

            NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
            currentSettings = new GamePreset();

            ShowPresetData();

            isInitialized = true;
        }
    }

    private void ShowPresetData()
    {
        int fileNumber = 1;
        string fileName = Application.streamingAssetsPath + "/Presets/Preset_" + fileNumber.ToString("00") + ".json";
        if (File.Exists(fileName))
        {
            string fileData = File.ReadAllText(fileName);
            displayedPreset = JsonUtility.FromJson<GamePreset>(fileData);

            presetsDataText.text = "Preset_" + fileNumber.ToString("00") + "\n" + displayedPreset.ToString();
            loadPresetButton.SetActive(true);
        }
        else
        {
            presetsDataText.text = "No Presets Found.";
            loadPresetButton.SetActive(false);
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
        //Debug.Log("Match Length: " + currentMatchLength);
        //Debug.Log("Match Index: " + Mathf.RoundToInt(currentMatchLength));
        //Debug.Log("Setting Match Length To " + GameSettings.matchLengths[Mathf.RoundToInt(currentMatchLength)]);
        currentSettings.roundLength = GameSettings.matchLengths[Mathf.RoundToInt(currentMatchLength)];
        UpdateRoomSetting("RoundLength", currentSettings.roundLength);
        UpdateMatchLengthLabel();
        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
    }

    /// <summary>
    /// Updates the number of hit points a player will have.
    /// </summary>
    /// <param name="playerHealth">The number of HP the player should have.</param>
    public void UpdatePlayerHealth(float playerHealth)
    {
        currentSettings.playerHP = Mathf.RoundToInt(playerHealth);
        UpdateRoomSetting("PlayerHP", Mathf.RoundToInt(playerHealth));
        UpdatePlayerHPLabel();
        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
    }

    /// <summary>
    /// Determines whether teams are active or not.
    /// </summary>
    /// <param name="teamsModeActive">If true, teams are active. If false, teams are not active.</param>
    public void ToggleTeamsMode(bool teamsModeActive)
    {
        currentSettings.teamMode = teamsModeActive;
        UpdateRoomSetting("TeamMode", teamsModeActive);
        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
        NetworkManagerScript.localNetworkPlayer.UpdateExclusiveColors(!(bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"]);
    }

    /// <summary>
    /// Determines whether upgrades are active or not.
    /// </summary>
    /// <param name="hazardsActive">If true, upgrades are active. If false, upgrades are not active.</param>
    public void ToggleHazardsActive(bool hazardsActive)
    {
        currentSettings.hazardsActive = hazardsActive;
        UpdateRoomSetting("HazardsActive", hazardsActive);
        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
    }

    /// <summary>
    /// Determines whether upgrades are active or not.
    /// </summary>
    /// <param name="upgradesActive">If true, upgrades are active. If false, upgrades are not active.</param>
    public void ToggleUpgradesActive(bool upgradesActive)
    {
        currentSettings.upgradesActive = upgradesActive;
        UpdateRoomSetting("UpgradesActive", upgradesActive);

        upgradeFrequencySlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);
        upgradeLengthSlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);

        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
    }

    /// <summary>
    /// Updates the number of hit points a player will have.
    /// </summary>
    /// <param name="upgradeFreq">The frequency of the upgrades spawning.</param>
    public void UpdateUpgradeFrequency(float upgradeFreq)
    {
        currentSettings.upgradeFrequency = GameSettings.upgradeFrequencies[Mathf.RoundToInt(upgradeFreq)];
        UpdateRoomSetting("UpgradeFrequency", currentSettings.upgradeFrequency);
        UpdateUpgradeFrequencyLabel();
        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
    }

    /// <summary>
    /// Updates the general length of an upgrade.
    /// </summary>
    /// <param name="upgradeLen">The length of the upgrade.</param>
    public void UpdateUpgradeLength(float upgradeLen)
    {
        currentSettings.upgradeLength = GameSettings.upgradeLengths[Mathf.RoundToInt(upgradeLen)];
        UpdateRoomSetting("UpgradeLength", currentSettings.upgradeLength);
        UpdateUpgradeLengthLabel();
        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
    }

    /// <summary>
    /// Updates the match length label text.
    /// </summary>
    public void UpdateMatchLengthLabel()
    {
        matchLengthLabel.text = "Match Length: ";

        //Debug.Log("Match Length: " + GetRoom().CustomProperties["RoundLength"].ToString());

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
    /// Updates the label of the Upgrade Frequency text.
    /// </summary>
    public void UpdateUpgradeFrequencyLabel()
    {
        string upgradeFrequency = "Upgrade Frequency: ";

        switch (GameSettings.UpgradeFrequencyToInt((float)GetRoom().CustomProperties["UpgradeFrequency"]))
        {
            case 0:
                upgradeFrequency += "Low";
                break;
            case 1:
                upgradeFrequency += "Medium";
                break;
            case 2:
                upgradeFrequency += "High";
                break;
        }

        upgradeFrequencyLabel.text = upgradeFrequency;
    }

    /// <summary>
    /// Updates the label of the Upgrade Length text.
    /// </summary>
    public void UpdateUpgradeLengthLabel()
    {
        string upgradeLength = "Upgrade Length: ";

        switch (GameSettings.UpgradeLengthToInt((float)GetRoom().CustomProperties["UpgradeLength"]))
        {
            case 0:
                upgradeLength += "Short";
                break;
            case 1:
                upgradeLength += "Medium";
                break;
            case 2:
                upgradeLength += "Long";
                break;
        }

        upgradeLengthLabel.text = upgradeLength;
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

    public void SavePreset()
    {
        string settingsData = JsonUtility.ToJson(currentSettings);

        string[] files = Directory.GetFiles(Application.streamingAssetsPath + "/Presets");

        int fileNumber = 1;
        
        foreach(var file in files)
        {
            string fileEnd = fileNumber.ToString("00");
            if (!file.Contains(fileEnd))
                break;
            else
                fileNumber++;
        }

        Debug.Log("Saving Preset_" + fileNumber.ToString("00") + ".json to " + Application.streamingAssetsPath + "/ Presets");

        File.WriteAllText(Application.streamingAssetsPath + "/Presets/Preset_" + fileNumber.ToString("00") + ".json", settingsData);
        ShowPresetData();
    }

    public void SetPreset(GameObject gameModeCapsule, bool isUnlocked)
    {

    }

    public void SetPreset()
    {
        if(displayedPreset != null)
            LoadPreset(displayedPreset);
    }

    /// <summary>
    /// Sets the room settings based on the preset values given.
    /// </summary>
    /// <param name="newPreset">The preset data.</param>
    private void LoadPreset(GamePreset newPreset)
    {
        UpdateRoomSetting("RoundLength", newPreset.roundLength);
        UpdateMatchLengthLabel();

        UpdateRoomSetting("PlayerHP", newPreset.playerHP);
        UpdatePlayerHPLabel();

        UpdateRoomSetting("HazardsActive", newPreset.hazardsActive);

        UpdateRoomSetting("UpgradesActive", newPreset.upgradesActive);
        upgradeFrequencySlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);
        upgradeLengthSlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);

        UpdateRoomSetting("UpgradeFrequency", newPreset.upgradeFrequency);
        UpdateUpgradeFrequencyLabel();

        UpdateRoomSetting("UpgradeLength", currentSettings.upgradeLength);
        UpdateUpgradeLengthLabel();

        UpdateRoomSetting("TeamMode", newPreset.teamMode);

        //Moves the dial to the index of the match lengths array that the custom room setting is equal to
        matchDial.ResetDial();
        matchDial.MoveDial(Array.IndexOf(GameSettings.matchLengths, (int)GetRoom().CustomProperties["RoundLength"]));

        //Moves the slider to the HP value
        HPSlider.MoveToValue((int)GetRoom().CustomProperties["PlayerHP"]);

        teamsModeToggle.ForceToggle((bool)GetRoom().CustomProperties["TeamMode"]);
        hazardsToggle.ForceToggle((bool)GetRoom().CustomProperties["HazardsActive"]);
        upgradesToggle.ForceToggle((bool)GetRoom().CustomProperties["UpgradesActive"]);

        upgradeFrequencySlider.MoveToValue(GameSettings.UpgradeFrequencyToInt((float)GetRoom().CustomProperties["UpgradeFrequency"]));
        upgradeLengthSlider.MoveToValue(GameSettings.UpgradeLengthToInt((float)GetRoom().CustomProperties["UpgradeLength"]));
        upgradeFrequencySlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);
        upgradeLengthSlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);

        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
        NetworkManagerScript.localNetworkPlayer.UpdateExclusiveColors(!(bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"]);

        currentSettings = newPreset;
    }

    private Room GetRoom() => PhotonNetwork.CurrentRoom;
}

[System.Serializable]
public class GamePreset
{
    public int roundLength;
    public int playerHP;
    public bool hazardsActive;
    public bool upgradesActive;
    public float upgradeFrequency;
    public float upgradeLength;
    public bool teamMode;

    public GamePreset()
    {
        roundLength = GameSettings.defaultMatchLength;
        playerHP = GameSettings.HPDefault;
        hazardsActive = GameSettings.hazardsActiveDefault;
        upgradesActive = GameSettings.upgradesActiveDefault;
        upgradeFrequency = GameSettings.defaultUpgradeFrequency;
        upgradeLength = GameSettings.defaultUpgradeLength;
        teamMode = GameSettings.teamModeDefault;
    }

    public GamePreset(int roundLength, int playerHP, bool hazardsActive, bool upgradesActive, float upgradeFrequency, float upgradeLength, bool teamMode)
    {
        this.roundLength = roundLength;
        this.playerHP = playerHP;
        this.hazardsActive = hazardsActive;
        this.upgradesActive = upgradesActive;
        this.upgradeFrequency = upgradeFrequency;
        this.upgradeLength = upgradeLength;
        this.teamMode = teamMode;
    }

    public override string ToString()
    {
        string matchLength = "Match Length: ";
        int currentRoundLength = roundLength;
        if (currentRoundLength < 60)
            matchLength += currentRoundLength.ToString() + " second" + (currentRoundLength > 1 ? "s" : "");
        else
            matchLength += (currentRoundLength / 60).ToString() + " minute" + (currentRoundLength / 60 > 1 ? "s" : "");

        string playerHP = "Player HP: " + this.playerHP.ToString();
        string teamsMode = "Teams Mode: " + (teamMode ? "On" : "Off");
        string hazardsActive = "Hazards Active: " + (this.hazardsActive ? "On" : "Off");
        string upgradesActive = "Upgrades Active: " + (this.upgradesActive ? "On" : "Off");
        string upgradeFrequency = "Upgrade Frequency: ";

        switch (GameSettings.UpgradeFrequencyToInt(this.upgradeFrequency))
        {
            case 0:
                upgradeFrequency += "Low";
                break;
            case 1:
                upgradeFrequency += "Medium";
                break;
            case 2:
                upgradeFrequency += "High";
                break;
        }

        string upgradeLength = "Upgrade Length: ";

        switch (GameSettings.UpgradeLengthToInt(this.upgradeLength))
        {
            case 0:
                upgradeLength += "Short";
                break;
            case 1:
                upgradeLength += "Medium";
                break;
            case 2:
                upgradeLength += "Long";
                break;
        }

        return matchLength + "\n" + playerHP + "\n" + teamsMode + "\n" + hazardsActive + "\n" + upgradesActive + (this.upgradesActive ? ("\n" + upgradeFrequency).ToString() : "") + (this.upgradesActive ? ("\n" + upgradeLength).ToString() : "");
    }
}