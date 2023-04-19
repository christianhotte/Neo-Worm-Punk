using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public enum Tutorial { GUNS, TURN, MOVE, CHAINSAW, HOOKSHOT, PARRY }

    [SerializeField] private string[] tutorialLabels;
    [SerializeField] private string[] tutorialMessages;

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
        PlayerController.instance.inverteboy.UpdateTutorialText(tutorialMessages[(int)tutorialSegment], tutorialLabels[(int)tutorialSegment]);
        currentTutorialSegment = tutorialSegment;
    }

    public void DisplayTutorial(int tutorialSegment)
    {
        PlayerController.instance.inverteboy.ShowInverteboyPopup("Spineless Benefactor");
        PlayerController.instance.inverteboy.Flash();
        PlayerController.instance.inverteboy.UpdateTutorialText(tutorialMessages[tutorialSegment], tutorialLabels[tutorialSegment]);
        currentTutorialSegment = (Tutorial)tutorialSegment;
        ResetTutorialTask();
    }

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

    public void IncrementTutorialProgress()
    {
        switch (currentTutorialSegment)
        {
            case Tutorial.GUNS:
                targetsShot++;
                if (targetsShot < targetGoal)
                    PlayerController.instance.inverteboy.UpdateTutorialProgress("Shoot Five Targets.\n" + targetsShot + " / " + targetGoal);
                else
                    PlayerController.instance.inverteboy.UpdateTutorialProgress("Task Complete.");
                break;
        }
    }

    public void DisplayEmptyText()
    {
        PlayerController.instance.inverteboy.UpdateTutorialText("No Incoming Messages.");
    }

    public Tutorial GetCurrentTutorialSegment() => currentTutorialSegment;
}
