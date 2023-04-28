using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LockerTubeController : MonoBehaviour
{
    private LockerTubeSpawner spawnManager;

    [SerializeField, Tooltip("The parent that holds all of the ready lights.")] private Transform readyLights;
    [SerializeField, Tooltip("The spawn point for the player's name.")] private Transform playerNameSpawnPoint;
    [SerializeField, Tooltip("The prefab that displays the player's name.")] private GameObject playerNamePrefab;
    [SerializeField, Tooltip("The GameObject for the host settings.")] private GameObject hostSettings;
    [SerializeField, Tooltip("The GameObject for the teams display.")] private GameObject teamsDisplay;

    //internal int tubeNumber;
    //public bool occupied = false;
    /// <summary>
    /// ID of player which is currently in this tube.
    /// </summary>
    internal int currentPlayerID;
    internal Transform spawnPoint;
    private bool isMyLocalTube;

    private Transform myPlayerObject;

    private Vector3[] playerCheckpoints = new Vector3[4];
    private Vector3 originalTubePosition;

    [SerializeField] public Vector3 spawnPointBias;
    [SerializeField] private AnimationCurve tubeRaiseCurve;

    private void Awake()
    {
        spawnManager = FindObjectOfType<LockerTubeSpawner>();
        spawnPoint = transform.Find("Spawnpoint");
        playerCheckpoints[1] = transform.position + spawnPointBias;
        playerCheckpoints[2] = transform.position + (transform.forward * 4) + spawnPointBias;
        playerCheckpoints[3] = transform.position + (transform.forward * 4) + spawnPointBias + new Vector3(0, 10, 0);
        originalTubePosition = transform.position;
        transform.position -= new Vector3(0, 10, 0);
        playerCheckpoints[0] = transform.position + spawnPointBias;
    }

    public void GiveTubeAPlayer(Transform playerTransform, bool myLocalTubeOrNot)
    {
        myPlayerObject = playerTransform;
        isMyLocalTube = myLocalTubeOrNot;
    }

    private void Update()
    {
        //if I have a player to track and this is not the local players' tube
        if (myPlayerObject != null && !isMyLocalTube) { transform.position = Vector3.Lerp(transform.position, myPlayerObject.position - spawnPointBias, Time.deltaTime * 8); }
    }

    public void PlayerToLobbyPosition()
    {
        StartCoroutine(MovePlayerToNextPosition(myPlayerObject, playerCheckpoints[1], 8));
    }

    public void TubeToBeginningPosition()
    {
        //StartCoroutine(MovePlayerToNextPosition(transform, originalTubePosition, 8));
    }

    public void PlayerToReadyPosition(float duration)
    {
        if(isMyLocalTube) { Debug.Log("I should personally be going to my ready position"); }
        StartCoroutine(MovePlayerToNextPosition(myPlayerObject, playerCheckpoints[2], duration));
    }

    public void PlayerToExitPosition(float duration)
    {
        if (isMyLocalTube) { Debug.Log("I should personally be going to my exit position"); }
        StartCoroutine(MovePlayerToNextPosition(myPlayerObject, playerCheckpoints[3], duration));
    }

    IEnumerator MovePlayerToNextPosition(Transform moveMe, Vector3 endPos, float moveTime)
    {
        //set initial time to initial time
        float timeElapsed = 0;
        //StartCoroutine(MoveTubeToNextPosition(endPos, moveTime));

        Vector3 startPos = moveMe.position;

        while(timeElapsed < moveTime)
        {
            //just in case, stay safe ;)
            if (GameManager.Instance.levelTransitionActive) { break; }

            //smooth lerp duration alg
            float t = timeElapsed / moveTime;
            t = tubeRaiseCurve.Evaluate(t);

            moveMe.position = Vector3.Lerp(startPos, endPos, t);
            if(isMyLocalTube) { transform.position = Vector3.Lerp(startPos - spawnPointBias, endPos - spawnPointBias, t); }

            //advance time
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        moveMe.position = endPos;
        if(isMyLocalTube) { transform.position = endPos - spawnPointBias; }
    }
    /*
    public IEnumerator MoveTubeToNextPosition(Vector3 endPos, float moveTime)
    {
        //set initial time to initial time
        float timeElapsed = 0;

        Vector3 startPos = transform.position;

        while (timeElapsed < moveTime)
        {
            //just in case, stay safe ;)
            if (GameManager.Instance.levelTransitionActive) { break; }

            //smooth lerp duration alg
            float t = timeElapsed / moveTime;
            t = tubeRaiseCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPos - spawnPointBias, endPos - spawnPointBias, t);

            //advance time
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        transform.position = endPos - spawnPointBias;
    }
    */

    /*
    public void StartTube(Transform playerObject)
    {
        myPlayerObject = playerObject;
        StartCoroutine(MoveTubeAndPlayer(tubeCheckpoints[1], playerCheckpoints[1], Vector3.zero, startTubeTotalTime, false));
    }

    public void StartOtherPlayersTube(Vector3 networkPlayerPos)
    {
        StartCoroutine(MoveTubeAndPlayer(tubeCheckpoints[1], playerCheckpoints[1], networkPlayerPos, startTubeTotalTime, true));
    }

    public void TubesToCenter(float duration)
    {
        StartCoroutine(MoveTubeAndPlayer(tubeCheckpoints[2], playerCheckpoints[2], Vector3.zero, duration, false));
    }

    public void TubesUp(float duration)
    {
        StartCoroutine(MoveTubeAndPlayer(tubeCheckpoints[3], playerCheckpoints[3], Vector3.zero, duration, false));
    }

    public void TubeExit(float duration)
    {
        StartCoroutine(MoveTubeAndPlayer(tubeCheckpoints[0], playerCheckpoints[0], Vector3.zero, duration, false));
    }

    /// <summary>
    /// Moves the tube and player to the next location they should go to
    /// </summary>
    /// <param name="transformChange"></param>
    /// <param name="moveTime"></param>
    /// <returns></returns>
    IEnumerator MoveTubeAndPlayer(Vector3 endPos, Vector3 playerEndPos, Vector3 networkPlayerPos, float moveTime, bool isOther)
    {
        //set initial time to initial time
        timeElapsed = 0;
                                                                                                                                                //just base this off of the players' movement

        Vector3 startPos = transform.localPosition;
        Vector3 playerStartPos = Vector3.zero;

        if (isOther)
        {
            //what percent are you through the movement?
            float fraction = Mathf.InverseLerp(tubeCheckpoints[0].y, tubeCheckpoints[1].y, networkPlayerPos.y - spawnPointBias.y);
            //total time = 4
            //current time = ?
            //fractional time = fraction
            //fraction time = current time / total time
            //fraction time * total time = current time to start at
            Debug.Log("MY FRACTION = " + fraction);

            timeElapsed = fraction * startTubeTotalTime;
        }
        Debug.Log("NETWORKPLAYER= " + networkPlayerPos);

        if (myPlayerObject != null)
        {
            playerStartPos = myPlayerObject.position;
        }

        //lerp the objects from start to end positions
        while (timeElapsed < moveTime)
        {
            //just in case, stay safe :)
            if (GameManager.Instance.levelTransitionActive) { break; }

            //smooth lerp duration alg
            float t = timeElapsed / moveTime;
            t = tubeRaiseCurve.Evaluate(t);

            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            if(myPlayerObject != null)
            {
                myPlayerObject.position = Vector3.Lerp(playerStartPos, playerEndPos, t);            //HOW ABOUT WE ONLY MODIFY ALONG THAT AXIS??? no jitter, then
            }
            
            //advance time
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        
        transform.localPosition = endPos;
        if (myPlayerObject != null) { myPlayerObject.position = playerEndPos; }
    }
    */



    /// <summary>
    /// Updates the lights depending on whether the player is ready or not.
    /// </summary>
    /// <param name="isActivated">If true, the player is ready. If false, the player is not ready.</param>
    public void UpdateLights(bool isActivated)
    {
        foreach (var light in readyLights.GetComponentsInChildren<ReadyLightController>())
            light.ActivateLight(isActivated);
    }

    /// <summary>
    /// Spawns the player name in the tube.
    /// </summary>
    /// <param name="playerName">The name of the player.</param>
    public void SpawnPlayerName(string playerName)
    {
        //If there is not a name in the tube, add a name
        if(playerNameSpawnPoint.childCount == 0)
        {
            GameObject playerNameObject = Instantiate(playerNamePrefab, playerNameSpawnPoint);
            playerNameObject.transform.localPosition = Vector3.zero;

            playerNameObject.GetComponentInChildren<TextMeshProUGUI>().text = playerName;
        }
    }

    public void ShowTeamsDisplay(bool showTeams) => teamsDisplay.SetActive(showTeams);

    /// <summary>
    /// Shows the host settings in the tube.
    /// </summary>
    /// <param name="showSettings">If true, the host settings are shown. If false, they are hidden.</param>
    public void ShowHostSettings(bool showSettings)
    {
        hostSettings.SetActive(showSettings);
    }

    public TeamsDisplay GetTeamsDisplay() => teamsDisplay.GetComponent<TeamsDisplay>();

    public int GetTubeNumber()
    {
        for(int i = 0; i < spawnManager.GetTubeList().Length; i++)
        {
            if (spawnManager.GetTubeList()[i] == this)
                return i + 1;
        }

        Debug.LogError("Failed to get " + name + " | Tube count = " + spawnManager.GetTubeList().Length);
        return -1;
    }
}