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
    private Vector3 _moveDirection;
    private float _dashTimeElapsed;
    private Vector3 _dashDirection;

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
        rotatePlayer();
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
        combinedCameraDirection.Normalize();

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
        // Ease in the movement magnitude for smoother acceleration
        float easedMagnitude = Mathf.SmoothStep(0, 1, _moveDirection.magnitude);
        _animator.SetFloat("MoveSpeed", easedMagnitude);
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
        _dashDirection = _moveDirection == Vector3.zero ? new Vector3(0, 0, 1) : _moveDirection;
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
        // if (t > 0.1f && t < 0.8f)
        //     _playerAvatar.SetActive(false);
        // else
        //     _playerAvatar.SetActive(true);

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
