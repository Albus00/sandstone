using UnityEngine;

public class RootMotionRelay : MonoBehaviour
{
    private PlayerController _playerController;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    private void Start()
    {
        _playerController = PlayerController.Instance;

        if (_playerController == null)
        {
            Debug.LogError("PlayerController instance is not found.");
        }
    }

    private void OnAnimatorMove()
    {
        if (_animator.applyRootMotion)
        {
            Vector3 delta = _animator.deltaPosition;
            delta.y = 0f; // Prevent upward drift if not intended
            _playerController.MovePlayer(delta);
        }
    }
}
