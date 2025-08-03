using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public static PlayerCombat Instance { get; private set; }

    [Header("Weapon Prefabs")]
    [SerializeField] private GameObject _knifePrefab;

    [Header("Combat Settings")]
    [SerializeField] private float _throwSpeed = 10f;
    [SerializeField] private float _throwCost = 10f;

    // Input actions
    private InputAction _throwAction;

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
    }

    void OnEnable()
    {
        _throwAction.Enable();
    }

    void OnDisable()
    {
        _throwAction.Disable();
    }

    void Update()
    {
        if (_throwAction.triggered)
        {
            ThrowKnife();
        }
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
        GameObject knife = Instantiate(_knifePrefab, transform.position + transform.forward, transform.rotation);
        ThrowableKnife throwableKnife = knife.GetComponent<ThrowableKnife>();
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
}
