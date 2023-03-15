using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines how a projectile homes in on targets (if targeting is enabled).
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TargetingSettings", order = 1)]
public class TargetingSettings : ScriptableObject
{
    [Header("Base Properties:")]
    [Min(0), Tooltip("How intensely projectiles home in toward targets (set to zero to disable homing).")]                                                     public float homingStrength;
    [MinMaxSlider(0, 180), Tooltip("Angle at which targeting system can lock on (second component is angle at which projectile will ignore target entirely.")] public Vector2 targetDesignationAngle;
    [Min(0), Tooltip("Maximum distance at which projectile can acquire a target.")]                                                                            public float targetingDistance;
    [Range(0, 1), Tooltip("Slide to the left to prioritize easier-to-hit targets, slide to the right to prioritize closer targets.")]                          public float angleDistancePreference;
    
    [Header("Additional Options:")]
    [Tooltip("Sets projectile target acquisition to always run, even when a target has already been found.")] public bool alwaysLookForTarget;
    [Tooltip("Require line-of-sight for active targeting.")]                                                  public bool LOSTargeting;
    [Tooltip("Layers to ignore when raycasting for line-of-sight targeting.")]                                public LayerMask targetingIgnoreLayers;
    
    [Header("Performance:")]
    [Range(0, 3), Tooltip("Projectile will use target velocity to predict movement and attempt interception, increase iterations for better accuracy (expensive).")] public int predictionIterations;
    [Min(1), Tooltip("Number of times per second projectile runs targeting function during target acquisition period.")]                                             public int targetingTickRate = 30;
}