using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Altitude : MonoBehaviour
{
    [SerializeField, Tooltip("The frequency in which the altitude indicator updates.")] private float updateFrequency;
    private TextMeshProUGUI altitudeText;
    private float currentTimer;
    private int maxRoomAltitude = 300;

    private void Awake()
    {
        altitudeText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTimer > updateFrequency)
        {
            currentTimer = 0;
            int altitude = Mathf.CeilToInt(PlayerController.instance.xrOrigin.transform.position.y) - maxRoomAltitude;
            altitudeText.text = altitude.ToString() + " m";
        }
        else
            currentTimer += Time.deltaTime;
    }
}
