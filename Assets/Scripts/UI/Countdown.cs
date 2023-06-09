using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Countdown : MonoBehaviour
{
    
    [SerializeField] private GameObject hideCanvas;
    [SerializeField] private GameObject countdownContainer;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private string countdownEndMessage = "Worm!";
    [SerializeField] private bool hideEndMessage;

    [SerializeField] private float entranceDuration;
    [SerializeField] private AnimationCurve entranceEaseType;
    [SerializeField] private float exitDuration;
    [SerializeField] private AnimationCurve exitEaseType;

    private LTDescr entranceTween, exitTween;
    private bool cancelCountdown;

    public void StartCountdown(int seconds)
    {
        StartCoroutine(CountdownSequence(seconds));
    }

    private IEnumerator CountdownSequence(int seconds)
    {
        if(hideCanvas != null)
            hideCanvas.SetActive(false);

        countdownContainer.SetActive(true);

        for(int i = seconds; i > 0 && !cancelCountdown; i--)
        {
            float second = 1;
            while (second > 0 && !cancelCountdown)
            {
                if (second == 1)
                {
                    countdownContainer.transform.localScale = Vector3.zero;
                    entranceTween = LeanTween.scale(countdownContainer, Vector3.one, entranceDuration).setEase(entranceEaseType);
                    exitTween = LeanTween.scale(countdownContainer, Vector3.zero, exitDuration).setEase(exitEaseType);
                    countdownText.text = i.ToString();
                }

                second -= Time.deltaTime;
                yield return null;
            }
        }

        if (cancelCountdown)
        {
            if (hideCanvas != null)
                hideCanvas.SetActive(true);

            countdownContainer.SetActive(false);

            if (entranceTween != null)
                LeanTween.cancel(entranceTween.id);

            if (exitTween != null)
                LeanTween.cancel(exitTween.id);
        }
        else
        {
            countdownText.text = countdownEndMessage;

            countdownContainer.transform.localScale = Vector3.zero;
            entranceTween = LeanTween.scale(countdownContainer, Vector3.one, entranceDuration).setEase(entranceEaseType).setOnComplete(OnEndMessage);
        }
    }

    private void OnEndMessage()
    {
        if (hideEndMessage)
            exitTween = LeanTween.scale(countdownContainer, Vector3.zero, exitDuration).setEase(exitEaseType).setOnComplete(() => countdownContainer.SetActive(false));
    }

    public void CancelCountdown() => cancelCountdown = true;
}
