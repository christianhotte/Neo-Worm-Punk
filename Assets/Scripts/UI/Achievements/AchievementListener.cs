using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementListener : MonoBehaviour
{
    [SerializeField, Tooltip("The list of achievements in the game.")] private AchievementItem[] achievementList;
    [SerializeField, Tooltip("The achievement prefab.")] private GameObject achievementPrefab;
    [SerializeField] private bool debugClearAchievements;

    private List<int> hiddenAchievements = new List<int>();

    public static AchievementListener Instance;

    private void Awake()
    {
        Instance = this;
        InitializeAchievements();
    }

    /// <summary>
    /// Initializes the achievements from the player data when the game starts.
    /// </summary>
    private void InitializeAchievements()
    {
        for(int i = 0; i < achievementList.Length; i++)
        {
            if(PlayerPrefs.GetInt("Achievement" + i) == 1)
            {
                achievementList[i].isUnlocked = true;
                if (achievementList[i].isHidden)
                {
                    hiddenAchievements.Add(i);
                    achievementList[i].isHidden = false;
                }
            }
        }
    }

    /// <summary>
    /// Unlocks an achievement.
    /// </summary>
    /// <param name="index">The index of the achievement in the list.</param>
    public void UnlockAchievement(int index)
    {
        PlayerPrefs.SetInt("Achievement" + index, 1);
        PlayerController.instance.AddAchievementPopup(achievementList[index]);
        achievementList[index].isUnlocked = true;
        if (achievementList[index].isHidden)
            achievementList[index].isHidden = false;
    }

    private void Update()
    {
        if (debugClearAchievements)
        {
            debugClearAchievements = false;
            ClearAchievements();
        }
    }

    public void ClearAchievements()
    {
        for (int i = 0; i < achievementList.Length; i++)
        {
            PlayerPrefs.SetInt("Achievement" + i, 0);
            achievementList[i].isUnlocked = false;
        }

        foreach (var achievement in hiddenAchievements)
            achievementList[achievement].isHidden = true;
    }

    public bool IsAchievementUnlocked(int index) => PlayerPrefs.GetInt("Achievement" + index) == 1;
    public AchievementItem[] GetAchievementList() => achievementList;
    public GameObject GetAchievementPrefab() => achievementPrefab;
}
