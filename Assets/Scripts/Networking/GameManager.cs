using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    internal bool levelTransitionActive = false;
    internal string prevSceneName;

    private void Awake()
    {
        Instance = this;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    /// <summary>
    /// Loads the new scene when starting the game.
    /// </summary>
    /// <param name="sceneIndex"></param>
    public void LoadGame(SceneIndexes sceneIndex)
    {
        Debug.Log("Loading Scene - " + sceneIndex.ToString());
        //SceneManager.LoadScene((int)sceneIndex);
        PhotonNetwork.LoadLevel((int)sceneIndex);
        levelTransitionActive = false;
    }

    public void OnSceneUnloaded(Scene scene)
    {
        prevSceneName = scene.name;
    }

    /// <summary>
    /// Determine whether the player is in a menu depending on the active scene name.
    /// </summary>
    /// <returns>If true, the player is in a menu scene. If false, the player is in a combat scene.</returns>
    public bool InMenu()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "MainMenu":
                return true;
            case "NetworkLockerRoom":
                return true;
            case "JustinMenuScene":
                return true;
            default:
                return false;
        }
    }

    // Gets the name of the last scene David Wu ;)
    public string GetLastSceneName()
    {
        // Retrieving the total number of scenes in build settings
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        // Retrieving root objects of the active scene.
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        // Using Linq to sort scenes based on their build index and filters any scenes without any root game objects in the active scene
        var sortedScenes = Enumerable.Range(0, sceneCount)
            .Select(i => SceneUtility.GetScenePathByBuildIndex(i))
            .Where(path => rootObjects.Any(o => o.scene.path == path))
            .OrderBy(path => SceneManager.GetSceneByPath(path).buildIndex)
            .ToList();      // Convert to list but we are just retreiving the last one.

        // Gets the last scene name from the list.
        return sortedScenes.LastOrDefault();
    }
}