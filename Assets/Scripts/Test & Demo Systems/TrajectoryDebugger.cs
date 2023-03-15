using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to predict the trajectory an object will take/launch objects at.
/// </summary>
public class TrajectoryDebugger : MonoBehaviour
{
    //Objects & Components:


    //Settings:
    [Header("Settings:")]
    [Min(1), SerializeField, Tooltip("Maximum number of points in trajectory arc.")] private int maxArcPoints;
    [SerializeField, Tooltip("Layers to disregard when calculating obstructions.")]  private LayerMask ignoreLayers;

    //Runtime variables:


    //RUNTIME METHODS:
    private void Update()
    {
        
    }
}
