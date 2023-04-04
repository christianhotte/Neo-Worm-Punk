using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasDistanceScaler : MonoBehaviour
{
    [SerializeField, Tooltip("The minimum distance for the canvas to constantly scale.")] private float minimumDistance = 10f;
    [SerializeField, Tooltip("The maximum distance for the canvas to constantly scale.")] private float maximumDistance = 50f;

    [SerializeField, Tooltip("The minimum scale for the canvas.")] private float minimumScale = 1f;
    [SerializeField, Tooltip("The maximum scale for the canvas")] private float maximumScale = 5f;

    // Update is called once per frame
    void Update()
    {
        //Rotates the canvas so that it is always looking at the player
        transform.LookAt(PlayerController.instance.xrOrigin.transform, Vector3.up);
        transform.Rotate(0f, 180f, 0f);

        //Gets the current distance between the canvas the player is seeing and the player's XR origin
        float currentDistance = Mathf.Clamp(Vector3.Distance(transform.position, PlayerController.instance.xrOrigin.transform.position), minimumDistance, maximumDistance);
        float currentScale = Mathf.Lerp(minimumScale, maximumScale, GetPercentBetweenRange(currentDistance));   //Gets a percentage between minimum and maximum scale
        transform.localScale = Vector3.one * (0.01f * currentScale);    //Sets the scale of the canvas
    }

    private float GetPercentBetweenRange(float currentDistance) => (currentDistance - minimumDistance) / (maximumDistance - minimumDistance);
}
