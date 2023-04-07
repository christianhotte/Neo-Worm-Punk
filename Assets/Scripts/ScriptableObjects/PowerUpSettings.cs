using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PowerUpSettings", order = 1)]
public class PowerUpSettings : ScriptableObject
{
    [Header("invulnerability:")]
    [Tooltip("Shader applied to players for Invulnerability.")]                            public Material invulnMat;
    [Tooltip("Time player is invulnerable for.")]                                          public float InvulnerableTime;
    [Header("MultiShot:")]
    [Tooltip("Multiplies the number of projectiles fired by projectile-based equipment.")] public int MS_projectileMultiplier = 2;
    [Tooltip("Increases weapon spread while in multishot mode.")]                          public float MS_spreadAdd = 6;
    [Header("HeatVision:")]
    [Tooltip("Shader applied to players for heat vision.")]                                public Material HeatVisMat;
    [Tooltip("Time heat vision power up is active for.")]                                  public float HeatVisionTime;
    [Tooltip("Name of the heat vision projectile.")]                                       public string heatSeekerPrefabName;
    [Tooltip("Sound to alert arena of power up spawn.")]                                   public AudioClip AlertSound;
    [Tooltip("Force power up is shot at with.")]                                           public float LaunchForce;
}
