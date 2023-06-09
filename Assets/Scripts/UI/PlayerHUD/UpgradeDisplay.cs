using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeDisplay : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField, Tooltip("The image for the upgrade background.")] private Image backgroundImage;
    [SerializeField, Tooltip("The image for the upgrade icon.")] private Image upgradeImage;
    [SerializeField, Tooltip("The text for the timer.")] private TextMeshProUGUI timerText;
    [Space(10)]

    [Header("Icons")]
    [SerializeField] private Sprite multiShotIcon;
    [SerializeField] private Color multiShotColor;
    [SerializeField] private Sprite heatVisionIcon;
    [SerializeField] private Color heatVisionColor;
    [SerializeField] private Sprite invincibilityIcon;
    [SerializeField, Tooltip("The sprite for the rainbow background.")] private Sprite rainbowSprite;
    [Space(10)]

    [Header("Animation Settings")]
    [SerializeField, Tooltip("The time that starts the warning flicker.")] private float startWarningFlickerAt;
    [SerializeField, Tooltip("The alpha for the warning flicker.")] private float warningFlickerAlpha;
    [SerializeField, Tooltip("The speed of the warning flicker.")] private float warningFlickerSpeed;

    [SerializeField, Tooltip("The time that starts the final flicker.")] private float startFinalFlickerAt;
    [SerializeField, Tooltip("The alpha for the final flicker.")] private float finalFlickerAlpha;
    [SerializeField, Tooltip("The speed of the final flicker.")] private float finalFlickerSpeed;

    [SerializeField, Tooltip("The speed in which the upgrade disappears.")] private float disappearSpeed;

    [SerializeField, Tooltip("The ease type for the flickering.")] private LeanTweenType flickerEaseType;

    [Header("Debug Options")]
    [SerializeField] private bool debugUpgrade;
    [SerializeField] private float debugUpgradeTime;

    private float timeRemaining;
    private bool upgradeActive = false;

    private bool warningFlickerStarted;
    private bool finalFlickerStarted;

    private LTDescr warningFlickerLeantween;
    private LTDescr finalFlickerLeantween;

    private Sprite defaultBackgroundImageSprite;

    private void Awake()
    {
        defaultBackgroundImageSprite = backgroundImage.sprite;
    }

    private void OnEnable()
    {
        if (debugUpgrade)
            StartUpgradeTimer(PowerUp.PowerUpType.None, debugUpgradeTime);
    }

    public void StartUpgradeTimer(PowerUp.PowerUpType powerUpType, float upgradeTimer)
    {
        switch (powerUpType)
        {
            case PowerUp.PowerUpType.Invulnerability:
                timeRemaining = upgradeTimer / 2;
                break;
            default:
                timeRemaining = upgradeTimer;
                break;
        }
        UpdateUpgradeDisplay(powerUpType);
        timerText.text = GetTimeDisplay();
        upgradeActive = true;
    }

    private void UpdateUpgradeDisplay(PowerUp.PowerUpType powerUpType)
    {
        switch (powerUpType)
        {
            case PowerUp.PowerUpType.MultiShot:
                backgroundImage.sprite = defaultBackgroundImageSprite;
                backgroundImage.color = multiShotColor;
                upgradeImage.sprite = multiShotIcon;
                break;
            case PowerUp.PowerUpType.HeatVision:
                backgroundImage.sprite = defaultBackgroundImageSprite;
                backgroundImage.color = heatVisionColor;
                upgradeImage.sprite = heatVisionIcon;
                break;
            case PowerUp.PowerUpType.Invulnerability:
                backgroundImage.sprite = rainbowSprite;
                upgradeImage.sprite = invincibilityIcon;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (upgradeActive)
        {
            if(timeRemaining < 0)
            {
                upgradeActive = false;
                StopFinalFlicker();
                timerText.color = new Color(0, 0, 0, 0);
                LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 0f, disappearSpeed).setEase(flickerEaseType).setDestroyOnComplete(true);
            }
            else
            {
                timeRemaining -= Time.deltaTime;
                timerText.text = GetTimeDisplay();

                if (timeRemaining <= startFinalFlickerAt && !finalFlickerStarted)
                    StartFinalFlicker();

                else if (timeRemaining <= startWarningFlickerAt && !warningFlickerStarted)
                    StartWarningFlicker();
            }
        }
    }

    private void StartWarningFlicker()
    {
        warningFlickerStarted = true;
        GetComponent<CanvasGroup>().alpha = 1f;
        warningFlickerLeantween = LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), warningFlickerAlpha, warningFlickerSpeed).setEase(flickerEaseType).setLoopPingPong();
    }

    private void StopWarningFlicker()
    {
        if(warningFlickerLeantween != null)
            LeanTween.cancel(warningFlickerLeantween.id);
    }

    private void StartFinalFlicker()
    {
        StopWarningFlicker();
        finalFlickerStarted = true;
        GetComponent<CanvasGroup>().alpha = 1f;
        finalFlickerLeantween = LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), finalFlickerAlpha, finalFlickerSpeed).setEase(flickerEaseType).setLoopPingPong();
    }

    private void StopFinalFlicker()
    {
        if(finalFlickerLeantween != null)
            LeanTween.cancel(finalFlickerLeantween.id);
    }

    public string GetTimeDisplay() => GetMinutes() + ":" + GetSeconds();
    public string GetMinutes() => Mathf.FloorToInt(timeRemaining / 60f < 0 ? 0 : timeRemaining / 60f).ToString();
    public string GetSeconds() => Mathf.FloorToInt(timeRemaining % 60f < 0 ? 0 : timeRemaining % 60f).ToString("00");
}
