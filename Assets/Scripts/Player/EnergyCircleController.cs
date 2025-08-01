using UnityEngine;

public class EnergyCircleController : MonoBehaviour
{
    public Material groundCircleMaterial;
    public Transform playerTransform;

    void Update()
    {
        if (groundCircleMaterial != null && playerTransform != null)
        {
            groundCircleMaterial.SetVector("_PlayerPos", playerTransform.position);
        }
    }
}
