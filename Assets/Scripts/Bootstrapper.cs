using UnityEngine;
using System.IO;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        //Before the scene loads, spawn an Init prefab and make sure it never gets destroyed, even between scenes
        Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("Init")));
        GameSettings.GetTheMilk();

        //If the presets directory path does not exist, create it
        if (!Directory.Exists(GameSettings.presetsDirectoryPath))
            Directory.CreateDirectory(GameSettings.presetsDirectoryPath);
    }
}