using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] private bool fadeOnStart = true;
    [SerializeField] private float fadeDuration = 2;
    [SerializeField] private Color fadeColor = new Color(0, 0, 0, 1);

    private Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        if (fadeOnStart)
            FadeIn();
    }

    public void FadeIn() => Fade(1, 0);
    public void FadeOut() => Fade(0, 1);

    private void Fade(float alphaIn, float alphaOut)
    {
        StartCoroutine(FadeRoutine(alphaIn, alphaOut));
    }

    private IEnumerator FadeRoutine(float alphaIn, float alphaOut)
    {
        float timer = 0;

        while (timer <= fadeDuration)
        {
            Color newColor = fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);

            rend.material.SetColor("_BaseColor", newColor);

            timer += Time.deltaTime;
            yield return null;
        }

        Color finalNewColor = fadeColor;
        finalNewColor.a = alphaOut;
        rend.material.SetColor("_BaseColor", finalNewColor);
    }

    public float GetFadeDuration() => fadeDuration;
}
