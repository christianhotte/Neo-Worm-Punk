using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines properties of grappling hook tool.
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/HookshotSettings", order = 1)]
public class HookshotSettings : ScriptableObject
{
    //ENUMS:
    /// <summary>
    /// Describes what happens when a grapple line is intersected by a non-player obstruction.
    /// </summary>
    [System.Serializable]
    public enum LineIntersectBehavior
    {
        [Tooltip("Nothing will happen when grapple line is intersected. Only the hook at the end can hit stuff (line may pass through walls).")] Ignore,
        [Tooltip("Hook will immediately release if anything intersects with its tehter (punishing).")] Release,
        [Tooltip("Hook will immediately grab onto anything that intersects its tether.")] Grab
    }

    //SETTINGS:
    [Header("General:")]
    [Tooltip("Name of hook prefab used by this tool (make sure this refers to a projectile in the Resources/Projectiles folder).")] public string hookResourceName;
    [Tooltip("Layers which hookshot will bounce off of automatically and cannot be used on.")]                                      public LayerMask bounceLayers;
    [Tooltip("Layers which hookshot will check for when looking for line intersections (should just be most obstructions).")]       public LayerMask lineCheckLayers;
    [Tooltip("Determines what happens when a non-player collider intersects the tether line while hook is traveling.")]             public LineIntersectBehavior travelIntersectBehavior = LineIntersectBehavior.Ignore;
    [Tooltip("Determines what happens when a non-player collider intersects the tether line while hook is locked on.")]             public LineIntersectBehavior hookedIntersectBehavior = LineIntersectBehavior.Ignore;
    [Min(0), Tooltip("Base speed at which hook pulls player toward hooked objects.")]                                               public float basePullSpeed;
    [Min(0), Tooltip("Speed at which hook returns to player while retracting.")]                                                    public float baseRetractSpeed;
    [Min(0), Tooltip("Amount by which retract speed increases every second hook is retracting.")]                                   public float retractAcceleration;
    [Tooltip("Conversion multiplier for relative lateral arm movement used to maneuver player while hook is locked to wall.")]      public float lateralManeuverForce;
    [Tooltip("Conversion multiplier for relative lateral arm movement used to pull player faster toward hook.")]                    public float yankForce;
    [Header("Feel:")]
    [Range(0, 1), Tooltip("How much the player must squeeze the grip in order to launch the grappling hook.")] public float deployThreshold = 1;
    [Range(0, 1), Tooltip("How much the player must release the grip for the grappling hook to reel back.")]   public float releaseThreshold = 0.5f;
    [Header("Extras:")]
    [Min(0), Tooltip("How much velocity player must punch with to trigger Insta-Hook system.")]                                              public float punchWhipSpeed;
    [Min(0), Tooltip("Minimum distance at which full punch whip can be triggered on an object (also distance at which hook will release).")] public float minPunchWhipDist;
    [Min(1), Tooltip("Multiplier for reel-in speed after a punch-whip.")]                                                                    public float punchWhipBoost;
    [Min(0), Tooltip("Amount of time grappler needs to cool down for after completing a punch-whip.")]                                       public float punchWhipCooldown;
    [Tooltip("Causes weapon on matching hand to become holstered while grappling hook is in use.")]                                          public bool holstersWeapon;
    [Header("Haptics:")]
    [Tooltip("Haptic vibration made when player launches the hook.")]          public PlayerEquipment.HapticData launchHaptics;
    [Tooltip("Haptic vibration made when hook is traveling through the air.")] public PlayerEquipment.HapticData reelHaptics;
    [Tooltip("Haptic vibration made when hook hits something.")]               public PlayerEquipment.HapticData hitHaptics;
    [Tooltip("Haptic vibration made when player releases the hook.")]          public PlayerEquipment.HapticData releaseHaptics;
    [Header("Sounds:")]
    [Tooltip("Sound played when hook is launched and while it is in the air.")] public AudioClip launchSound;
    [Tooltip("Sound played when player lands a successful punch-whip.")]        public AudioClip whipSound;
    [Tooltip("Sound played while hook is being reeled back toward player.")]    public AudioClip reelSound;
    [Tooltip("Sound played when hook lets go of something it's locked on to.")] public AudioClip releaseSound;
    [Tooltip("Sound made when hook digs into a surface.")]                      public AudioClip hitSound;
    [Tooltip("Sound made when hook hits another player.")]                      public AudioClip playerHitSound;
    [Tooltip("Sound made when hook bounces off an incompatible surface.")]      public AudioClip bounceSound;
}
