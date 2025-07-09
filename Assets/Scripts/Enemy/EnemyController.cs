using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _agent;
    [SerializeField] private float movementSpeed = 5f;
    // [SerializeField] private float rotationSpeed = 5f;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = movementSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyAnimation();
    }

    private void ApplyAnimation()
    {
        // Animate the enemy based on its movement
        _animator.SetFloat("Speed", _agent.velocity.magnitude);
    }
}
