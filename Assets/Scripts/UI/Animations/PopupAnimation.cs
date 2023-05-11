using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupAnimation : MonoBehaviour
{
    [Header("Starting Animation Settings")]
    [SerializeField, Tooltip("The starting position of the pop up.")] private Vector2 startPosition;
    [SerializeField, Tooltip("The amount of units the pop up will move during the animation.")] private Vector2 popupMovement;
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
        popupTransform.anchoredPosition = startPosition;
        gameObject.transform.localScale = Vector3.one;
        PopupStartAnimation();
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
    }

    private void PopupStartAnimation()
    {
        LeanTween.moveX(popupTransform, startPosition.x + popupMovement.x, startMovementSpeed).setEase(easeInCurve);
        LeanTween.moveY(popupTransform, startPosition.y + popupMovement.y, startMovementSpeed).setEase(easeInCurve).setOnComplete(PopupEndAnimation);
    }

    private void PopupEndAnimation()
    {
        LeanTween.delayedCall(popupDuration, () => LeanTween.scale(gameObject, Vector3.zero, endMovementSpeed).setEase(easeOutCurve)).setDestroyOnComplete(true);
    }
}
