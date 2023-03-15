using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DEPRECATED: Interface for any class which reacts to being shot by player weapons.
/// </summary>
public interface IShootable
{
    /// <summary>
    /// Called when a projectile hits this object.
    /// </summary>
    public void IsHit(Projectile projectile);
}
