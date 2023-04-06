using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines player health, healing & death properties.
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/HealthSettings", order = 1)]
public class HealthSettings : ScriptableObject
{
    [Header("General:")]
    [Min(1), Tooltip("Base starting health value.")]                                                                                  public int defaultHealth = 3;
    [Min(0), Tooltip("Rate (in units per second) that health regenerates (set to zero to disable regeneration).")]                    public float regenSpeed = 0;
    [Min(0), Tooltip("Number of seconds to wait after damage before beginning regeneration (ignore if health does not regenerate).")] public float regenPauseTime = 0;
    [Min(0), Tooltip("Number of seconds player spends in death zone before spawning back into game.")]                                public float deathTime = 3;
    [Header("Effects:")]
    [Tooltip("Base sound object makes when it is damaged.")] public AudioClip hurtSound;
    [Tooltip("Sound object makes when it is destroyed.")]    public AudioClip deathSound;
    [Header("Gameplay:")]
    [Min(0), Tooltip("How much time after spawning player has before they become vulnerable.")] public float spawnInvincibilityTime;
}
