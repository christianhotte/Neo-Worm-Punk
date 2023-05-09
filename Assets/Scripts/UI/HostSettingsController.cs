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
    [Header("Host Settings Labels")]
    [SerializeField, Tooltip("The label displaying the match length.")] private TextMeshProUGUI matchLengthLabel;
    [SerializeField, Tooltip("The label displaying the player HP.")] private TextMeshProUGUI playerHPLabel;
    [SerializeField, Tooltip("The label displaying the upgrade spawner frequency.")] private TextMeshProUGUI upgradeFrequencyLabel;
    [SerializeField, Tooltip("The label displaying the upgrade duration.")] private TextMeshProUGUI upgradeLengthLabel;
    [Space(10)]

    [Header("Host Settings Objects")]
    [SerializeField, Tooltip("The lever that controls the room type (public or private).")] private LeverController roomTypeController;
    [SerializeField, Tooltip("The dial that controls the match length.")] private DialRotationController matchDial;
    [SerializeField, Tooltip("The slider that controls the player HP.")] private SliderController HPSlider;
    [SerializeField, Tooltip("The toggle that controls whether teams mode is active or not.")] private PhysicalToggleController teamsModeToggle;
    [SerializeField, Tooltip("The toggle that controls whether hazards are active or not.")] private PhysicalToggleController hazardsToggle;
    [SerializeField, Tooltip("The toggle that controls whether upgrades are active or not.")] private PhysicalToggleController upgradesToggle;
    [SerializeField, Tooltip("The slider that controls the upgrade spawner frequency.")] private SliderController upgradeFrequencySlider;
    [SerializeField, Tooltip("The slider that controls the upgrade duration.")] private SliderController upgradeLengthSlider;
    [Space(10)]

    [Header("Game Mode Settings")]
    [SerializeField, Tooltip("The label that displays the active game mode.")] private TextMeshProUGUI gameModeLabel;
    [SerializeField, Tooltip("The game mode panels with settings specific to each game mode.")] private GameObject[] gameModePanels;
    [SerializeField, Tooltip("The area where the capsule is placed to set the game mode.")] private LockController gameModeArea;
    [SerializeField, Tooltip("The capsule that sets the game mode.")] private GameObject capsulePrefab;
    [SerializeField, Tooltip("The transform where the game mode capsules are spawned from.")] private Transform gameModeCapsuleSpawner;
    [Space(10)]

    [Header("Preset Settings")]
    [SerializeField, Tooltip("The area where the capsule is placed to set the game preset.")] private LockController presetsArea;
    [SerializeField, Tooltip("The text that displays the preset file data in a readable manner.")] private TextMeshProUGUI presetsDataText;
    [SerializeField, Tooltip("The button that spawns a preset capsule.")] private GameObject loadPresetButton;
    [SerializeField, Tooltip("The button that deletes the displayed preset file.")] private GameObject deletePresetButton;
    [SerializeField, Tooltip("The button that decrements through the list of preset files.")] private GameObject prevPresetButton;
    [SerializeField, Tooltip("The button that increments through the list of preset files.")] private GameObject nextPresetButton;
    [SerializeField, Tooltip("The transform where the game preset capsules are spawned from.")] private Transform presetCapsuleSpawner;
    [SerializeField, Tooltip("The capsule that holds the game preset information.")] private PresetSettings presetCapsulePrefab;
    [SerializeField, Tooltip("The log text for the preset data.")] private TextMeshProUGUI presetLogText;
    [SerializeField, Tooltip("The normal color the preset log text.")] private Color presetLogColor;
    [SerializeField, Tooltip("The color of the preset log text when displaying an error.")] private Color presetErrorColor;

    private bool isInitialized = false;                         //Checks to see if the current room settings are initialized on the room settings UI
    private GameMode currentGameMode = GameMode.TimeAttack;     //The current game mode active
    private GameObject currentGameModePanel;                    //The current game mode panel active

    private GamePreset currentSettings;                         //The current settings on the host settings panel
    private GamePreset displayedPreset;                         //The current settings of the preset being displayed
    private string currentPresetName;                           //The name of the game settings preset file
    private int currentFileNumber;                              //The current index of the preset file being displayed

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

            //Moves the host settings objects to the appropriate values
            HPSlider.MoveToValue((int)GetRoom().CustomProperties["PlayerHP"]);
            teamsModeToggle.ForceToggle((bool)GetRoom().CustomProperties["TeamMode"]);
            hazardsToggle.ForceToggle((bool)GetRoom().CustomProperties["HazardsActive"]);
            upgradesToggle.ForceToggle((bool)GetRoom().CustomProperties["UpgradesActive"]);

            upgradeFrequencySlider.MoveToValue(GameSettings.UpgradeFrequencyToInt((float)GetRoom().CustomProperties["UpgradeFrequency"]));
            upgradeLengthSlider.MoveToValue(GameSettings.UpgradeLengthToInt((float)GetRoom().CustomProperties["UpgradeLength"]));
            upgradeFrequencySlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);
            upgradeLengthSlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);

            //Sets the labels to display the current information
            UpdateMatchLengthLabel();
            UpdatePlayerHPLabel();
            UpdateUpgradeFrequencyLabel();
            UpdateUpgradeLengthLabel();

            //Updates the room settings both locally and on the network
            NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
            currentSettings = new GamePreset();

            ShowPresetData();   //Shows any preset data that the player may have

            //Finish initialization
            isInitialized = true;
        }
    }

    /// <summary>
    /// Shows the current file's preset data.
    /// </summary>
    private void ShowPresetData()
    {
        string [] files = GetPresetFiles();

        //If there are any files, show the file indicated by the current file number index
        if (files.Length > 0)
        {
            currentFileNumber = Math.Clamp(currentFileNumber, 0, files.Length - 1); //Ensure that the file number is within the bounds of the array
            string fileName = files[currentFileNumber];

            //Display the arrow buttons depending on the position of the file number
            prevPresetButton.SetActive(currentFileNumber > 0);
            nextPresetButton.SetActive(currentFileNumber < files.Length - 1);

            //Get the data from the preset file and read it into the displayed preset object
            string fileData = File.ReadAllText(fileName);
            displayedPreset = JsonUtility.FromJson<GamePreset>(fileData);

            //Get the name of the file
            currentPresetName = Path.GetFileNameWithoutExtension(fileName);

            //Display the name of the file and the information within the file
            presetsDataText.text = "<size=3>" + currentPresetName + "</size>\n\n" + displayedPreset.ToString();
            loadPresetButton.SetActive(true);
            deletePresetButton.SetActive(true);
        }
        //If there are no files, hide any buttons and show some text
        else
        {
            currentFileNumber = 0;

            presetsDataText.text = "<size=6>No Presets Found.</size>";
            loadPresetButton.SetActive(false);
            deletePresetButton.SetActive(false);
            prevPresetButton.SetActive(false);
            nextPresetButton.SetActive(false);
        }
    }

    /// <summary>
    /// Increments the preset data and shows the appropriate file.
    /// </summary>
    /// <param name="increment">The number and direction to increment the data in.</param>
    public void UpdatePresetData(int increment)
    {
        currentFileNumber += increment;
        WriteToPresetLog("");
        ShowPresetData();
    }

    /// <summary>
    /// Gets the list of preset files from the StreamingAssets Preset folder.
    /// </summary>
    /// <returns>An array of file directories to all of the player's saved presets.</returns>
    public string[] GetPresetFiles()
    {
        //Get every file within the Presets folder
        string[] files = Directory.GetFiles(GameSettings.presetsDirectoryPath);

        List<string> validFiles = new List<string>();

        //Only include files that have the .json extension and exclude any .meta files
        foreach(var file in files)
        {
            if (file.Contains(".json") && !file.Contains(".meta"))
                validFiles.Add(file);
        }

        return validFiles.ToArray();
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

    /// <summary>
    /// Spawns a game mode capsule and stores the desired information onto it.
    /// </summary>
    /// <param name="gameMode">The index of the game mode information to store onto the capsule.</param>
    public void SpawnGameModeCapsule(int gameMode)
    {
        //Destroys any other existing game capsule
        foreach (var capsule in FindObjectsOfType<SettingsCapsule>())
            Destroy(capsule.gameObject);

        //Spawn the capsule and give it information
        GameObject newGameMode = Instantiate(capsulePrefab, gameModeCapsuleSpawner.position, Quaternion.identity);
        newGameMode.GetComponent<SettingsCapsule>().SetGameMode((GameMode)gameMode);
    }

    /// <summary>
    /// Sets the game mode of the room.
    /// </summary>
    /// <param name="gameModeCapsule">The capsule given to the game mode locker.</param>
    /// <param name="isUnlocked"></param>
    public void SetGameMode(GameObject gameModeCapsule, bool isUnlocked)
    {
        GameMode currentGameMode = gameModeCapsule.GetComponent<SettingsCapsule>().GetGameMode();
        gameModeLabel.text = "Game Mode: " + GameModeDisplay.DisplayGameMode(currentGameMode);
        SwitchGameModePanel(currentGameMode);
    }

    /// <summary>
    /// Switches the game mode panels based on the current game mode.
    /// </summary>
    /// <param name="newGameMode">The current game mode.</param>
    public void SwitchGameModePanel(GameMode newGameMode)
    {
        GameObject newGameModePanel = gameModePanels[(int)newGameMode];

        currentGameModePanel.SetActive(false);
        newGameModePanel.SetActive(true);
        currentGameModePanel = newGameModePanel;
    }

    /// <summary>
    /// Spawns a preset capsule and saves the data of the currently displayed preset.
    /// </summary>
    public void SpawnPresetCapsule()
    {
        //Destroys any other existing preset capsule
        foreach (var capsule in FindObjectsOfType<PresetSettings>())
            Destroy(capsule.gameObject);

        //Spawn the capsule and give it information
        PresetSettings newPreset = Instantiate(presetCapsulePrefab, presetCapsuleSpawner.position, Quaternion.identity);
        newPreset.SetPresetData(displayedPreset);
        newPreset.UpdatePresetLabelText(currentPresetName);
    }

    /// <summary>
    /// Sets the room settings based on the information that the current preset capsule has.
    /// </summary>
    /// <param name="presetCapsule">The capsule given to the preset locker.</param>
    /// <param name="isUnlocked"></param>
    public void SetPreset(GameObject presetCapsule, bool isUnlocked)
    {
        LoadPreset(presetCapsule.GetComponent<PresetSettings>().GetPresetData());
    }

    /// <summary>
    /// Saves the current room settings to a preset file.
    /// </summary>
    public void SavePreset()
    {
        string settingsData = JsonUtility.ToJson(currentSettings);  //Converts the settings into the format needed for a json file.

        string[] files = GetPresetFiles();                          //Gets a list of current preset files

        //If the amount of files that the player has stored has reached the maximum amount allowed, throw an error and return.
        if(files.Length >= GameSettings.maxPresetFiles)
        {
            WriteToPresetLog("Error: Maximum Preset Files Reached. Delete An Existing Preset Before Saving.", true);
            return;
        }

        int fileNumber = 1;
        
        //Find smallest available number to give to the preset file name.
        foreach(var file in files)
        {
            string fileEnd = fileNumber.ToString("00");
            if (!file.Contains(fileEnd))
                break;
            else
                fileNumber++;
        }

        //Saving the preset data to a file and storing it on a folder
        Debug.Log("Saving Preset_" + fileNumber.ToString("00") + ".json to " + GameSettings.presetsDirectoryPath);
        File.WriteAllText(GameSettings.presetsDirectoryPath + "/Preset_" + fileNumber.ToString("00") + ".json", settingsData);
        WriteToPresetLog("Preset_" + fileNumber.ToString("00") + " Successfully Created.");

        ShowPresetData();
    }

    /// <summary>
    /// Deletes the current displayed preset file data.
    /// </summary>
    public void DeletePreset()
    {
        string[] files = GetPresetFiles();  //Gets a list of files

        //If the file that the player is trying to delete exists, delete it and display on the log.
        if (File.Exists(files[currentFileNumber]))
        {
            File.Delete(files[currentFileNumber]);
            WriteToPresetLog(Path.GetFileNameWithoutExtension(files[currentFileNumber]) + " Successfully Deleted.");
        }

        //Update the preset display
        ShowPresetData();
    }

    /// <summary>
    /// Sets the room settings based on the preset values given.
    /// </summary>
    /// <param name="newPreset">The preset data.</param>
    private void LoadPreset(GamePreset newPreset)
    {
        //Updates the room settings data to equal the preset data given to it
        UpdateRoomSetting("RoundLength", newPreset.roundLength);
        UpdateRoomSetting("PlayerHP", newPreset.playerHP);
        UpdateRoomSetting("TeamMode", newPreset.teamMode);
        UpdateRoomSetting("HazardsActive", newPreset.hazardsActive);
        UpdateRoomSetting("UpgradesActive", newPreset.upgradesActive);
        upgradeFrequencySlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);
        upgradeLengthSlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);

        UpdateRoomSetting("UpgradeFrequency", newPreset.upgradeFrequency);
        UpdateRoomSetting("UpgradeLength", currentSettings.upgradeLength);

        //Updates the labels on the host settings
        UpdateMatchLengthLabel();
        UpdatePlayerHPLabel();
        UpdateUpgradeFrequencyLabel();
        UpdateUpgradeLengthLabel();

        //Moves the host settings to display the new room settings
        matchDial.ResetDial();
        matchDial.MoveDial(Array.IndexOf(GameSettings.matchLengths, (int)GetRoom().CustomProperties["RoundLength"]));
        HPSlider.MoveToValue((int)GetRoom().CustomProperties["PlayerHP"]);
        teamsModeToggle.ForceToggle((bool)GetRoom().CustomProperties["TeamMode"]);
        hazardsToggle.ForceToggle((bool)GetRoom().CustomProperties["HazardsActive"]);
        upgradesToggle.ForceToggle((bool)GetRoom().CustomProperties["UpgradesActive"]);
        upgradeFrequencySlider.MoveToValue(GameSettings.UpgradeFrequencyToInt((float)GetRoom().CustomProperties["UpgradeFrequency"]));
        upgradeLengthSlider.MoveToValue(GameSettings.UpgradeLengthToInt((float)GetRoom().CustomProperties["UpgradeLength"]));
        upgradeFrequencySlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);
        upgradeLengthSlider.gameObject.SetActive((bool)GetRoom().CustomProperties["UpgradesActive"]);

        //Updates the room settings display on the network
        NetworkManagerScript.localNetworkPlayer.UpdateRoomSettingsDisplay();
        //Checks to see if the players need exclusive colors or not
        NetworkManagerScript.localNetworkPlayer.UpdateExclusiveColors(!(bool)PhotonNetwork.CurrentRoom.CustomProperties["TeamMode"]);

        //Sets the current settings to the new preset
        currentSettings = newPreset;
    }

    /// <summary>
    /// Writes a message for the preset log.
    /// </summary>
    /// <param name="message">The message for the preset log.</param>
    /// <param name="isError">If true, the message should be displayed as an error.</param>
    private void WriteToPresetLog(string message, bool isError = false)
    {
        //Change the log color based on whether it's marked as an error or not
        if (isError)
            presetLogText.color = presetErrorColor;
        else
            presetLogText.color = presetLogColor;

        presetLogText.text = message;
    }

    /// <summary>
    /// A function that gets the player's current room for convenience.
    /// </summary>
    /// <returns>The current room that the player is connected to.</returns>
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