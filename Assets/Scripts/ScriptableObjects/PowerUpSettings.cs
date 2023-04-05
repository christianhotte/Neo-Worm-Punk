using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PowerUpSettings", order = 1)]
public class PowerUpSettings : ScriptableObject
{
    [Header("MultiShot:")]
    [Tooltip("Multiplies the number of projectiles fired by projectile-based equipment.")] public int MS_projectileMultiplier = 2;
    [Tooltip("Increases weapon spread while in multishot mode.")]                          public float MS_spreadAdd = 6;
}
