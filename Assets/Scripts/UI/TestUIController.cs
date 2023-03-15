using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestUIController : MonoBehaviour
{
    [SerializeField, Tooltip("Test value text objects.")] private TextMeshProUGUI toggleText, dialText, sliderText, leverText, unlockText;

    /// <summary>
    /// Updates the test toggle text.
    /// </summary>
    public void ToggleText()
    {
        GameSettings.toggleTest = !GameSettings.toggleTest;
        if (GameSettings.toggleTest)
            toggleText.text = "Toggle: On";
        else
            toggleText.text = "Toggle: Off";
    }

    /// <summary>
    /// Updates the test dial text.
    /// </summary>
    /// <param name="val">The value of the dial.</param>
    public void UpdateDialText(float val)
    {
        GameSettings.testDialValue = val;
        dialText.text = "Dial Value: " + GameSettings.testDialValue.ToString("F0");
    }

    /// <summary>
    /// Updates the test slider text.
    /// </summary>
    /// <param name="val">The current value of the slider.</param>
    public void UpdateSliderText(float val)
    {
        GameSettings.testSliderValue = val;
        sliderText.text = "Slider Value: " + GameSettings.testSliderValue.ToString("F0");
    }

    /// <summary>
    /// Updates the test lever text.
    /// </summary>
    /// <param name="val">The current value of the lever.</param>
    public void UpdateLeverText(float val)
    {
        leverText.text = "Lever Value: " + val.ToString("F0");
    }

    public void UnlockText(bool isUnlocked)
    {
        if (isUnlocked)
            unlockText.text = "Key Unlocked: True";
        else
            unlockText.text = "Key Unlocked: False";
    }
}
