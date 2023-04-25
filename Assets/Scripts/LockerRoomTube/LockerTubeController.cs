using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LockerTubeController : MonoBehaviour
{
    private LockerTubeSpawner spawnManager;

    [SerializeField, Tooltip("The parent that holds all of the ready lights.")] private Transform readyLights;
    [SerializeField, Tooltip("The spawn point for the player's name.")] private Transform playerNameSpawnPoint;
    [SerializeField, Tooltip("The prefab that displays the player's name.")] private GameObject playerNamePrefab;
    [SerializeField, Tooltip("The GameObject for the host settings.")] private GameObject hostSettings;

    //internal int tubeNumber;
    //public bool occupied = false;
    /// <summary>
    /// ID of player which is currently in this tube.
    /// </summary>
    internal int currentPlayerID;
    internal Transform spawnPoint;

    private Transform myPlayerObject;

    private void Awake()
    {
        spawnManager = FindObjectOfType<LockerTubeSpawner>();
        spawnPoint = transform.Find("Spawnpoint");
        transform.localPosition -= new Vector3(0, 10, 0);
    }

    private void Start()
    {

        //when all players start the match
        //do countdown and have the middle shotgun thing spin (last chance to put levers back)

        //stop spin, move players together, locked in.
        //StartCoroutine(MoveTubeAndPlayer((transform.localPosition + transform.forward * 4), 6));

        //THEN move up
        //StartCoroutine(MoveTubeAndPlayer((transform.localPosition + new Vector3(0, 10, 0)), 8));
        //in the middle of the last one, fade out and switch scenes

    }

    public void StartTube(Transform playerObject, float duration)
    {
        myPlayerObject = playerObject;
        StartCoroutine(MoveTubeAndPlayer((new Vector3(0, 10, 0)), duration));
    }

    public void TubesToCenter(float duration)
    {
        StartCoroutine(MoveTubeAndPlayer((transform.forward * 4), duration));
    }

    public void TubesUp(float duration)
    {
        StartCoroutine(MoveTubeAndPlayer(new Vector3(0, 10, 0), duration));
    }

    /// <summary>
    /// Moves the tube and player to the next location they should go to
    /// </summary>
    /// <param name="transformChange"></param>
    /// <param name="moveTime"></param>
    /// <returns></returns>
    IEnumerator MoveTubeAndPlayer(Vector3 transformChange, float moveTime)
    {
        //set initial time to 0
        float timeElapsed = 0;

        Vector3 startPos = transform.localPosition;
        Vector3 endPos = transform.localPosition + transformChange;
        Vector3 playerStartPos = Vector3.zero;
        Vector3 playerEndPos = Vector3.zero;


        if (myPlayerObject != null)
        {
            playerStartPos = myPlayerObject.position;
            playerEndPos = myPlayerObject.position + transformChange;
        }

        //lerp the objects from start to end positions
        while (timeElapsed < moveTime)
        {
            //just in case, stay safe :)
            if (GameManager.Instance.levelTransitionActive) { break; }

            //smooth lerp duration alg
            float t = timeElapsed / moveTime;
            t = t * t * (3f - 2f * t);

            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            if(myPlayerObject != null)
            {
                myPlayerObject.position = Vector3.Lerp(playerStartPos, playerEndPos, t);
            }
            
            
            //advance time
            timeElapsed += Time.deltaTime;

            yield return null;
        }
    }

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

    /// <summary>
    /// Shows the host settings in the tube.
    /// </summary>
    /// <param name="showSettings">If true, the host settings are shown. If false, they are hidden.</param>
    public void ShowHostSettings(bool showSettings)
    {
        hostSettings.SetActive(showSettings);
    }

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