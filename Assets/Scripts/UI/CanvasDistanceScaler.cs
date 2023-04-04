using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasDistanceScaler : MonoBehaviour
{
    [SerializeField] private float minimumDistance = 10f;
    [SerializeField] private float maximumDistance = 50f;

    [SerializeField] private float minimumScale = 1f;
    [SerializeField] private float maximumScale = 5f;

    // Update is called once per frame
    void Update()
    {
        float currentDistance = Mathf.Clamp(Vector3.Distance(transform.position, PlayerController.instance.transform.position), minimumDistance, maximumDistance);
        float currentScale = Mathf.Lerp(minimumScale, maximumScale, GetPercentBetweenRange(currentDistance));
        transform.localScale = Vector3.one * (0.01f * currentScale);
    }

    private float GetPercentBetweenRange(float currentDistance) => (currentDistance - minimumDistance) / (maximumDistance - minimumDistance);
}
