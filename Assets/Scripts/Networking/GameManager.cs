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
    public List<Transform> spawnPoints = new List<Transform>();
    //public List<SpawnPointManager> spawnPointsManager = new List<SpawnPointManager>();
    public List<LockerTubeController> tubes = new List<LockerTubeController>();

    internal bool levelTransitionActive = false;
    internal string prevSceneName;

    private void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDestroy()
    {
        Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    /// <summary>
    /// Loads the new scene when starting the game.
    /// </summary>
    /// <param name="sceneName">The name of the scene.</param>
    public void LoadGame(string sceneName)
    {
        Debug.Log("Loading Scene - " + sceneName);

        PhotonNetwork.LoadLevel(sceneName);
        levelTransitionActive = false;
    }

    public void OnSceneUnloaded(Scene scene)
    {
        prevSceneName = scene.name;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "NetworkLockerRoom")
        {
            // find all spawn points in the scene
            GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
            foreach (GameObject spawnPointObject in spawnPointObjects)
            {
                // add the transform of each spawn point to the list
                spawnPoints.Add(spawnPointObject.transform);
            }
        }
    }

    /// <summary>
    /// Determine whether the player is in a menu depending on the active scene name.
    /// </summary>
    /// <returns>If true, the player is in a menu scene. If false, the player is in a combat scene.</returns>
    public bool InMenu()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Init":
                return true;
            case "StartScene":
                return true;
            case "MainMenu":
                return true;
            case "NetworkLockerRoom":
                return true;
            case "JustinMenuScene":
                return true;
            case "DavidMenuScene":
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