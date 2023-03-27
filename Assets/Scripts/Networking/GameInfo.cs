using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class GameInfo : MonoBehaviour
{
    public enum PINGLEVEL { LOW, MEDIUM, HIGH };

    [SerializeField, Tooltip("The colors that indicate the severity of the player's ping.")] private Color[] pingSeverityLevels;

    [SerializeField, Tooltip("The minimum ping for decent ping.")] private float okPing;
    [SerializeField, Tooltip("The minimum ping for bad ping.")] private float badPing;

    [SerializeField, Tooltip("The text to show the ping.")] private TextMeshProUGUI pingText;
    [SerializeField, Tooltip("The text to show the ping.")] private TextMeshProUGUI fpsText;
    [SerializeField, Tooltip("The indicator to show the ping severity.")] private Image pingIndicator;

    private bool isShowingPing = false;
    private bool showFPS = false;


    private int lastFrameIndex;
    private float[] frameDeltaTimeArray;

    private void Awake()
    {
        frameDeltaTimeArray = new float[50];
    }

    // Start is called before the first frame update
    void Start()
    {
        showFPS = GameSettings.debugMode;
    }

    // Update is called once per frame
    void Update()
    {
        //Show ping if the player is connected to the server and ready
        isShowingPing = PhotonNetwork.IsConnectedAndReady;
        GetComponent<Canvas>().enabled = isShowingPing || showFPS;

        if (isShowingPing)
            UpdatePingDisplay();
        else
            pingText.text = "";

        //Show the FPS
        if (showFPS)
            UpdateFPSDisplay();
        else
            fpsText.text = "";
    }

    /// <summary>
    /// Updates the ping display accordingly.
    /// </summary>
    private void UpdatePingDisplay()
    {
        int currentPing = PhotonNetwork.GetPing();
        pingText.text = currentPing.ToString() + " ms";

        if(currentPing >= badPing)
        {
            pingIndicator.color = pingSeverityLevels[(int)PINGLEVEL.HIGH];
        }
        else if(currentPing >= okPing)
        {
            pingIndicator.color = pingSeverityLevels[(int)PINGLEVEL.MEDIUM];
        }
        else
        {
            pingIndicator.color = pingSeverityLevels[(int)PINGLEVEL.LOW];
        }
    }

    /// <summary>
    /// Updates the FPS display accordingly.
    /// </summary>
    private void UpdateFPSDisplay()
    {
        frameDeltaTimeArray[lastFrameIndex] = Time.unscaledDeltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

        fpsText.text = "FPS: " + Mathf.RoundToInt(CalculateFPS()).ToString();
    }

    /// <summary>
    /// Calculates the current average FPS.
    /// </summary>
    /// <returns>The average FPS.</returns>
    private float CalculateFPS()
    {
        float total = 0f;
        foreach (float deltaTime in frameDeltaTimeArray)
        {
            total += deltaTime;
        }

        return frameDeltaTimeArray.Length / total;
    }
}
