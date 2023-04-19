using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public enum Tutorial { GUNS, TURN, MOVE, CHAINSAW, HOOKSHOT, PARRY }

    [SerializeField, Tooltip("The different tutorial segments.")] private TutorialSegment[] tutorialSegments;

    private Tutorial currentTutorialSegment;

    //Progress variables
    private int targetsShot;
    private int targetGoal;

    private void Start()
    {
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
        }
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
        }
    }

    /// <summary>
    /// Calls whenever a task is completed.
    /// </summary>
    private void OnTaskComplete()
    {
        PlayerController.instance.inverteboy.UpdateTutorialProgress("Task Complete.");
        switch (currentTutorialSegment)
        {
            default:
                //Task completed
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
}

[System.Serializable]
public class TutorialSegment
{
    public string label;
    public string message;
    public Sprite diagram;
}
