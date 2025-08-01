using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Player stats
    public float MovementSpeed = 5f; // Speed of the player movement
    public float RotationSpeed = 100f; // Speed of the player rotation
    public float DashSpeed = 20f; // Speed of the player dash
    public float DashDuration = 0.5f;

    // Player states
    [SerializeField] private bool _stateIsDashing = false; // Whether the player is currently dashing

    // Reference to the InputAction for player movement
    InputAction moveAction;
    InputAction dashAction;

    // Reference to the character controller
    CharacterController controller;

    // Reference to the Camera for player movement direction
    Camera mainCamera;

    // Reference to the player avatar for visual effects
    private GameObject _playerAvatar;
    private Material _avatarMaterial;


    // Dash storage
    private Vector3 _dashDirection;
    private float _dashTimeElapsed = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        moveAction = InputSystem.actions.FindAction("Player/Move");
        dashAction = InputSystem.actions.FindAction("Player/Dash");

        _playerAvatar = transform.Find("Avatar").gameObject;
        _avatarMaterial = _playerAvatar.GetComponent<Renderer>().material;

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
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

        rotatePlayer();
        movePlayer();
    }

    /// <summary>
    /// Calculates the movement direction based on camera orientation.
    /// </summary>
    /// <param name="moveDirection">The normalized movement direction of the player</param>
    /// <returns>The normalized movement direction of the player, from the cameras perspective</returns>
    Vector3 useCameraVectors(Vector3 moveDirection)
    {
        // Get the camera's forward and right vectors
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        // Ignore the vertical component of the camera vectors
        cameraForward.y = 0;
        cameraRight.y = 0;

        // Normalize the vectors to ensure they have a magnitude of 1
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate the final movement direction based on camera orientation
        Vector3 combinedCameraDirection = cameraForward * moveDirection.z + cameraRight * moveDirection.x;
        combinedCameraDirection.Normalize();

        return combinedCameraDirection;
    }

    void rotatePlayer()
    {
        // Get current rotation
        float playerRotationY = transform.rotation.eulerAngles.y;

        // Get camera rotation
        float cameraRotationY = mainCamera.transform.rotation.eulerAngles.y;

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

        // Move the player using the CharacterController
        controller.Move(useCameraVectors(moveDirection) * MovementSpeed * Time.deltaTime);
    }

    void triggerDash(Vector3 moveDirection)
    {
        _stateIsDashing = true;

        // Calculate the dash direction based on input
        _dashDirection = moveDirection;

        // If there's no input, use the camera's forward direction for dashing
        if (_dashDirection == Vector3.zero)
        {
            _dashDirection = new Vector3(0, 0, 1); // Forward direction
        }
    }

    void performDash()
    {
        float t = _dashTimeElapsed / DashDuration;
        float easedT = EasingFunctions.EaseInOutCirc(t);

        // Calculate the dash direction with easing
        controller.Move(useCameraVectors(_dashDirection) * DashSpeed * Time.deltaTime);

        // // Fade player avatar opacity
        // Color avatarColor = _avatarMaterial.color;
        // avatarColor.a = Mathf.Sin(Mathf.PI * (t + 1)) + 1;
        // _avatarMaterial.color = avatarColor;

        // Toggle avatar visibility
        if (t > 0.1f && t < 0.8f)
            _playerAvatar.SetActive(false);
        else
            _playerAvatar.SetActive(true);

        // End the dash after the specified duration
        _dashTimeElapsed += Time.deltaTime;
        if (_dashTimeElapsed >= DashDuration)
        {
            _stateIsDashing = false; // Reset dashing state
            _dashTimeElapsed = 0f; // Reset dash time
            _avatarMaterial.color = new Color(_avatarMaterial.color.r, _avatarMaterial.color.g, _avatarMaterial.color.b, 1f); // Reset opacity
        }
    }
}
