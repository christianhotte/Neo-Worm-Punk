using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnLights : EventTrigger
{
    [SerializeField] private Light[] lights;

    // Start is called before the first frame update
    void Start()
    {
        OnTriggerEnterEvent.AddListener(() => ToggleLights(true));
    }

    public void ToggleLights(bool turnOn)
    {
        foreach(var light in lights)
        {
            light.gameObject.SetActive(turnOn);
        }
    }
}
