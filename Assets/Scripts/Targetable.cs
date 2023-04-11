using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Place this on objects to make them (or parts of them) targetable by certain projectiles.
/// </summary>
public class Targetable : MonoBehaviour
{
    //Classes, Eunums & Structs:
    /// <summary>
    /// Denotes which player projectiles will lock on to a target.
    /// </summary>
    public enum TargetType
    {
        /// <summary>Any projectile will be able to home in on this object.</summary>
        All,
        /// <summary>Only player bullets can lock on to this target.</summary>
        BulletsOnly,
        /// <summary>Only player grappling hooks can lock on to this target.</summary>
        HooksOnly
    }

    //Objects & Components:
    /// <summary>
    /// Master list of all targetable objects in scene.
    /// </summary>
    public static List<Targetable> instances = new List<Targetable>();

    //Settings:
    [Tooltip("The point in space which projectiles will target (leave empty to make it this object's transform.")] public Transform targetPoint;
    [Tooltip("Which projectiles will be able to lock on to this targetable instance.")]                            public TargetType type = TargetType.All;
    [Tooltip("Forces hook projectiles to snap to center of object when hit")]                                      public bool hookToCenter = false;

    //Runtime Variables:
    internal bool active = true;

    //RUNTIME VARIABLES:
    private protected virtual void Awake()
    {
        //Initialize:
        instances.Add(this); //Add this targetable object to master list of targetable instances
       // print("Targetable added: " + name);

        //Get objects & components:
        if (targetPoint == null) targetPoint = transform; //Set target point to self if not set in editor
    }
    private protected virtual void OnDestroy()
    {
        //Final cleanup:
        instances.Remove(this); //Remove this object from master list of targetable instances
    }

    //FUNCTIONALITY METHODS:
    public virtual void IsHit(int damage) { }
    public virtual void IsHit(int damage, int playerID, Vector3 velocity) { }
  
}
