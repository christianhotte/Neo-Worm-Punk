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
    [SerializeField] private float fovChangeSpeed = 1f;
    [SerializeField] private float minDistance = 2.0f; // The minimum distance from the player
    [SerializeField] private float maxDistance = 10.0f; // The maximum distance from the player
    [SerializeField] private float minHeight = 1.0f; // The minimum height from the player
    [SerializeField] private float maxHeight = 5.0f; // The maximum height from the player
    [SerializeField] private float droneSpeed = 12f;
    [SerializeField] private float droneLookSpeed = 5f;
    [SerializeField] float minDroneLookAngle = -60f;
    [SerializeField] float maxDroneLookAngle = 60f;
    private float yaw = 0f;
    private float pitch = 0f;
    private float currentDistance; // The current distance from the player
    private float currentHeight; // The current height from the player
    private float currentRotationX; // The current rotation around the player on the X-axis
    private float currentRotationY; // The current rotation around the player on the Y-axis
    private PlayerController demoPlayer;
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public Camera droneCamera;

    private float originFov;

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
        originFov = firstPersonCamera.fieldOfView;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Changes priority of the cameras from first person to third person and to drone camera.
            if (firstPersonCamera.depth == 2 && thirdPersonCamera.depth == 1 && droneCamera.depth == 0)
            {
                firstPersonCamera.depth = 1;
                thirdPersonCamera.depth = 2;
                droneCamera.depth = -1;
                //Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            // Going from drone camera to first person.
            else if (firstPersonCamera.depth == 2 && thirdPersonCamera.depth == 1 && droneCamera.depth == 3)
            {
                firstPersonCamera.depth = 2;
                thirdPersonCamera.depth = 1;
                droneCamera.depth = 0;
            }

            // Going from third person to drone camera.
            else if (firstPersonCamera.depth == 1 && thirdPersonCamera.depth == 2 && droneCamera.depth == -1)
            {
                firstPersonCamera.depth = 2;
                thirdPersonCamera.depth = 1;
                droneCamera.depth = 3;
                droneCamera.transform.parent = null;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
                //Cursor.lockState = CursorLockMode.Locked;
            }

            // Going from third person to first person (Shouldn't be called)
            else if (firstPersonCamera.depth == 1 && thirdPersonCamera.depth == 2 && droneCamera.depth == -1)
            {
                firstPersonCamera.depth = 2;
                thirdPersonCamera.depth = 1;
                droneCamera.depth = -2;
            }
        }

        if (Input.GetMouseButtonDown(2))
        {
            if (firstPersonCamera.depth < thirdPersonCamera.depth)
            {

            }
            else
            {
                firstPersonCamera.fieldOfView = originFov;
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

        // Zoom in/out with scroll wheel while third person is active
        float scrollAmt = Input.GetAxis("Mouse ScrollWheel");
        if (firstPersonCamera.depth < thirdPersonCamera.depth && droneCamera.depth < 1)
        {
            currentDistance -= scrollAmt * zoomSpeed;
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

            droneCamera.transform.position = thirdPersonCamera.transform.position;
            droneCamera.transform.rotation = thirdPersonCamera.transform.rotation;
        }

        // If the drone camera is active.
        else if (droneCamera.depth == 3)
        {
            // Scroll wheel increases/decreases the drone's move speed.
            droneSpeed += scrollAmt * droneSpeed;

            // Move the drone spectator camera using WASD and E/Q keys to go up/down.
            float x = Input.GetAxis("Horizontal") * droneSpeed * Time.deltaTime;
            float z = Input.GetAxis("Vertical") * droneSpeed * Time.deltaTime;
            float y = 0f;

            // Moves the drone camera up when E is pressed.
            if (Input.GetKey(KeyCode.E))
            {
                y = droneSpeed * Time.deltaTime;
            }

            // Moves the drone camera down when Q is pressed.
            else if (Input.GetKey(KeyCode.Q))
            {
                y = -droneSpeed * Time.deltaTime;
            }

            // Moves the drone camera round.
            droneCamera.transform.Translate(x, y, z, Space.Self);

            // Rotate the camera based on mouse movement
            yaw += Input.GetAxis("Mouse X") * droneLookSpeed;
            pitch -= Input.GetAxis("Mouse Y") * droneLookSpeed;
            pitch = Mathf.Clamp(pitch, minDroneLookAngle, maxDroneLookAngle);

            droneCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        // If the first person spectator camera is active.
        else
        {
            float currentFov = firstPersonCamera.fieldOfView;
            currentFov -= scrollAmt * fovChangeSpeed;
            firstPersonCamera.fieldOfView = currentFov;
        }
    }

    // Sets the target of what you want the camera to follow
    /*public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }*/
}