using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines properties of player shotgun.
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ShotgunSettings", order = 1)]
public class ShotgunSettings : ScriptableObject
{
    [Header("Resources:")]
    [Tooltip("Name of projectile prefab fired by this weapon (make sure this refers to a projectile in the Resources/Projectiles folder).")] public string projectileResourceName;
    [Tooltip("Prefab for shells ejected by weapon when reloading.")]                                                                         public GameObject shellPrefab;
    [Header("Mechanical Properties:")]
    [Min(1), Tooltip("Maximum number of shots which can be loaded into weapon (not necessarily equal to number of barrels).")]               public int maxLoadedShots = 2;
    [Min(1), Tooltip("Number of projectiles spawned whenever weapon is fired.")]                                                             public int projectilesPerShot = 1;
    [Range(0, 90), Tooltip("Maximum random spread angle added to every projectile fired.")]                                                  public float shotSpread = 0;
    [Range(0, 90), Tooltip("Angle which barrels snap to when breach is open.")]                                                              public float breakAngle = 45;
    [Min(0), Tooltip("How much time weapon needs to be left open for in order for it to be reloaded.")]                                      public float cooldownTime = 0.7f;
    [Min(0), Tooltip("If both shotguns are fired within this amount of time, player will get a speed boost.")]                               public float doubleFireTime = 0.25f;
    [Min(1), Tooltip("Fire velocity multiplier used when shotguns are double-fired.")]                                                       public float doubleFireBoost = 1.8f;
    [Tooltip("How long to wait after emptying gun before automatically ejecting (set negative to turn off this feature).")]                  public float emptyEjectWait = 0.6f;
    [Header("Locomotion:")]
    [Min(0), Tooltip("Magnitude of velocity imparted on player when weapon is fired (primary locomotion setting).")]                  public float fireVelocity;
    [Min(1), Tooltip("Multiplier applied to launch velocity when player is firing backwards.")]                                       public float reverseFireBoost;
    [Min(1), Tooltip("Greatest multiplier which can be applied to fire velocity due to closeness to a wall")]                         public float maxWallBoost;
    [Min(0), Tooltip("Distance from wall at which wall boost power will begin to take effect (closer means more power).")]            public float maxWallBoostDist;
    [Tooltip("If player fires weapon near a wall, always give them the maximum possible boost.")]                                     public bool alwaysMaxWallBoost = true;
    [Tooltip("Layers which weapon will consider a wall for the purposes of wallboosting.")]                                           public LayerMask wallBoostLayers;
    [Min(0), Tooltip("Radius of sphere used to check whether or not shotgun is in a wall.")]                                          public float wallCheckSphereRadius = 1f;
    [Min(1), Tooltip("If player fires within this angle of their current velocity, shot will add velocity instead of replacing it.")] public float additiveVelocityMaxAngle;
    [Range(0, 1), Tooltip("Multiplier applied to velocity addition when player is firing multiple times in the same direction.")]     public float additiveVelocityMultiplier;
    [Tooltip("Modulates additive velocity multiplier depending on how aligned shot is with current velocity.")]                       public AnimationCurve additiveVelocityCurve;
    [Header("Gunfeel:")]
    [Tooltip("Forces player to wait for end of recoil phase before they are able to fire again.")] public bool noFireDuringRecoil = true;
    [Range(0, 1), Tooltip("How far back the player has to pull the trigger before it fires.")]     public float triggerThreshold = 1;
    [Min(0), Tooltip("Dampens gun wobble when pressed to make aiming a bit easier.")]              public float triggerDamper = 20;
    [Min(0), Tooltip("Strength of force used to close breach when swinging guns vertically.")]     public float closerForce;
    [Min(0), Tooltip("Time to wait after opening breach before allowing swing-close assistance.")] public float swingCloseWait;
    [Tooltip("When true, shotguns will only bounce player when shooting directly at a wall.")]     public bool wallBoostOnly;
    [Tooltip("Enables natural shotgun recoil boosting.")]                                          public bool neutralFireBoost = false;
    [Min(0), Tooltip("Time player has to hold eject button to activate secondary mode.")]          public float modeHoldTime = 0.25f;
    [Space()]
    [Min(0), Tooltip("Approximate number of seconds it takes weapon to rotate 180 degrees during reverse fire procedure.")]          public float reverseSpeed = 0.15f;
    [Min(0), Tooltip("Minimum duration between reverse button presses (prevents spamming that jams up guns).")]                      public float reverseButtonCooldown = 0.2f;
    [Min(0), Tooltip("Minimum speed at which player must swing barrel in order to flip it in a particular direction (up or down).")] public float directionalReverseSwingSpeed;
    [Space()]
    [Min(0), Tooltip("Length of linear recoil weapon goes through when fired.")]                           public float recoilDistance;
    [Min(0.01f), Tooltip("Amout of time (in seconds) gun spends in linear recoil phase.")]                 public float recoilTime;
    [Tooltip("Describes linear recoil motion over time.")]                                                 public AnimationCurve recoilCurve;
    [Min(1), Tooltip("Maximum scale multiplier weapon reaches during recoil phase.")]                      public float recoilScale = 1;
    [Tooltip("Describes scale modulation throughout recoil phase.")]                                       public AnimationCurve recoilScaleCurve;
    [Min(0), Tooltip("Maximum vertical rotation of weapon (in degrees) during recoil phase.")]             public float recoilRotation;
    [Tooltip("Describes weapon rotation throughout recoil phase.")]                                        public AnimationCurve recoilRotationCurve;
    [Space()]
    [Min(0), Tooltip("Distance barrels move backward when reciprocating after firing")]       public float barrelReciprocationDistance;
    [Tooltip("Curve describing the motion of barrel reciprocation throughout recoil phase.")] public AnimationCurve barrelReciproCurve;
    [Min(0), Tooltip("Max distance ejector nubbins can move along their rails.")]             public float ejectorTraverseDistance;
    [Min(0), Tooltip("Amount of time ejectors take to travel to their target positions.")]    public float ejectorTraverseTime;
    [Tooltip("Describes motion of ejector during traversal.")]                                public AnimationCurve ejectorTraverseCurve;
    [Min(0), Tooltip("Amount forward firing pins move when weapon is fired.")]                public float pinTraverseDistance;
    [Header("Effects:")]
    [Tooltip("Settings for configuring the vibration player feels when firing.")]                             public PlayerController.HapticData fireHaptics;
    [Tooltip("Settings for configuring the vibration player feels when ejecting shells.")]                    public PlayerController.HapticData ejectHaptics;
    [Tooltip("Settings for configuring the vibration player feels when closing the breach.")]                 public PlayerController.HapticData closeHaptics;
    [Tooltip("Magnitude (x) and duration (y) of screenshake event when weapon is fired.")]                    public Vector2 fireScreenShake;
    [Header("Sounds:")]
    [Tooltip("SFX for when weapon is fired")]          public AudioClip fireSound;
    [Tooltip("SFX for when breach is opened")]         public AudioClip ejectSound;
    [Tooltip("SFX for when breach is closed")]         public AudioClip lockSound;
    [Tooltip("SFX for when weapon is flipped around")] public AudioClip flipSound;
}
