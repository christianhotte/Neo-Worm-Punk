using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupAnimation : MonoBehaviour
{
    [Header("Starting Animation Settings")]
    [SerializeField, Tooltip("The starting position of the pop up.")] private float startYPosition;
    [SerializeField, Tooltip("The amount of units the pop up will move during the animation.")] private float yMovement;
    [SerializeField, Tooltip("The speed of the start animation.")] private float startMovementSpeed;
    [SerializeField, Tooltip("The amount of time the pop up will show before exiting.")] private float popupDuration;
    [SerializeField, Tooltip("The ease curve of the start animation.")] private LeanTweenType easeInCurve;
    [Space(10)]

    [Header("Ending Animation Settings")]
    [SerializeField, Tooltip("The speed of the end animation.")] private float endMovementSpeed;
    [SerializeField, Tooltip("The ease curve of the end animation")] private LeanTweenType easeOutCurve;

    private RectTransform popupTransform;

    private void Awake()
    {
        popupTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Vector3 startPos = popupTransform.anchoredPosition;
        startPos.y = startYPosition;
        popupTransform.anchoredPosition = startPos;
        gameObject.transform.localScale = Vector3.one;
        PopupStartAnimation();
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
    }

    private void PopupStartAnimation()
    {
        LeanTween.moveY(popupTransform, startYPosition + yMovement, startMovementSpeed).setEase(easeInCurve).setOnComplete(PopupEndAnimation);
    }

    private void PopupEndAnimation()
    {
        LeanTween.delayedCall(popupDuration, () => LeanTween.scale(gameObject, Vector3.zero, endMovementSpeed).setEase(easeOutCurve)).setDestroyOnComplete(true);
    }
}
