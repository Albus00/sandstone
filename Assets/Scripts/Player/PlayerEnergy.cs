using System;
using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    public static PlayerEnergy Instance { get; private set; }
    public const float MaxEnergy = 100f;
    [SerializeField] private float _energy = MaxEnergy;
    [SerializeField] private float _energyRegenRate = 10f;

    // Public events
    public event Action<float> OnEnergyChanged;

    // Public read-only access
    public float Energy => _energy;

    private void Awake()
    {
        // Make sure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Optional: destroy duplicates
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        // Regenerate energy over time
        SetEnergy(_energy + _energyRegenRate * Time.deltaTime);
    }

    public void SetEnergy(float amount)
    {
        _energy = Mathf.Clamp(amount, 0, MaxEnergy);
        OnEnergyChanged.Invoke(_energy);
    }
    public void UseEnergy(float amount)
    {
        SetEnergy(_energy - amount);
    }
}
