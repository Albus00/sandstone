using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public static PlayerCombat Instance { get; private set; }

    [Header("Weapon Prefabs")]
    [SerializeField] private GameObject _knifePrefab;

    [Header("Combat Settings")]
    [SerializeField] private float _throwCost = 10f;

    // Runtime variables
    private GameObject _currentKnife;

    // External references
    private CharacterController _controller;

    // Input actions
    private InputAction _throwAction;
    private System.Action<InputAction.CallbackContext> _onThrow;
    private InputAction _teleportAction;
    private System.Action<InputAction.CallbackContext> _onTeleport;

    private void Awake()
    {
        // Make sure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Optional: destroy duplicates
            return;
        }

        Instance = this;

        _throwAction = InputSystem.actions.FindAction("Player/Throw");
        _teleportAction = InputSystem.actions.FindAction("Player/Teleport");
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null)
        {
            Debug.LogError("CharacterController component is missing on PlayerCombat.");
        }

        if (_knifePrefab == null)
        {
            Debug.LogError("Knife prefab is not assigned in the inspector.");
        }
    }

    private void OnEnable()
    {
        _onThrow = ctx => ThrowKnife();
        _onTeleport = ctx => TeleportToKnife();

        _throwAction.performed += _onThrow;
        _teleportAction.performed += _onTeleport;

        _throwAction.Enable();
        _teleportAction.Enable();
    }

    private void OnDisable()
    {
        _throwAction.performed -= _onThrow;
        _teleportAction.performed -= _onTeleport;

        _throwAction.Disable();
        _teleportAction.Disable();
    }

    private void ThrowKnife()
    {
        // Check if the player has enough energy to throw
        if (PlayerEnergy.Instance.Energy < _throwCost)
        {
            Debug.Log("Not enough energy to throw the knife!");
            return;
        }

        // Use energy for throwing the knife
        PlayerEnergy.Instance.UseEnergy(_throwCost);

        // Instantiate and throw the knife
        _currentKnife = Instantiate(_knifePrefab, transform.position + transform.forward, transform.rotation);
        ThrowableKnife throwableKnife = _currentKnife.GetComponent<ThrowableKnife>();
        if (throwableKnife != null)
        {
            Vector3 throwDirection = transform.forward;
            throwableKnife.Throw(throwDirection);
        }
        else
        {
            Debug.LogWarning("ThrowableKnife component not found on knife prefab.");
        }
    }

    private void TeleportToKnife()
    {
        if (_currentKnife != null)
        {
            // Teleport the player to the knife's position
            _controller.enabled = false; // Disable controller to prevent physics issues
            transform.position = _currentKnife.transform.position;
            _controller.enabled = true; // Re-enable controller

            _currentKnife.SetActive(false); // Optionally deactivate the knife after teleporting
            _currentKnife = null; // Clear the reference
        }
        else
        {
            Debug.Log("No knife to teleport to.");
        }
    }
}
