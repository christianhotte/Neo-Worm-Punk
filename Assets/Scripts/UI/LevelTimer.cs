using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;

public class LevelTimer : MonoBehaviour
{
    private int levelTime;  //The desired timer for the level
    private float currentTime;  //The current time left

    private TextMeshProUGUI timerText;  //The timer text component

    private bool timerActivated;  //If true, the timer has been activated. If false, the timer has not been activated
    private bool timerActive;   //If true, the timer is active. If false, the timer is not active
    private bool timerEnded;    //If true, the timer has ended. If false, the timer has not ended

    public UnityEvent OnTimerEnd;  //The event to call when the timer ends

    private void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
        timerActivated = false;
        timerActive = false;

        if(!GameManager.Instance.InMenu())
            ActivateTimer();
    }

    private void OnEnable()
    {
        OnTimerEnd.AddListener(BackToLockerRoom);
    }

    /// <summary>
    /// Gets the room's round length and applies it to the timer.
    /// </summary>
    private void ActivateTimer()
    {
        if (PhotonNetwork.CurrentRoom != null && !timerActivated)
        {
            Debug.Log("Round Length: " + (int)PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"]);
            if ((int)PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"] < 0)
            {
                timerActive = false;
                timerText.color = new Color(0, 0, 0, 0);
                timerActivated = true;
            }
            else
            {
                Debug.Log("Timer Started!");
                SetLevelTime((int)PhotonNetwork.CurrentRoom.CustomProperties["RoundLength"]);
                currentTime = levelTime;
                timerActive = true;
                timerActivated = true;
            }
        }
    }

    private void OnDisable()
    {
        OnTimerEnd.RemoveListener(BackToLockerRoom);
    }

    private void Update()
    {
        //If the timer is active
        if (timerActive)
        {
            //If there is time left, decrement the timer
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                DisplayTime();
            }
            //If there is no time, end the timer and call the subscribed event(s)
            else if (!timerEnded)
            {
                currentTime = 0;
                DisplayTime();

                Debug.Log("Time Is Up!");
                OnTimerEnd.Invoke();
                timerEnded = true;
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Displays the time in minutes and seconds.
    /// </summary>
    private void DisplayTime()
    {
        timerText.text = GetMinutes() + ":" + GetSeconds();
    }

    /// <summary>
    /// Brings all of the players in the scene back to the locker room.
    /// </summary>
    public void BackToLockerRoom()
    {
        NetworkManagerScript.instance.LoadSceneWithFade(NetworkManagerScript.instance.roomScene);
    }

    public string GetMinutes() => Mathf.FloorToInt(currentTime / 60f).ToString();
    public string GetSeconds() => Mathf.FloorToInt(currentTime % 60f).ToString("00");

    public void SetLevelTime(int newLevelTime) => levelTime = newLevelTime;
}
