using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public enum Tutorial { GUNS, TURN, MOVE, CHAINSAW, HOOKSHOT, PARRY }

    [SerializeField, Tooltip("The different tutorial segments.")] private TutorialSegment[] tutorialSegments;
    [SerializeField, Tooltip("The tutorial checkpoint locations.")] private Transform[] checkpoints;

    private Tutorial currentTutorialSegment;
    private Transform playerObject;

    //Progress variables
    private int targetsShot;
    private int targetGoal;

    private int timesParried;
    private int parryGoal;

    [SerializeField] Transform SpawnPoint;


    private void Start()
    {
        playerObject = PlayerController.instance.xrOrigin.transform;
        DisplayEmptyText();
        Invoke("StartTutorial", 2f);
    }

    private void StartTutorial()
    {
        PlayerController.instance.inverteboy.ShowInverteboyPopup("Spineless Benefactor");
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
        PlayerController.instance.inverteboy.ShowInverteboyPopup("Spineless Benefactor");
        PlayerController.instance.inverteboy.Flash();
        PlayerController.instance.inverteboy.UpdateTutorialText(tutorialSegments[tutorialSegment].message, tutorialSegments[tutorialSegment].label, tutorialSegments[tutorialSegment].diagram);
        currentTutorialSegment = (Tutorial)tutorialSegment;
        ResetTutorialTask();
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
                break;
            case Tutorial.PARRY:
                timesParried = 0;
                parryGoal = 3;
                PlayerController.instance.inverteboy.UpdateTutorialProgress("Parry Three Times.\n" + timesParried + " / " + parryGoal);
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
                    OnTaskComplete();
                break;
            case Tutorial.PARRY:
                timesParried++;
                if (timesParried < parryGoal)
                    PlayerController.instance.inverteboy.UpdateTutorialProgress("Parry Three Times.\n" + timesParried + " / " + parryGoal);
                else
                    OnTaskComplete();
                break;
        }
    }

    /// <summary>
    /// Calls whenever a task is completed.
    /// </summary>
    private void OnTaskComplete()
    {
        switch (currentTutorialSegment)
        {
            default:
                //Task completed
                PlayerController.instance.inverteboy.UpdateTutorialProgress("Task Complete.");
                break;
        }
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
        Debug.Log("Returning to Menu");
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
