using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LogEntry : MonoBehaviour
{
    [SerializeField, Tooltip("The text on the log.")] private TextMeshProUGUI logText;

    [SerializeField] private float startAnimationDuration;
    [SerializeField] private float delayDuration;
    [SerializeField] private float endAnimationDuration;

    [SerializeField] private LeanTweenType startingAnimationEaseType;
    [SerializeField] private LeanTweenType endingAnimationEaseType;

    // Start is called before the first frame update
    void Start()
    {
        LogAnimation();
    }

    private void LogAnimation()
    {
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, startAnimationDuration).setEase(startingAnimationEaseType).setOnComplete(
            () => LeanTween.delayedCall(delayDuration, () => LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 0f, endAnimationDuration).setEase(endingAnimationEaseType).setDestroyOnComplete(true)));
    }

    public void UpdateLogText(string message)
    {
        logText.text = message;
    }
}
