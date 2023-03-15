using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomEnums;

/// <summary>
/// Determines base properties of a ballistic projectile.
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ProjectileSettings", order = 1)]
public class ProjectileSettings : ScriptableObject
{
    [Header("Hit Effect:")]
    [Min(0), Tooltip("How much damage this projectile deals to targets it hits.")] public int damage = 1;
    [Min(0), Tooltip("How much force this projectile applies to struck objects.")] public float knockback = 0;
    [Min(0), Tooltip("Object spawned when projectile burns out or hits a wall.")]  public GameObject explosionPrefab;
    
    [Header("Travel Properties:")]
    [Tooltip("Speed at which projectile travels upon spawn.")]                                     public float initialVelocity;
    [Tooltip("Maximum distance projectile can travel (leave zero to make infinite).")]             public float range = 0;
    [Min(0), Tooltip("Distance in front of barrel position at which projectile actually spawns.")] public float barrelGap = 0;
    [Min(0), Tooltip("Optional amount of bullet drop (in meters per second).")]                    public float drop = 0;
    
    [Header("Targeting:")]
    [Min(0), Tooltip("How intensely projectiles home in toward targets (set to zero to disable homing).")]                                                     public float homingStrength;
    [Tooltip("Curve describing intensity of projectile homing throughout the course of its range.")]                                                           public AnimationCurve homingStrengthCurve;
    [MinMaxSlider(0, 180), Tooltip("Angle at which targeting system can lock on (second component is angle at which projectile will ignore target entirely.")] public Vector2 targetDesignationAngle;
    [Tooltip("Curve describing multiplier for target designation angle throughout the course of projectile's range")]                                          public AnimationCurve targetAngleCurve;
    [Min(0), Tooltip("Maximum distance at which projectile can acquire a target.")]                                                                            public float targetingDistance;
    [Tooltip("Curve describing homing distance multiplier throughout course of projectile's range.")]                                                          public AnimationCurve targetingDistanceCurve;
    [Tooltip("Require line-of-sight for active targeting.")]                                                                                                   public bool LOSTargeting;
    [Tooltip("Sets projectile target acquisition to always run, even when a target has already been found.")]                                                  public bool alwaysLookForTarget;
    [Range(0, 1), Tooltip("Amount by which projectile will use target velocity to predict movement and attempt interception.")]                                public float predictionStrength;
    [Range(0, 1), Tooltip("Slide to the left to prioritize easier-to-hit targets, slide to the right to prioritize closer targets.")]                          public float angleDistancePreference;
    [Tooltip("Layers to ignore when raycasting for line-of-sight targeting.")]                                                                                 public LayerMask targetingIgnoreLayers;
    
    [Header("Collision:")]
    [Tooltip("Physics layers which projectile will not collide with.")]                                                         public LayerMask ignoreLayers;
    [Tooltip("Extra physics layers which projectile will ignore during cast with radius (reccomended to add obstacles here).")] public LayerMask radiusIgnoreLayers;
    [Tooltip("Radius used by remote projectile scripts to manually check for non-networked targets.")]                          public float dumbTargetAquisitionRadius = 1;
    [Min(0), Tooltip("Wideness of projectile collision zone (zero if projectile is a point).")]                                 public float radius = 0;
}