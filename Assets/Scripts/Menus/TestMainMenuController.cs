using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestMainMenuController : MonoBehaviour
{
    public enum MenuArea { LOBBY }

    private PlayerController playerObject;
    [SerializeField, Tooltip("The positions for where the player moves to in the menu areas.")] private Transform[] menuLocations;
    [SerializeField, Tooltip("The doors linked where to the menu areas that the player will move to.")] private DoorController[] doorLocations;

    private void Start()
    {
        playerObject = FindObjectOfType<PlayerController>();
    }

    public void GoToArena()
    {
/*        if (GameManager.Instance != null)
            GameManager.Instance.LoadGame(SceneIndexes.ARENA);
        else
            SceneManager.LoadScene((int)SceneIndexes.ARENA);*/
    }

    public void ToggleDoor(DoorController doorController)
    {
        //If the door is not open, open the door
        if(!doorController.IsDoorOpen())
            doorController.OpenDoor();
        else
            doorController.CloseDoor();
    }

    /// <summary>
    /// Transports the player to the lobby area.
    /// </summary>
    /// <param name="speed">The number of seconds it takes to move from the main area to the lobby area.</param>
    public void TransportToLobby(float speed)
    {
        NetworkManagerScript.instance.JoinLobby();
        StartCoroutine(MovePlayerInMenu(MenuArea.LOBBY, speed));
    }

    /// <summary>
    /// Launch the player into the sky.
    /// </summary>
    /// <param name="doorController">The door that opens to let the player get launched.</param>
    public void LaunchPlayer(DoorController doorController)
    {
        if (NetworkManagerScript.instance.IsLocalPlayerInRoom())
        {
            GameManager.Instance.levelTransitionActive = true;
            StartCoroutine(LaunchPlayerSequence(doorController));
        }
    }

    private IEnumerator LaunchPlayerSequence(DoorController doorController)
    {
        //Open the door and wait for completion
        doorController.OpenDoor();
        yield return new WaitForSeconds(doorController.GetDoorSpeed());

        //Launch the player with an upward force
        playerObject.GetComponentInChildren<Rigidbody>().AddForce(Vector3.up * 10f, ForceMode.Impulse);
        playerObject.GetComponentInChildren<FadeScreen>().FadeOut();

        yield return new WaitForSeconds(playerObject.GetComponentInChildren<FadeScreen>().GetFadeDuration());
        yield return null;

        GameManager.Instance.LoadGame(SceneIndexes.NETWORKLOCKERROOM);
    }

    private IEnumerator MovePlayerInMenu(MenuArea menuArea, float speed)
    {
        //Get the starting position and ending position based on the area the player is moving to
        Vector3 startingPos = playerObject.transform.localPosition;
        Vector3 endingPos = menuLocations[(int)menuArea].position;

        //Get the door that reveals the next area and open it
        DoorController currentDoor = doorLocations[(int)menuArea];
        currentDoor.OpenDoor();

        yield return new WaitForSeconds(currentDoor.GetDoorSpeed());

        //Move the player with a lerp
        float timeElapsed = 0;

        while (timeElapsed < speed)
        {
            //Smooth lerp duration algorithm
            float t = timeElapsed / speed;
            t = t * t * (3f - 2f * t);

            playerObject.transform.localPosition = Vector3.Lerp(startingPos, endingPos, t);    //Lerp the player's movement

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        //Set the player's position and close the door
        playerObject.transform.localPosition = endingPos;
        currentDoor.CloseDoor();
    }
}
