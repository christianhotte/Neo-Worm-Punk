using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Apparently doesn't work with the new input system :(
public class SpectatorCamera : MonoBehaviour
{
    private Transform target; // The player's transform
    [SerializeField] private float distance = 5.0f; // The distance from the player
    [SerializeField] private float height = 1.0f; // The height from the player
    [SerializeField] private float rotationSpeed = 160.0f; // The rotation speed
    [SerializeField] private float zoomSpeed = 10.0f; // The zoom speed
    [SerializeField] private float minDistance = 2.0f; // The minimum distance from the player
    [SerializeField] private float maxDistance = 10.0f; // The maximum distance from the player
    [SerializeField] private float minHeight = 1.0f; // The minimum height from the player
    [SerializeField] private float maxHeight = 5.0f; // The maximum height from the player
    private float currentDistance; // The current distance from the player
    private float currentHeight; // The current height from the player
    private float currentRotationX; // The current rotation around the player on the X-axis
    private float currentRotationY; // The current rotation around the player on the Y-axis
    private PlayerController demoPlayer;
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;

    void Start()
    {
        currentDistance = distance;
        currentHeight = height;
        currentRotationX = transform.rotation.eulerAngles.y;
        currentRotationY = transform.rotation.eulerAngles.x;
        target = GameObject.Find("XR Origin").transform;
        demoPlayer = GetComponentInParent<PlayerController>();

        // Set the game window to use the first available display by default
        Display.main.SetRenderingResolution(Screen.width, Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Changes priority of the cameras from first person to third person and vice versa.
            if (firstPersonCamera.depth == 1 && thirdPersonCamera.depth == 0)
            {
                firstPersonCamera.depth = 0;
                thirdPersonCamera.depth = 1;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
                //Cursor.lockState = CursorLockMode.Locked;
            }

            // Going from third person to first person.
            else if (firstPersonCamera.depth == 0 && thirdPersonCamera.depth == 1)
            {
                firstPersonCamera.depth = 1;
                thirdPersonCamera.depth = 0;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    // Update is called once per end of frame
    void LateUpdate()
    {
        // Check if there is a target to follow
        if (target == null)
        {
            return;
        }

        // Zoom in/out with scroll wheel
        currentDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        // Rotate around player with mouse input
        currentRotationX += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        currentRotationY -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        currentRotationY = Mathf.Clamp(currentRotationY, -90.0f, 90.0f);

        // Calculate new camera position and rotation
        Vector3 direction = new Vector3(0.0f, 0.0f, -currentDistance);
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0.0f);
        Vector3 position = target.position + rotation * direction + Vector3.up * currentHeight;

        // Set camera position and rotation
        transform.position = position;
        transform.rotation = rotation;
    }

    // Sets the target of what you want the camera to follow
    /*public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }*/
}