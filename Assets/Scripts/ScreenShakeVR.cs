using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//CREDIT: Original screenshake code: http://www.zulubo.com/gamedev/2019/1/5/vr-screen-shake-the-art-of-not-throwing-up
//NOTE: This version has been modified by Christian Hotte

public class ScreenShakeVR : MonoBehaviour
{
    //Classes, Structs & Enums:
    public class ShakeEvent
    {
        //Data:
        public float magnitude;
        public float length;
        private float exponent;
        private float time;
        public bool finished { get { return time >= length; } }
        public float currentStrength { get { return magnitude * Mathf.Clamp01(1 - time / length); } }

        //RUNTIME METHODS:
        public ShakeEvent(float mag, float len, float exp = 2)
        {
            magnitude = mag;
            length = len;
            exponent = exp;
        }
        public void Update(float deltaTime)
        {
            time += deltaTime; //Update time tracker
        }
    }

    //Objects & Components:
    private Material material;

    //Settings:
    [SerializeField, Tooltip("Base intensity of shake effect")] private float baseMagnitude = 0.1f;
    [SerializeField, Tooltip("Base speed of shake effect")] private float baseFrequency = 20f;

    //Runtime Vars:
    private float shakeVal;
    private float shakeCumulation;
    private List<ShakeEvent> activeShakes = new List<ShakeEvent>();

    //RUNTIME METHODS:
    void Awake()
    {
        if (material != null) material.shader = Shader.Find("Hidden/ScreenShakeVR");
        else material = new Material(Shader.Find("Hidden/ScreenShakeVR"));
    }
    private void OnEnable()
    {
        Awake();
    }
    private void Update()
    {
        shakeCumulation = 0;
        //iterate through all active shake events
        for (int i = activeShakes.Count - 1; i >= 0; i--)
        {
            //accumulate their current magnitude
            activeShakes[i].Update(Time.deltaTime);
            shakeCumulation += activeShakes[i].currentStrength;
            //and remove them if they've finished
            if (activeShakes[i].finished)
            {
                activeShakes.RemoveAt(i);
            }
        }

        if (shakeCumulation > 0)
        {
            shakeVal = Mathf.PerlinNoise(Time.time * baseFrequency, 10.234896f) * shakeCumulation * baseMagnitude;
        }
        else
        {
            shakeVal = 0;
        }
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Mathf.Approximately(shakeVal, 0) == false)
        {
            material.SetFloat("_ShakeFac", shakeVal);
            Graphics.Blit(source, destination, material);
        }
        else
        {
            //no shaking currently
            Graphics.Blit(source, destination);
        }
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Trigger a shake event.
    /// </summary>
    /// <param name="magnitude">Magnitude of the shaking. Should range from 0 - 1</param>
    /// <param name="length">Length of the shake event.</param>
    /// <param name="exponent">Falloff curve of the shaking</param>
    public void Shake(float magnitude, float length, float exponent = 2)
    {
        activeShakes.Add(new ShakeEvent(magnitude, length, exponent));
    }
}