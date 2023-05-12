using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerController : MonoBehaviour
{
    [Header("ALL Conveyers")]
    [SerializeField, Tooltip("teehee this is a fake tooltip bet you'll never find this Hotte")] private Transform[] conveyerBeltStopPositions;
    [SerializeField, Tooltip("All of the objects on the conveyer belt")] private Transform[] conveyerBeltObjects;
    [SerializeField, Tooltip("How much time it takes for objects to get from conveyerbelt stop positions")] private float transportTime;
    [SerializeField, Tooltip("Reference to MenuStationController")] private MenuStationController menuStationControllerRef;
    [SerializeField, Tooltip("Indicates whether or not this is the player's conveyerbelt")] private bool isPlayerBelt;

    [Header("PLAYER Conveyer")]
    [SerializeField, Tooltip("Ref to the launch tube")] private Transform tube;
    [SerializeField, Tooltip("Ref to the launch tube Window")] private Transform tubeWindow;
    [SerializeField, Tooltip("Ref to the lobbyUI controller")] private LobbyUIScript lubbyUIScriptRef;
    [SerializeField, Tooltip("Ref to the FindRoomUI controller")] private FindRoomController findRoomControllerRef;

    [Header("Doors")]
    [SerializeField, Tooltip("The door that separates the credits from the name area.")] private BigDoorController creditsDoor;

    private bool conveyerBeltIsMoving = false;
    private int[] newConveyerObjectPositions;
    private bool changingHereBoi = false;
    private bool createRoomOption;
    private bool yeetNotYetNoob = false;
    private bool tutorialOption = false;
    private bool sandboxOption = false;
    private float saveTransportTime;



    /// <summary>
    /// Initialize and fill the storage of object index positioning
    /// </summary>
    private void Start()
    {
        saveTransportTime = 1f;
        //if this is not the conveyerbelt start repeatedely calling the conveyerbelt to move
        newConveyerObjectPositions = new int[conveyerBeltObjects.Length];

        if (!isPlayerBelt)
        {
            //set position to where they are
            for(int i = 0; i < conveyerBeltObjects.Length; i++)
            {
                newConveyerObjectPositions[i] = i;
                /*
                //if the conveyer position is equal to the current position of the belt, then set it to that position index
                for (int j = 0; j < conveyerBeltStopPositions.Length; j++)
                {
                    if(conveyerBeltStopPositions[j].position.z == conveyerBeltObjects[i].position.z) { newConveyerObjectPositions[i] = j; }
                }
                */
            }
            //make the belt move automatically and start somewhat randomly
            InvokeRepeating("DisplayBeltMove", Random.Range(0.5f, 6f), transportTime * 1.5f);
        }
        else
        {
            //set position to same
            for (int i = 0; i < conveyerBeltObjects.Length; i++)
            {
                newConveyerObjectPositions[i] = 0;
            }
        }
    }

    /// <summary>
    /// move conveyer objects all to different belt positions
    /// </summary>
    /// <param name="nextBeltPositions"></param>
    public void MoveConveyer(int[] nextBeltPositions)
    {
        //generate new positions for the 

        //only calls the coroutine if it is not currently runningmove the conveyer one slot for
        if (!conveyerBeltIsMoving) { StartCoroutine(MovingConveyerBelt(nextBeltPositions)); }
        //safety
        conveyerBeltIsMoving = true;
    }

    ///move conveyer objects all to the same positions
    public void MoveConveyer(int nextBeltPosition)
    {
        //generates an int array to feed coroutine
        int[] nextBeltPositions = new int[conveyerBeltObjects.Length];
        for(int i = 0; i < conveyerBeltObjects.Length; i++)
        {
            nextBeltPositions[i] = nextBeltPosition;
        }
        //only calls the coroutine if it is not currently runningmove the conveyer one slot for
        if (!conveyerBeltIsMoving) { StartCoroutine(MovingConveyerBelt(nextBeltPositions)); }
        //safety
        conveyerBeltIsMoving = true;
        lubbyUIScriptRef.UpdateErrorMessage("");
    }

    public void TeleportConveyer(int nextBeltPosition)
    {
        int[] nextBeltPositions = new int[conveyerBeltObjects.Length];
        for (int i = 0; i < conveyerBeltObjects.Length; i++)
            nextBeltPositions[i] = nextBeltPosition;

        //only do if this is the player's belt
        if (isPlayerBelt)
        {
            menuStationControllerRef.DeactivateAllOtherStations(nextBeltPositions[0]); //retract all menu ui that is down except for the next one
            menuStationControllerRef.ActivateStation(nextBeltPositions[0]);
        }

        //make arrays to store starting + ending positions
        Vector3[] startPositions = new Vector3[conveyerBeltObjects.Length];
        Vector3[] endPositions = new Vector3[conveyerBeltObjects.Length];

        //assign the arrays wit the correct starting and ending positions
        for (int i = 0; i < conveyerBeltObjects.Length; i++)
        {
            startPositions[i] = conveyerBeltObjects[i].position;
            endPositions[i] = new Vector3(conveyerBeltObjects[i].position.x, conveyerBeltObjects[i].position.y, conveyerBeltStopPositions[nextBeltPositions[i]].position.z);
        }

        for (int i = 0; i < conveyerBeltObjects.Length; i++)
            conveyerBeltObjects[i].position = endPositions[i];

        conveyerBeltIsMoving = false;

        //If the player is teleported to the credits, move them immediately back to the starting area
        if (isPlayerBelt && nextBeltPosition == 8 && creditsDoor != null)
        {
            transportTime = 12f;
            MoveConveyer(0);
            creditsDoor.EnableDoors(10);
        }
    }

    /// <summary>
    /// This Coroutine moves all of the objects along the conveyer belt to their target slot
    /// </summary>
    /// <param name="nextBeltPositions"></param>
    /// <returns></returns>
    private IEnumerator MovingConveyerBelt(int[] nextBeltPositions)
    {
        //safety first folks
        conveyerBeltIsMoving = true;

        //only do if this is the player's belt
        if (isPlayerBelt)
        {
            //retract all menu ui that is down except for the next one
            menuStationControllerRef.DeactivateAllOtherStations(nextBeltPositions[0]);
        }

        //time for menuUI to start retracting
        yield return new WaitForSeconds(0.4f);

        //make arrays to store starting + ending positions
        Vector3[] startPositions = new Vector3[conveyerBeltObjects.Length];
        Vector3[] endPositions = new Vector3[conveyerBeltObjects.Length];

        //assign the arrays wit the correct starting and ending positions
        for (int i = 0; i < conveyerBeltObjects.Length; i++)
        {
            startPositions[i] = conveyerBeltObjects[i].position;
            endPositions[i] = new Vector3(conveyerBeltObjects[i].position.x, conveyerBeltObjects[i].position.y, conveyerBeltStopPositions[nextBeltPositions[i]].position.z);
        }

        //set initial time to 0
        float timeElapsed = 0;

        //lerp the objects from start to end positions
        while(timeElapsed < transportTime)
        {
            //just in case, stay safe :)
            if (GameManager.Instance.levelTransitionActive) { break; }

            //smooth lerp duration alg
            float t = timeElapsed / transportTime;
            t = t * t * (3f - 2f * t);

            //loops through all of the conveyer belt objects and lerps them from start to end
            for (int i = 0; i < conveyerBeltObjects.Length; i++)
            {
                conveyerBeltObjects[i].position = Vector3.Lerp(startPositions[i], endPositions[i], t);
            }

            //advance time
            timeElapsed += Time.deltaTime;

            
            //only do if this is the player's belt
            if (isPlayerBelt)
            {
                //activate menu II anim if under 0.25 seconds from ending
                if (timeElapsed > (transportTime - 0.25f))
                {
                    if(nextBeltPositions[0] == 7 && !changingHereBoi)
                    {
                        changingHereBoi = true;
                        StartCoroutine(TransitionToNewSceneSequence());
                    }
                    else if(nextBeltPositions[0] != 7)
                    {
                        menuStationControllerRef.ActivateStation(nextBeltPositions[0]);
                    }
                }
            }
            yield return null;
        }

        transportTime = saveTransportTime;
        //sall gud to call this coroutine again :)
        conveyerBeltIsMoving = false;

        //If the player has moved to the credits section, move them immediately back to the starting area
        if (isPlayerBelt && nextBeltPositions[0] == 8 && creditsDoor != null)
        {
            MoveConveyer(0);
            creditsDoor.EnableDoors(10);
            transportTime = 12f;
        }
    }

    private IEnumerator TransitionToNewSceneSequence()
    {
        changingHereBoi = true;

        float timeElapsed = 0;
        float endTransportTime = 0.25f;

        Vector3 startYPos = tubeWindow.localPosition;
        Vector3 endYPos = new Vector3(startYPos.x, 0.12f, startYPos.z);


        //lerp door up
        while (timeElapsed < endTransportTime)
        {
            //just in case, stay safe :)
            if (GameManager.Instance.levelTransitionActive) { break; }

            //smooth lerp duration alg
            float t = timeElapsed / endTransportTime;
            t = t * t * (3f - 2f * t);

            tubeWindow.localPosition = Vector3.Lerp(startYPos, endYPos, t);

            //advance time
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        //reset timer
        timeElapsed = 0f;
        endTransportTime = 0.7f;

        Vector3 startYRot = tube.localRotation.eulerAngles;
        Vector3 endYRot = Vector3.zero;



        //lerp door around
        while(timeElapsed < endTransportTime)
        {
            //just in case, stay safe :)
            if (GameManager.Instance.levelTransitionActive) { break; }

            //smooth lerp duration alg
            float t = timeElapsed / endTransportTime;
            t = t * t * (3f - 2f * t);

            tube.localRotation = Quaternion.Euler(Vector3.Lerp(startYRot, endYRot, t));

            //advance time
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        tube.localRotation = Quaternion.Euler(endYRot);

        timeElapsed = 0f;
        endTransportTime = 3.0f;

        startYPos = tube.localPosition;
        Vector3 secondStartYPos = conveyerBeltObjects[0].position;
        endYPos = startYPos;
        endYPos += new Vector3(0, 10, 0);
        Vector3 secondEndYPos = secondStartYPos + new Vector3(0, 10, 0);
        yeetNotYetNoob = false;

        //lerp whole tube + player up
        while (timeElapsed < endTransportTime)
        {
            //will  cut this off once you start loading the new scene
            if (GameManager.Instance.levelTransitionActive) { break; }

            tube.localPosition = Vector3.Lerp(startYPos, endYPos, timeElapsed / endTransportTime);
            conveyerBeltObjects[0].position = Vector3.Lerp(secondStartYPos, secondEndYPos, timeElapsed / endTransportTime);

            if (!yeetNotYetNoob && timeElapsed > 1.0f)
            {
                yeetNotYetNoob = true;
                StartCoroutine(FadeToBlackAndGoToNextScene(0.5f));
            }

            //advance time
            timeElapsed += Time.deltaTime;

            yield return null;
        }
    }

    private IEnumerator FadeToBlackAndGoToNextScene(float loadDelay)
    {
        yeetNotYetNoob = true;

        conveyerBeltObjects[0].GetComponentInChildren<FadeScreen>().FadeOut();

        if (PlayerController.instance.hudScreen.activeInHierarchy)
            PlayerController.instance.HideHUD(0.5f);

        yield return new WaitForSeconds(conveyerBeltObjects[0].GetComponentInChildren<FadeScreen>().GetFadeDuration());
        yield return new WaitForSeconds(loadDelay);

        //join or create room based on which one works
        if(tutorialOption)
        {
            GameManager.Instance.LoadGame(GameSettings.tutorialScene);
        }
        else if (sandboxOption)
        {
            GameManager.Instance.LoadGame(GameSettings.arenaScene);
        }
        else if (createRoomOption)
        {
            Debug.Log("You SHOULD be creating and joining a room right now");
            lubbyUIScriptRef.CreateRoom();
        }
        else
        {
            Debug.Log("You SHOULD be joining someone else's room right now");
            findRoomControllerRef.ConnectToRoom();
        }
    }

    public void CreateRoomOptionChosen()
    {
        createRoomOption = true;
        tutorialOption = false;
        sandboxOption = false;
    }

    public void JoinRoomOptionChosen()
    {
        findRoomControllerRef.SetRoomToConnectTo();
        createRoomOption = false;
        tutorialOption = false;
        sandboxOption = false;
    }

    public void TutorialOptionChosen()
    {
        tutorialOption = true;
    }

    public void SandboxOptionChosen()
    {
        sandboxOption = true;
    }

    /// <summary>
    /// Persona5 sucks. Yes this is a direct attack on Peter. Yes I'm joking. Period.
    /// </summary>
    private void DisplayBeltMove()
    {
        //advances conveyer positions by 1 and loops them
        for (int i = 0; i < newConveyerObjectPositions.Length; i++)
        {
            //add one to the position and loop if past the limit of conveyerbelt positions
            newConveyerObjectPositions[i] = MyUtils.MyMod(newConveyerObjectPositions[i] + 1, conveyerBeltStopPositions.Length);

            //teleport objects who have looped from one end of the belt to the other in index to that new index
            if (newConveyerObjectPositions[i] == 0)
            {
                conveyerBeltObjects[i].position = new Vector3(conveyerBeltObjects[i].position.x, conveyerBeltObjects[i].position.y, conveyerBeltStopPositions[0].position.z);
                newConveyerObjectPositions[i] = 1;
            }
        }

        //move the conveyer belt
        MoveConveyer(newConveyerObjectPositions);
    }
}
