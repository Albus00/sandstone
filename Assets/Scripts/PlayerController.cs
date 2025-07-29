using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Player stats
    public float MovementSpeed = 5f; // Speed of the player movement
    public float RotationSpeed = 100f; // Speed of the player rotation
    public float DashSpeed = 20f; // Speed of the player dash

    // Player states
    [SerializeField] private bool _stateIsDashing = false; // Whether the player is currently dashing

    // Reference to the InputAction for player movement
    InputAction moveAction;
    InputAction dashAction;

    // Reference to the character controller
    CharacterController controller;

    // Reference to the Camera for player movement direction
    Camera mainCamera;
    Vector3 cameraForward;
    Vector3 cameraRight;

    // Dash storage
    private Vector3 _dashDirection;
    private float _dashDuration = 0.2f;
    private float _dashTimeElapsed = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        moveAction = InputSystem.actions.FindAction("Player/Move");
        dashAction = InputSystem.actions.FindAction("Player/Dash");

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Hide the cursor
    }

    // Update is called once per frame
    void Update()
    {
        // Check player state
        if (_stateIsDashing)
        {
            performDash();
            return;
        }

        useCameraVectors();
        rotatePlayer();
        movePlayer();
    }

    void useCameraVectors()
    {
        // TODO: return 'finalDirection'
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

        // float rotationAmount = cameraRotationY - playerRotationY;
        // if (rotationAmount > 180)
        // {
        //     rotationAmount -= 360; // Adjust for wrap-around
        // }
        // Debug.Log($"Rotation Amount: {rotationAmount}");

        // Rotate the player towards the camera's direction
        Quaternion targetRotation = Quaternion.Euler(0, cameraRotationY, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
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

        // Check if the player wants to dash
        if (dashAction.triggered)
        {
            triggerDash(moveDirection);
            return; // Skip normal movement if dashing
        }


        // Calculate the final movement direction based on camera orientation
        Vector3 finalDirection = cameraForward * moveDirection.z + cameraRight * moveDirection.x;
        finalDirection.Normalize();

        // Move the player using the CharacterController
        controller.Move(finalDirection * MovementSpeed * Time.deltaTime);
    }

    void triggerDash(Vector3 moveDirection)
    {
        _stateIsDashing = true;

        // Calculate the dash direction based on input
        _dashDirection = moveDirection;

        // If there's no input, use the camera's forward direction for dashing
        if (_dashDirection == Vector3.zero)
        {
            _dashDirection = cameraForward;
        }
    }

    void performDash()
    {
        float t = _dashTimeElapsed / _dashDuration;
        float easedT = EasingFunctions.EaseInOutCirc(t);

        controller.Move(_dashDirection * DashSpeed * Time.deltaTime);

        _dashTimeElapsed += Time.deltaTime;
        if (_dashTimeElapsed >= _dashDuration)
        {
            _stateIsDashing = false; // Reset dashing state
            _dashTimeElapsed = 0f; // Reset dash time
        }
    }
}
