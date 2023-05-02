using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class RoundTimer : MonoBehaviour
{
    private RoundManager roundManager;
    private TextMeshProUGUI timerText;

    private void Awake()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //jumboAud = this.GetComponent<AudioSource>();
        PhotonView masterPV = PhotonView.Find(17);
        roundManager = masterPV.GetComponent<RoundManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Gets the timer from the Round Manager which is synced throughout the network.
        if (roundManager != null)
        {
            string remainingTime = roundManager.GetTimeDisplay();
            timerText.text = remainingTime;
        }
        else
        {
            timerText.text = "";
        }
    }
}
