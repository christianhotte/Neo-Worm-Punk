using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines physical joint properties of PlayerEquipment (simplifies ConfigurableJoint setup).
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/EquipmentJointSettings", order = 1)]
public class EquipmentJointSettings : ScriptableObject
{
    [Header("Follower Properties:")]
    [Tooltip("Offsets equipment from target transform position by given amount.")]               public Vector3 offset;
    [Min(0), Tooltip("Reduces apparent object lag by countering velocity of player rigidbody.")] public float velocityCompensation = 0.01f;
    [Header("Joint Properties:")]
    [Min(0.01f), Tooltip("Mass of object rigidbody.")]                                                     public float mass = 0.5f;
    [Range(0, 180), Tooltip("Range of angular joint motion (in degrees).")]                                public float limitAngle = 160f;
    [Range(0, 1), Tooltip("Force (percentage retained) of bounce when object angle hits angular limit.")]  public float limitBounciness = 0.3f;
    [Min(0), Tooltip("Increase to make angular springs more stiff (set at zero to lock angular limits).")] public float angularSpringiness = 100f;
    [Min(0), Tooltip("Increase to dampen effect of angular springs.")]                                     public float angularDampening = 5f;
    [Space()]
    [Min(0), Tooltip("Strength of spring force keeping equipment at target position")] public float linearDrive = 50f;
    [Min(0), Tooltip("Strength of spring force keeping equipment at target rotation")] public float angularDrive = 500f;
    [Min(0), Tooltip("Increase to dampen effect of angular spring drive")]             public float angularDriveDamper = 3f;
    [Header("Misc Properties:")]
    [Tooltip("Base maximum angular speed of weapon rigidbody (affects weapon swinginess).")]                               public float maxAngularSpeed = 15;
    [Min(1), Tooltip("Number of fixed updates to keep in position memory for smoothing the results of RelativeVelocity.")] public int positionMemory = 10;
    [Min(0), Tooltip("How quickly (in seconds) equipment travels to and returns from its holster.")]                       public float holsterSpeed = 0.2f;
    [Tooltip("Describes holstering (or unholstering) motion over time.")]                                                  public AnimationCurve holsterCurve;
}
