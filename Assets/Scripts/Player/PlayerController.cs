using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _dashSpeed = 20f;

    [Header("Dash Settings")]
    [SerializeField] private float _dashDuration = 0.5f;
    [SerializeField] private float _dashEnergyCost = 20f;

    [Header("Runtime")]
    [SerializeField] private bool _isDashing = false;
    private float _dashTimeElapsed;
    private Vector3 _dashDirection;

    // External references
    private InputAction _moveAction;
    private InputAction _dashAction;
    private PlayerEnergy _playerEnergy;
    private CharacterController _controller;
    private Camera _mainCamera;
    private GameObject _playerAvatar;
    private Material _avatarMaterial;

    private void Awake()
    {
        // Make sure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Optional: destroy duplicates
            return;
        }

        Instance = this;

        _moveAction = InputSystem.actions.FindAction("Player/Move");
        _dashAction = InputSystem.actions.FindAction("Player/Dash");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;

        _playerAvatar = transform.Find("Avatar").gameObject;
        _avatarMaterial = _playerAvatar.GetComponent<Renderer>().material;

        _playerEnergy = PlayerEnergy.Instance;

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
    }

    // Update is called once per frame
    void Update()
    {
        // Check player state
        if (_isDashing)
        {
            performDash();
            return;
        }

        rotatePlayer();
        movePlayer();
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _dashAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _dashAction.Disable();
    }

    /// <summary>
    /// Calculates the movement direction based on camera orientation.
    /// </summary>
    /// <param name="moveDirection">The normalized movement direction of the player</param>
    /// <returns>The normalized movement direction of the player, from the cameras perspective</returns>
    Vector3 useCameraVectors(Vector3 moveDirection)
    {
        // Get the camera's forward and right vectors
        Vector3 cameraForward = _mainCamera.transform.forward;
        Vector3 cameraRight = _mainCamera.transform.right;

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
        float cameraRotationY = _mainCamera.transform.rotation.eulerAngles.y;

        // Rotate the player towards the camera's direction
        Quaternion targetRotation = Quaternion.Euler(0, cameraRotationY, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    void movePlayer()
    {
        // Get controller input for movement
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        // Check if the player wants to dash
        if (_dashAction.triggered)
        {
            triggerDash(moveDirection);
            return; // Skip normal movement if dashing
        }

        // Move the player using the CharacterController
        _controller.Move(useCameraVectors(moveDirection) * _movementSpeed * Time.deltaTime);
    }

    void triggerDash(Vector3 moveDirection)
    {
        // Check if the player has enough energy to dash
        if (_playerEnergy.Energy < _dashEnergyCost)
        {
            Debug.Log("Not enough energy to dash!");
            return;
        }

        _isDashing = true;
        _playerEnergy.UseEnergy(_dashEnergyCost);

        // Use the move direction or forward if no input is given
        _dashDirection = moveDirection == Vector3.zero ? new Vector3(0, 0, 1) : moveDirection;
    }

    void performDash()
    {
        float t = _dashTimeElapsed / _dashDuration;
        float easedT = EasingFunctions.EaseInOutCirc(t);

        // Calculate the dash direction with easing
        _controller.Move(useCameraVectors(_dashDirection) * _dashSpeed * Time.deltaTime);

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
        if (_dashTimeElapsed >= _dashDuration)
        {
            _isDashing = false; // Reset dashing state
            _dashTimeElapsed = 0f; // Reset dash time
            _avatarMaterial.color = new Color(_avatarMaterial.color.r, _avatarMaterial.color.g, _avatarMaterial.color.b, 1f); // Reset opacity
        }
    }
}
