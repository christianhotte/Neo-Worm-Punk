using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    public static bool debugMode = true;

    public static bool toggleTest = false;
    public static float testDialValue = 0f;
    public static float testSliderValue = 0f;

    //Default Room Settings
    public static int[] matchLengths = { 60, 120, 180, 300, 420, 600 };
    public static int roomCodeLength = 5;
    public static int defaultMatchLength = matchLengths[0];
    public static int HPDefault = 3;
    public static float[] upgradeFrequencies = { 1f, 0.5f, 0.25f };
    public static float defaultUpgradeFrequency = upgradeFrequencies[1];
    public static float[] upgradeLengths = { 10f, 20f, 30f };
    public static float defaultUpgradeLength = upgradeLengths[1];
    public static bool upgradesActiveDefault = true;
    public static bool hazardsActiveDefault = true;
    public static bool teamModeDefault = false;

    public static string titleScreenScene = "DavidMenuScene";
    public static string roomScene = "NetworkLockerRoom";
    public static string arenaScene = "DM_0.17_Arena";
    public static string tutorialScene = "TutorialFinal";

    public static float defaultMasterSound = 0.5f;
    public static float defaultMusicSound = 0.5f;
    public static float defaultSFXSound = 0.5f;
    public static float defaultVoiceSound = 0.5f;

    public static int UpgradeFrequencyToInt(float upgradeFrequency)
    {
        for(int i = 0; i < upgradeFrequencies.Length; i++)
        {
            if (upgradeFrequency == upgradeFrequencies[i])
                return i;
        }

        return -1;
    }

    public static int UpgradeLengthToInt(float upgradeLength)
    {
        for (int i = 0; i < upgradeLengths.Length; i++)
        {
            if (upgradeLength == upgradeLengths[i])
                return i;
        }

        return -1;
    }
}
