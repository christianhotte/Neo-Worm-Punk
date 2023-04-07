using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines properties of player chainsaw.
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ChainsawSettings", order = 1)]
public class ChainsawSettings : ScriptableObject
{
    [Header("Mechanical Settings:")]
    [Tooltip("Base speed and max speed at which chainsaw can grind along walls and floors.")]                              public Vector2 grindSpeedRange;
    [Min(0), Tooltip("Time player must continuously grind to reach maximum grind speed.")]                                 public float grindAccelTime;
    [Tooltip("Layers which player can grind on.")]                                                                         public LayerMask grindLayers;
    [MinMaxSlider(0, 1), Tooltip("Range of positions along the blade which player will magnetize to while wallgrinding.")] public Vector2 grindSweetSpot;
    [Tooltip("Effective width of blade hitbox (should be more or less the actual size of the real blade.")]                public float bladeWidth;
    [Min(0), Tooltip("Force applied to player away from surface when disengaging from grind.")]                            public float disengageForce;
    [Header("Animation:")]
    [Min(0), Tooltip("Amount by which blade is pulled back as player squeezes the trigger.")]      public float bladePreRetractDistance;
    [Tooltip("Curve describing motion of pre-retraction, evaluated based on trigger pull value.")] public AnimationCurve bladePreRetractCurve;
    [Space()]
    [Min(0), Tooltip("How far the blade travels when it is extended.")]      public float bladeTraverseDistance;
    [Min(0), Tooltip("How much time blade takes to extend when activated.")] public float bladeExtendTime;
    [Tooltip("Describes motion of blade over time as it extends.")]          public AnimationCurve bladeExtendCurve;
    [Tooltip("Describes motion of blade over time as it retracts.")]         public AnimationCurve bladeRetractCurve;
    [Space()]
    [Min(0), Tooltip("How far forward wrist joint extends when blade is deployed.")]                              public float wristExtendDistance;
    [Range(0, 1), Tooltip("Percentage of total bladeExtendTime during which wrist is extending as well.")]        public float wristDeployPeriod;
    [Tooltip("Curve describing rate of wrist deployment as chainsaw is being activated/deactivated.")]            public AnimationCurve wristDeployCurve;
    [Range(0, 180), Tooltip("Greatest angle wrist joint is allowed to rotate to.")]                               public float maxWristAngle;
    [Min(0), Tooltip("How quickly wrist lerps to match player hand rotation while chainsaw is deployed.")]        public float wristLerpRate;
    [Min(0), Tooltip("Speed (in degrees per second) at which wrist returns to base rotation during retraction.")] public float wristRotReturnRate;
    [Header("Deflection:")]
    [Min(0), Tooltip("How long it takes for blade to move into (and from) deflect position.")]          public float deflectTransTime;
    [Min(0), Tooltip("Radius of deflection area while deflection is active.")]                          public float deflectRadius;
    [Min(0), Tooltip("Speed at which blade rotates during deflection mode.")]                           public float deflectRotRate;
    [Min(0), Tooltip("Distance wrist extends forward during deflection mode.")]                         public float deflectWristExtend;
    [Min(0), Tooltip("Additive force used to pull player while in deflect mode (helicopter-style).")]   public float deflectPullForce;
    [Min(0), Tooltip("Maximum amount of time player can spend in deflection mode.")]                    public float deflectTime;
    [Min(0), Tooltip("How many seconds of deflect time player recovers per second of not deflecting.")] public float deflectCooldownRate;
    [Range(0, 180), Tooltip("Maximum tolerated angle at which projectiles can be deflected.")]          public float deflectionAngle = 90;
    [Space()]
    [Range(0, 180), Tooltip("Angle blade turns to when set to reverse mode.")]                          public float reverseGripAngle;
    [Min(0), Tooltip("How rapidly blade moves to and from reverse grip position.")]                     public float reverseGripLerpRate;
    [Min(0), Tooltip("How rapidly blade returns from reverse grip position when blade is retracting.")] public float reverseGripReturnRate;
    [Header("Feel & Input:")]
    [MinMaxSlider(0, 1), Tooltip("How much player needs to squeeze the trigger in order to activate/release the blade.")] public Vector2 gripThresholds;
    [MinMaxSlider(0, 1), Tooltip("How much player needs to squeeze the trigger in order to activate/release the blade.")] public Vector2 triggerThresholds;
    [Tooltip("Lower and upper bounds of speed at which player can swing blade to activate/extend plasma extender.")]      public Vector2 swingSpeedRange;
    [Header("Sounds:")]
    [Tooltip("Sound chainsaw makes when it first extends.")]                                 public AudioClip extendSound;
    [Tooltip("Sound chainsaw makes while it is actively running.")]                          public AudioClip runningSound;
    [Tooltip("Sound chainsaw makes while it is actively running (while buried in a wall).")] public AudioClip runningWallSound;
    [Tooltip("Sound chainsaw makes when player releases it.")]                               public AudioClip sheathSound;
    [Tooltip("Sound made when player deflects a projectile with the chainsaw.")]             public AudioClip deflectSound;
    [Header("Haptics:")]
    [Tooltip("Haptic effect which plays when player extends their blade.")]                                            public PlayerController.HapticData extendHaptics;
    [Tooltip("Haptic effect which plays when player retracts their blade.")]                                           public PlayerController.HapticData retractHaptics;
    [Tooltip("Base properties of haptic pulses which are played continuously while blade is active.")]                 public PlayerController.HapticData activeHapticPulse;
    [Tooltip("Maximum value of random decrease for pulse magnitude while chainsaw is active.")]                        public float activeHapticMagnitudeVariance;
    [Tooltip("While chainsaw is active, pulses will be separated by random time values between zero and this value.")] public float activeHapticFrequencyVariance;
}
