using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MainMenuController : MonoBehaviour
{
    public enum MenuArea { SETTINGS, FINAL, TUBE }

    [SerializeField, Tooltip("The platform that moves on the conveyor belt.")] private Transform platform;
    private Transform playerObject;
    [SerializeField, Tooltip("The positions for where the player moves to in the menu areas.")] private Transform[] menuLocations;
    [SerializeField, Tooltip("The location of the lobby.")] private Transform lobbyLocation;
    [SerializeField, Tooltip("The animator for 1st Panel.")] private Animator Panel1Animator;
    [SerializeField, Tooltip("The animator for 1st Panel.")] private Animator Panel2Animator;
    [SerializeField, Tooltip("The animator for 1st Panel.")] private Animator Panel3Animator;

    [SerializeField, Tooltip("Main Menu Background Music")] private AudioClip mainMenuMusic;
    [SerializeField, Tooltip("Wormpunk Sound")] private AudioClip wormPunkSound;

    [SerializeField, Tooltip("The main menu music audio source.")] private AudioSource menuAudioSource;

    private void Start()
    {
        // Play menu music
        menuAudioSource.clip = mainMenuMusic;
        menuAudioSource.Play();
        menuAudioSource.volume = PlayerPrefs.GetFloat("MusicVolume", GameSettings.defaultMusicSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound);

        // Move the player forward on the conveyor once the game starts
        playerObject = PlayerController.instance.xrOrigin.transform;
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
        Panel1Animator.SetBool("Activated", false);
        Panel2Animator.SetBool("Activated", false);
        Panel3Animator.SetBool("Activated", false);
        StartCoroutine(MovePlayerInMenu(MenuArea.FINAL, speed));
        Invoke("WaitOnCloseDoor", speed);
    }

    /// <summary>
    /// Transports the player to the tube.
    /// </summary>
    /// <param name="speed">The number of seconds it takes to move from the final area to the tube.</param>
    public void TransportToTube(float speed)
    {
        StartCoroutine(MovePlayerInMenu(MenuArea.TUBE, speed));
    }

    private void WaitOnCloseDoor()
    {
        FindObjectOfType<DoorTrigger>().CloseDoor();
    }

    private IEnumerator MovePlayerInMenu(MenuArea menuArea, float speed)
    {
        //Get the starting position and ending position based on the area the platform and player are moving to
        Vector3 startingPlatformPos = platform.position;
        Vector3 endingPlatformPos = platform.position;
        endingPlatformPos.z = menuLocations[(int)menuArea].position.z;

        Vector3 startingPlayerPos = playerObject.position;
        Vector3 endingPlayerPos = playerObject.position;
        endingPlayerPos.z = menuLocations[(int)menuArea].position.z;

        //Move the player with a lerp
        float timeElapsed = 0;

        while (timeElapsed < speed)
        {
            //Smooth lerp duration algorithm
            float t = timeElapsed / speed;
            t = t * t * (3f - 2f * t);

            platform.position = Vector3.Lerp(startingPlatformPos, endingPlatformPos, t);    //Lerp the platform's movement
            playerObject.position = Vector3.Lerp(startingPlayerPos, endingPlayerPos, t);    //Lerp the player's movement

            timeElapsed += Time.deltaTime;

            yield return null;
        }
    }

    public void PlayWormpunkSound()
    {
        GetComponent<AudioSource>().PlayOneShot(wormPunkSound, PlayerPrefs.GetFloat("SFXVolume", GameSettings.defaultSFXSound) * PlayerPrefs.GetFloat("MasterVolume", GameSettings.defaultMasterSound));
    }

    public void GoToLobby()
    {
        StartCoroutine(TeleportPlayerToLobby());
    }

    private IEnumerator TeleportPlayerToLobby()
    {
        yield return new WaitForSeconds(2.0f);
        NetworkManagerScript.instance.JoinLobby();
        FadeScreen playerScreenFader = PlayerController.instance.GetComponentInChildren<FadeScreen>();
        playerScreenFader.FadeOut();
        yield return new WaitForSeconds(playerScreenFader.GetFadeDuration());

        playerObject.position = lobbyLocation.position;
        yield return new WaitForSeconds(0.5f);
        playerScreenFader.FadeIn();
        StopAllCoroutines();
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

        menuAudioSource.Stop();
        GameManager.Instance.LoadGame(GameSettings.roomScene);
    }
}
