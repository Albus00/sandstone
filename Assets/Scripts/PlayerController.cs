using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Player stats
    public float movementSpeed = 5f; // Speed of the player movement

    // Reference to the InputAction for player movement
    InputAction moveAction;

    // Reference to the Rigidbody component for physics-based movement
    Rigidbody rb;

    // Reference to the Camera for player movement direction
    Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputActionAsset.FindAction("Move");
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        movePlayer();
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

        // Get the camera's forward and right vectors        
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0; // Ignore vertical component
        cameraRight.y = 0; // Ignore vertical component
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate the final movement direction based on camera orientation
        Vector3 finalDirection = cameraForward * moveDirection.z + cameraRight * moveDirection.x;
        finalDirection.Normalize();

        // Apply the movement to the Rigidbody
        rb.MovePosition(rb.position + finalDirection * Time.deltaTime * movementSpeed); //
    }


}
