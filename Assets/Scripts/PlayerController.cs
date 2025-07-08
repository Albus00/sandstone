using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Player stats
    public float movementSpeed = 5f; // Speed of the player movement
    public float rotationSpeed = 100f; // Speed of the player rotation

    // Reference to the InputAction for player movement
    InputAction moveAction;

    // Reference to the character controller
    CharacterController controller;

    // Reference to the Camera for player movement direction
    Camera mainCamera;
    Vector3 cameraForward;
    Vector3 cameraRight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        moveAction = InputSystem.actions.FindAction("Player/Move");
        if (moveAction == null)
        {
            Debug.LogError("Move action not found in Input System. Please check your Input Actions setup.");
        }

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Hide the cursor
    }

    // Update is called once per frame
    void Update()
    {
        getCameraVectors();
        rotatePlayer();
        movePlayer();
    }

    void getCameraVectors()
    {
        // Get the camera's forward and right vectors
        cameraForward = mainCamera.transform.forward;
        cameraRight = mainCamera.transform.right;

        // Ignore the vertical component of the camera vectors
        cameraForward.y = 0;
        cameraRight.y = 0;

        // Normalize the vectors to ensure they have a magnitude of 1
        cameraForward.Normalize();
        cameraRight.Normalize();
    }

    void rotatePlayer()
    {
        // Get current rotation
        float playerRotationY = transform.rotation.eulerAngles.y;

        // Get camera rotation
        float cameraRotationY = mainCamera.transform.rotation.eulerAngles.y;

        float rotationAmount = cameraRotationY - playerRotationY;
        if (rotationAmount > 180)
        {
            rotationAmount -= 360; // Adjust for wrap-around
        }
        Debug.Log($"Rotation Amount: {rotationAmount}");

        // Rotate the player towards the camera's direction
        Quaternion targetRotation = Quaternion.Euler(0, cameraRotationY, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void movePlayer()
    {
        // Get controller input for movement
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        // Calculate the final movement direction based on camera orientation
        Vector3 finalDirection = cameraForward * moveDirection.z + cameraRight * moveDirection.x;
        finalDirection.Normalize();

        // Move the player using the CharacterController
        controller.Move(finalDirection * movementSpeed * Time.deltaTime);
    }
}
