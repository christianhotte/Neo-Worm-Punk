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
        DisplayTutorial(Tutorial.MOVEMENT);
    }

    public void DisplayTutorial(Tutorial tutorialSegment)
    {
        PlayerController.instance.inverteboy.UpdateTutorialText(tutorialMessages[(int)tutorialSegment], tutorialLabels[(int)tutorialSegment]);
    }
}
