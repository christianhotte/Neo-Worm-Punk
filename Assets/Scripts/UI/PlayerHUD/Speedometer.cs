using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    [SerializeField, Tooltip("The frequency in which the speedometer updates.")] private float updateFrequency;
    private TextMeshProUGUI speedometerText;
    private float currentTimer;

    private void Awake()
    {
        speedometerText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTimer > updateFrequency)
        {
            currentTimer = 0;
            double mph = (PlayerController.instance.bodyRb.velocity.magnitude * 2.23694);
            if(mph < 0.01)
                speedometerText.text = "Speed: " + (PlayerController.instance.bodyRb.velocity.magnitude * 2.23694).ToString("F0") + " mph";
            else
                speedometerText.text = "Speed: " + (PlayerController.instance.bodyRb.velocity.magnitude * 2.23694).ToString("F2") + " mph";
        }
        else
            currentTimer += Time.deltaTime;
    }
}
