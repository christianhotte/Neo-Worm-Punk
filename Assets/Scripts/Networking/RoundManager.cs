using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoundManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private float roundTime;
    private float timeRemaining;
    private bool roundActive = false; // Whether a round is currently active
    private float timeElapsed;

    // Makes sure that the Round Manager is only instantiated once (for the master client only).

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.InRoom)
            roundTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"];
        else
        {
            roundActive = false;
            return;
        }

        timeRemaining = roundTime;
        StartRound();
    }

    // Update is called once per frame
    void Update()
    {
        if (roundActive)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                EndRound();
            }
            timeElapsed += Time.deltaTime;
        }
    }

    // Starts the countdown when the round begins.
    public void StartRound()
    {
        roundActive = true;
        photonView.RPC("SyncTimer", RpcTarget.AllBuffered, timeRemaining);
    }

    // The end of the round.
    public void EndRound()
    {
        roundActive = false;
        timeRemaining = roundTime;
        NetworkManagerScript.instance.LoadSceneWithFade(GameSettings.roomScene);
    }

    // Updates the timer to display the same time for every client.
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send the current time remaining to other players
            stream.SendNext(timeRemaining);
        }

        else
        {
            // Receive the current time remaining from the network
            timeRemaining = (float)stream.ReceiveNext();
        }
    }

    // Communicates with the server to make sure everybody's timer is synced throughout each client.
    [PunRPC]
    public void SyncTimer(float syncedTimeRemaining)
    {
        timeRemaining = syncedTimeRemaining;
    }

    public void ForceEndRound()
    {
        if(PhotonNetwork.IsMasterClient)
            photonView.RPC("RPC_EndRound", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_EndRound() => EndRound();

    public string GetTimeDisplay() => GetMinutes() + ":" + GetSeconds();
    public string GetMinutes() => Mathf.FloorToInt(timeRemaining / 60f < 0 ? 0 : timeRemaining / 60f).ToString();
    public string GetSeconds() => Mathf.FloorToInt(timeRemaining % 60f < 0 ? 0 : timeRemaining % 60f).ToString("00");
    public float GetTotalSecondsLeft() => timeRemaining;
    public float GetCurrentRoundTime() => timeElapsed;
    public float LevelTimePercentage() => 100f - ((timeRemaining / roundTime) * 100f);
}