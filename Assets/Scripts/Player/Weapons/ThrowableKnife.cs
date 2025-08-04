using UnityEngine;

public class ThrowableKnife : MonoBehaviour
{
    [SerializeField] private float throwForce = 20f;
    [SerializeField] private float lifeAfterImpact = 2f;
    [SerializeField] private GameObject impactEffect;

    private Rigidbody _rb;
    private bool _hasImpacted = false;

    public void Throw(Vector3 direction)
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.AddForce(direction * throwForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("No Rigidbody on Knife prefab.");
        }
    }

    // void Update()
    // {
    //     if (_hasImpacted) return;

    //     RaycastHit hit;
    //     if (Physics.Raycast(transform.position, transform.forward, out hit, 0.1f))
    //     {
    //         if (_rb) _rb.isKinematic = true;
    //         GetComponent<Collider>().enabled = false;
    //         _hasImpacted = true;
    //     }
    // }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasImpacted) return;

        _hasImpacted = true;

        // Instantiate(impactEffect, transform.position, Quaternion.identity);        }

        if (_rb) _rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, lifeAfterImpact);
    }
}

