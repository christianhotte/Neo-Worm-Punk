using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoJoinRoom : MonoBehaviour
{
    [SerializeField, Tooltip("If true, automatically spawn the player in the locker room upon joining the room.")] private bool goToLockerRoom;
    [SerializeField, Tooltip("The name of the room for when joining on Init scene.")] private string roomName = "[DEMO ROOM]";

    void Awake()
    {
        // For debugging purposes. Drag & drop this script to anything in the scene to auto join a room.
        NetworkManagerScript.instance.joinRoomOnLoad = true;
    }

    // Unchecks off joinRoomOnLoad so that when we leave the scene, we won't be stuck in GameDevTest settings.
    private void OnDestroy()
    {
        NetworkManagerScript.instance.joinRoomOnLoad = false;
    }

    /// <summary>
    /// Automatically loads the player into a scene.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load the player into.</param>
    public void AutoLoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public string GetRoomName() => roomName;

    public bool GoToLockerRoom() => goToLockerRoom;
}