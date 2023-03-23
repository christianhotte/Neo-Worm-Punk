using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a temporary effect which is spawned in the scene by an event and despawns after a certain period of time.
/// </summary>
public class EffectSystem : MonoBehaviour
{
    public float duration;
    private float timeAlive = 0;

    private void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive > duration) Destroy(gameObject);
    }
}
