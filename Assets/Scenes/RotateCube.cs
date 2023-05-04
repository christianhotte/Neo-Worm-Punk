using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCube : MonoBehaviour
{
    [Header("Settings:")]
    [SerializeField, Tooltip("Speed at which the cube rotates.")] private float rotationSpeed;
    [SerializeField, Tooltip("Axis around which cube rotates.")]  private Vector3 rotationAxis = Vector3.up;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
