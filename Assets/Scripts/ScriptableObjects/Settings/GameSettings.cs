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
    public static int[] matchLengths = { 30, 60, 120, 300, 420 };
    public static int roomCodeLength = 5;
    public static int testMatchLength = 300;
    public static int HPDefault = 3;

    public static string titleScreenScene = "JustinMenuScene";
    public static string roomScene = "NetworkLockerRoom";
    public static string arenaScene = "DM_0.16_Arena";
    public static string tutorialScene = "Tutorial";

    public static float defaultMasterSound = 0.5f;
    public static float defaultMusicSound = 0.5f;
    public static float defaultSFXSound = 0.5f;
    public static float defaultVoiceSound = 0.5f;
}
