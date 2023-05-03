using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public enum Tutorial { GUNS, TURN, MOVE, CHAINSAW, SPIN, HOOKSHOT, PARRY, SHOOT}

    [SerializeField, Tooltip("The name of the tutorial character.")] private string tutorialCharacterName = "Spineless Benefactor";
    [SerializeField, Tooltip("The different tutorial segments.")] private TutorialSegment[] tutorialSegments;
    [SerializeField, Tooltip("The animators that open when tasks are complete.")] private Animator[] TaskDoors;
    [SerializeField, Tooltip("The tutorial checkpoint locations.")] private Transform[] checkpoints;
    [SerializeField, Tooltip("The parent that holds all of the hoop event triggers.")] private Transform hoopEventContainer;
    public GameObject Chainsaw;
    public GameObject Hookshot;

    private Tutorial currentTutorialSegment;
    private Transform playerObject;

    //Progress variables
    private int targetsShot;
    private int targetGoal;

    private int targetsLookedAt;
    private int targetsLookedAtGoal;

    private int hoopsEntered;
    private int hoopsEnteredGoal;

    private int timesParried;
    private int parryGoal;

    private int movingTargetsShot;
    private int movingTargetsGoal;

    [SerializeField] Transform SpawnPoint;

    private bool taskActive = false;


    private void Start()
    {
        playerObject = PlayerController.instance.xrOrigin.transform;
        DisplayEmptyText();
        ShowHoopTriggers(false);
        Invoke("StartTutorial", 2f);
    }

    private void StartTutorial()
    {
        PlayerController.instance.inverteboy.ShowInverteboyPopup(tutorialCharacterName);
        DisplayTutorial(Tutorial.GUNS);
    }

    /// <summary>
    /// Displays a tutorial message based on the tutorial segment active.
    /// </summary>
    /// <param name="tutorialSegment">The tutorial segment to show the information for.</param>
    public void DisplayTutorial(Tutorial tutorialSegment)
    {
        PlayerController.instance.inverteboy.Flash();
        PlayerController.instance.inverteboy.UpdateTutorialText(tutorialSegments[(int)tutorialSegment].message, tutorialSegments[(int)tutorialSegment].label, tutorialSegments[(int)tutorialSegment].diagram);
        currentTutorialSegment = tutorialSegment;
        ResetTutorialTask();
    }

    /// <summary>
    /// Displays a tutorial message based on the tutorial segment active.
    /// </summary>
    /// <param name="tutorialSegment">The tutorial segment to show the information for.</param>
    public void DisplayTutorial(int tutorialSegment)
    {
        PlayerController.instance.inverteboy.ShowInverteboyPopup(tutorialCharacterName);
        PlayerController.instance.inverteboy.Flash();
        PlayerController.instance.inverteboy.UpdateTutorialText(tutorialSegments[tutorialSegment].message, tutorialSegments[tutorialSegment].label, tutorialSegments[tutorialSegment].diagram);
        currentTutorialSegment = (Tutorial)tutorialSegment;
        ResetTutorialTask();
    }

    public void UpdateTutorialMessage(string newMessage, string newLabel = "")
    {
        PlayerController.instance.inverteboy.ShowInverteboyPopup(tutorialCharacterName);
        PlayerController.instance.inverteboy.Flash();
        PlayerController.instance.inverteboy.UpdateTutorialText(newMessage, newLabel, tutorialSegments[(int)currentTutorialSegment].diagram);
    }

    /// <summary>
    /// Resets the tutorial task based on the tutorial segment active.
    /// </summary>
    public void ResetTutorialTask()
    {
        switch (currentTutorialSegment)
        {
            case Tutorial.GUNS:
                targetsShot = 0;
                targetGoal = 5;
                PlayerController.instance.inverteboy.UpdateTutorialProgress("Shoot Five Targets.\n"+ targetsShot +" / " + targetGoal);
                taskActive = true;
                break;
            case Tutorial.TURN:
                targetsLookedAt = 0;
                targetsLookedAtGoal = 2;
                PlayerController.instance.inverteboy.UpdateTutorialProgress("Look At The Two Targets.\n" + targetsLookedAt + " / " + targetsLookedAtGoal);
                taskActive = true;
                break;
            case Tutorial.CHAINSAW:
                ShowHoopTriggers(true);
                hoopsEntered = 0;
                hoopsEnteredGoal = 5;
                PlayerController.instance.inverteboy.UpdateTutorialProgress("Pass Through Five Of The Hoops.\n" + hoopsEntered + " / " + hoopsEnteredGoal);
                taskActive = true;
                break;
            case Tutorial.SPIN:
                DestroyAllHoopTriggers();
                PlayerController.instance.inverteboy.UpdateTutorialProgress("");
                break;
            case Tutorial.PARRY:
                timesParried = 0;
                parryGoal = 3;
                PlayerController.instance.inverteboy.UpdateTutorialProgress("Parry Three Times.\n" + timesParried + " / " + parryGoal);
                taskActive = true;
                break;
            case Tutorial.SHOOT:
                movingTargetsShot = 0;
                movingTargetsGoal = 10;
                PlayerController.instance.inverteboy.UpdateTutorialProgress("Shoot The Moving Targets Ten Times.\n" + movingTargetsShot + " / " + movingTargetsGoal);
                taskActive = true;
                break;
            default:
                PlayerController.instance.inverteboy.UpdateTutorialProgress("");
                break;
        }
    }

    public void MoveSpawnPoint(int checkpoint)
    {
        SpawnPoint.position = checkpoints[checkpoint].position;
        SpawnPoint.rotation = checkpoints[checkpoint].rotation;
    }

    public void OnTargetLookedAt()
    {
        if (currentTutorialSegment == Tutorial.TURN)
            IncrementTutorialProgress();
    }

    public void OnHoopEntered()
    {
        if (currentTutorialSegment == Tutorial.CHAINSAW)
            IncrementTutorialProgress();
    }

    public void DestroyAllHoopTriggers()
    {
        foreach (Transform trans in hoopEventContainer)
            Destroy(trans.gameObject);
    }

    public void ShowHoopTriggers(bool showTriggers)
    {
        foreach (Transform trans in hoopEventContainer)
            trans.gameObject.SetActive(showTriggers);
    }

    /// <summary>
    /// Increments the tutorial task in progress.
    /// </summary>
    public void IncrementTutorialProgress()
    {
        switch (currentTutorialSegment)
        {
            case Tutorial.GUNS:
                targetsShot++;
                if (targetsShot < targetGoal)
                    PlayerController.instance.inverteboy.UpdateTutorialProgress("Shoot Five Targets.\n" + targetsShot + " / " + targetGoal);
                else
                {
                    OnTaskComplete();
                    OnCompleteOpen(0);
                }
                break;
            case Tutorial.TURN:
                targetsLookedAt++;
                if (targetsLookedAt < targetsLookedAtGoal)
                    PlayerController.instance.inverteboy.UpdateTutorialProgress("Look At The Two Targets.\n" + targetsLookedAt + " / " + targetsLookedAtGoal);
                else
                {
                    OnTaskComplete();
                    OnCompleteOpen(1);
                }
                break;
            case Tutorial.CHAINSAW:
                hoopsEntered++;
                if (hoopsEntered < hoopsEnteredGoal)
                    PlayerController.instance.inverteboy.UpdateTutorialProgress("Enter Five Of The Hoops Near You.\n" + hoopsEntered + " / " + hoopsEnteredGoal);
                else
                {
                    OnTaskComplete();
                    OnCompleteOpen(2);
                }
                break;
            case Tutorial.PARRY:
                timesParried++;
                if (timesParried < parryGoal)
                    PlayerController.instance.inverteboy.UpdateTutorialProgress("Parry Three Times.\n" + timesParried + " / " + parryGoal);
                else
                {
                    OnTaskComplete();
                    OnCompleteOpen(3);
                }
                break;
            case Tutorial.SHOOT:
                movingTargetsShot++;
                if (movingTargetsShot < movingTargetsGoal)
                    PlayerController.instance.inverteboy.UpdateTutorialProgress("Shoot The Moving Targets Ten Times.\n" + movingTargetsShot + " / " + movingTargetsGoal);
                else
                {
                    OnTaskComplete();
                    OnCompleteOpen(4);
                }
                break;
        }
    }

    /// <summary>
    /// Calls whenever a task is completed.
    /// </summary>
    private void OnTaskComplete()
    {
        if (taskActive)
        {
            switch (currentTutorialSegment)
            {
                case Tutorial.SHOOT:
                    //Task completed
                    PlayerController.instance.inverteboy.UpdateTutorialProgress("Task Complete.");
                    UpdateTutorialMessage("That's all from me, soldier. Enter the wormhole when you're done, and we'll see you in the battle arena.");
                    break;
                case Tutorial.CHAINSAW:
                    DestroyAllHoopTriggers();
                    PlayerController.instance.inverteboy.UpdateTutorialProgress("Task Complete.");
                    UpdateTutorialMessage("Good job. Move onto the next section.");
                    break;
                default:
                    //Task completed
                    PlayerController.instance.inverteboy.UpdateTutorialProgress("Task Complete.");
                    UpdateTutorialMessage("Good job. Move onto the next section.");
                    break;
            }

            taskActive = false;
        }
    }

    public void OnCompleteOpen(int animatorIndex)
    {
        TaskDoors[animatorIndex].SetBool("Locked", false);
    }

    public void GetChainsaw()
    {
        Chainsaw.SetActive(true);
    }

    public void GetHookshot()
    {
        Hookshot.SetActive(true);
    }

    /// <summary>
    /// Displays empty text for when there are no active tutorials.
    /// </summary>
    public void DisplayEmptyText()
    {
        PlayerController.instance.inverteboy.UpdateTutorialText("No Incoming Messages.");
        PlayerController.instance.inverteboy.UpdateTutorialProgress("");
    }

    public Tutorial GetCurrentTutorialSegment() => currentTutorialSegment;

    public void ReturnToMenu()
    {
        //GameManager.Instance.LoadGame(GameSettings.titleScreenScene);
        Debug.Log("Returning to Menu...");
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        playerObject.GetComponentInChildren<FadeScreen>().FadeOut();

        yield return new WaitForSeconds(playerObject.GetComponentInChildren<FadeScreen>().GetFadeDuration());
        yield return null;

        GameManager.Instance.LoadGame(GameSettings.titleScreenScene);
    }
}

[System.Serializable]
public class TutorialSegment
{
    public string label;
    public string message;
    public Sprite diagram;
}
