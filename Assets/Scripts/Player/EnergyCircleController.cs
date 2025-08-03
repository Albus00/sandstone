using UnityEngine;

public class EnergyCircleController : MonoBehaviour
{
    [Range(0f, 1f)]
    private float _maxFillAmount;
    private Material _groundCircleMaterial;

    void Start()
    {
        PlayerEnergy.Instance.OnEnergyChanged += SetFillAmount;
        _maxFillAmount = PlayerEnergy.MaxEnergy;
        _groundCircleMaterial = GetComponent<Renderer>().material;
    }

    public void SetFillAmount(float amount)
    {
        float fillAmount = Mathf.Clamp01(amount / _maxFillAmount);
        _groundCircleMaterial.SetFloat("_FillAmount", fillAmount);
    }
}
