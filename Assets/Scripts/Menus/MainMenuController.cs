using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MainMenuController : MonoBehaviour
{
    public enum MenuArea { SETTINGS, FINAL, TUBE }

    private PlayerController playerObject;
    [SerializeField, Tooltip("The positions for where the player moves to in the menu areas.")] private Transform[] menuLocations;
    [SerializeField, Tooltip("The location of the lobby.")] private Transform lobbyLocation;
    [SerializeField, Tooltip("The animator for 1st Panel.")] private Animator Panel1Animator;
    [SerializeField, Tooltip("The animator for 1st Panel.")] private Animator Panel2Animator;
    [SerializeField, Tooltip("The animator for 1st Panel.")] private Animator Panel3Animator;

    private void Start()
    {
        /// Move the player forward on the conveyor once the game starts
        playerObject = FindObjectOfType<PlayerController>();
        Invoke("TransportToSettings", 3);
    }

    public void GoToArena()
    {
        /*        if (GameManager.Instance != null)
                    GameManager.Instance.LoadGame(SceneIndexes.ARENA);
                else
                    SceneManager.LoadScene((int)SceneIndexes.ARENA);*/
    }

    /// <summary>
    /// Transports the player to the settings area.
    /// </summary>
    private void TransportToSettings()
    {
        StartCoroutine(MovePlayerInMenu(MenuArea.SETTINGS, 10));
    }

    /// <summary>
    /// Transports the player to the final area.
    /// </summary>
    /// <param name="speed">The number of seconds it takes to move from the main area to the final area.</param>
    public void TransportToFinal(float speed)
    {
        //NetworkManagerScript.instance.JoinLobby();
        StartCoroutine(MovePlayerInMenu(MenuArea.FINAL, speed));
        Panel1Animator.Play("Panel_1_Rev");
        Panel2Animator.Play("Panel_2_Rev");
        Panel3Animator.Play("Panel_3_Rev");
    }

    /// <summary>
    /// Transports the player to the tube.
    /// </summary>
    /// <param name="speed">The number of seconds it takes to move from the final area to the tube.</param>
    public void TransportToTube(float speed)
    {
        StartCoroutine(MovePlayerInMenu(MenuArea.TUBE, speed));
    }

    private IEnumerator MovePlayerInMenu(MenuArea menuArea, float speed)
    {
        //Get the starting position and ending position based on the area the player is moving to
        Vector3 startingPos = playerObject.transform.localPosition;
        Vector3 endingPos = menuLocations[(int)menuArea].position;

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
    }

    public void GoToLobby()
    {
        StartCoroutine(TeleportPlayerToLobby());
    }

    private IEnumerator TeleportPlayerToLobby()
    {
        NetworkManagerScript.instance.JoinLobby();
        FadeScreen playerScreenFader = PlayerController.instance.GetComponentInChildren<FadeScreen>();
        playerScreenFader.FadeOut();
        yield return new WaitForSeconds(playerScreenFader.GetFadeDuration());
        PlayerController.instance.transform.position = lobbyLocation.position;
        yield return new WaitForSeconds(0.5f);
        playerScreenFader.FadeIn();
    }

    public void FadeToLockerRoom()
    {
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        playerObject.GetComponentInChildren<FadeScreen>().FadeOut();

        yield return new WaitForSeconds(playerObject.GetComponentInChildren<FadeScreen>().GetFadeDuration());
        yield return null;

        GameManager.Instance.LoadGame(SceneIndexes.NETWORKLOCKERROOM);
    }
}
