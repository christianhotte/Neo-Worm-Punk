using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public enum Tutorial { MOVEMENT, HOOKSHOT, SHOTGUN, CHAINSAW }

    [SerializeField] private string[] tutorialLabels;
    [SerializeField] private string[] tutorialMessages;

    private void Start()
    {
        Invoke("StartTutorial", 2f);
    }

    private void StartTutorial()
    {
        PlayerController.instance.inverteboy.ShowInverteboyPopup("Spineless Benefactor");
        DisplayTutorial(Tutorial.MOVEMENT);
    }

    /// <summary>
    /// Displays a tutorial message based on the tutorial segment active.
    /// </summary>
    /// <param name="tutorialSegment">The tutorial segment to show the information for.</param>
    public void DisplayTutorial(Tutorial tutorialSegment)
    {
        PlayerController.instance.inverteboy.Flash();
        PlayerController.instance.inverteboy.UpdateTutorialText(tutorialMessages[(int)tutorialSegment], tutorialLabels[(int)tutorialSegment]);
    }
}
