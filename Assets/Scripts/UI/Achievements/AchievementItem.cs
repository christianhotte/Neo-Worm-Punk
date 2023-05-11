using UnityEngine;

[System.Serializable]
public class AchievementItem
{
    [Tooltip("The name of the achievement.")] public string name;
    [Tooltip("The description of the achievement.")] public string description;
    [Tooltip("If true, the player has unlocked this achievement.")] public bool isUnlocked;
    [Tooltip("If true, this achievement is hidden in the list until it is unlocked.")] public bool isHidden;
}
