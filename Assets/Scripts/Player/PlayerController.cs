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
    [SerializeField] private float _smoothRotationThreshold = 20f;
    [SerializeField] private float _movementInterpolationTime = 0.1f; // Time in milliseconds to smooth movement

    [Header("Dash Settings")]
    [SerializeField] private float _dashDuration = 0.5f;
    [SerializeField] private float _dashEnergyCost = 20f;

    [Header("Runtime")]
    [SerializeField] private bool _isDashing = false;
    private Vector3 _moveDirection;
    private float _dashTimeElapsed;
    private Vector3 _dashDirection;
    private float _currentSpeed;

    // External references
    private PlayerEnergy _playerEnergy;
    private CharacterController _controller;
    private Animator _animator;
    private Camera _mainCamera;

    // Input actions
    private InputAction _moveAction;
    private InputAction _dashAction;
    private System.Action<InputAction.CallbackContext> _onDash;

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
        _animator = GetComponentInChildren<Animator>();
        _mainCamera = Camera.main;

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

        _moveDirection = useCameraVectors(getMoveDirection());
        animateMovement();
    }

    private void OnEnable()
    {
        _onDash = ctx => triggerDash();

        _dashAction.performed += _onDash;
        _moveAction.Enable();
        _dashAction.Enable();
    }

    private void OnDisable()
    {
        _dashAction.performed -= _onDash;

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

        return combinedCameraDirection;
    }

    Vector3 getMoveDirection()
    {
        // Get controller input for movement
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        return moveDirection;
    }

    void animateMovement()
    {
        if (_moveDirection != Vector3.zero)
        {
            rotatePlayer();
        }

        // Smoothly interpolate movement magnitude for acceleration/deceleration
        _currentSpeed = Mathf.Lerp(_currentSpeed, _moveDirection.magnitude, Time.deltaTime / _movementInterpolationTime);
        _animator.SetFloat("MoveSpeed", _currentSpeed);
    }


    void rotatePlayer()
    {
        const float FAST_ROTATION_MULTIPLIER = 4f;

        float playerRotationY = transform.rotation.eulerAngles.y;
        float moveDirectionY = Mathf.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, moveDirectionY, 0);

        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(playerRotationY, moveDirectionY));
        float rotationSpeed = _rotationSpeed * Time.deltaTime * (angleDifference < _smoothRotationThreshold ? 1f : FAST_ROTATION_MULTIPLIER);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
    }

    /// <summary>
    /// Moves the player based on the delta position calculated by the Animator.
    /// This method is called by the RootMotionRelay script to apply root motion.
    /// </summary>
    /// <param name="delta">Based on the avatars last position using root motion</param>
    public void MovePlayer(Vector3 delta)
    {
        // Move the player using the CharacterController
        _controller.Move(delta);
    }

    void triggerDash()
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
        _dashDirection = _moveDirection == Vector3.zero ? useCameraVectors(new Vector3(0, 0, 1)) : _moveDirection;
        Debug.Log($"Dashing in direction: {_dashDirection}");
    }

    void performDash()
    {
        float t = _dashTimeElapsed / _dashDuration;
        float easedT = EasingFunctions.EaseOutCirc(t);

        // Calculate the dash direction with easing
        _controller.Move(_dashDirection * _dashSpeed * Time.deltaTime);

        // End the dash after the specified duration
        _dashTimeElapsed += Time.deltaTime;
        if (_dashTimeElapsed >= _dashDuration)
        {
            _isDashing = false; // Reset dashing state
            _dashTimeElapsed = 0f; // Reset dash time
            // _avatarMaterial.color = new Color(_avatarMaterial.color.r, _avatarMaterial.color.g, _avatarMaterial.color.b, 1f); // Reset opacity
        }
    }
}
