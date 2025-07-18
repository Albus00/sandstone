using UnityEngine;
using UnityEngine.InputSystem;

public class CombatPrototype : MonoBehaviour
{
    // Player stats
    public float movementSpeed = 5f; // Speed of the player movement

    // Reference to the InputAction for player movement
    InputAction moveAction;

    // Reference to the character controller
    CharacterController controller;

    // Reference to the Camera for player movement direction
    Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        moveAction = InputSystem.actions.FindAction("Player/Move");
        if (moveAction == null)
        {
            Debug.LogError("Move action not found in Input System. Please check your Input Actions setup.");
        }

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Hide the cursor
    }

    // Update is called once per frame
    void Update()
    {
        movePlayer();
    }

    void movePlayer()
    {
        // Get controller input for movement
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        // Move the player using the CharacterController
        controller.Move(moveDirection * movementSpeed * Time.deltaTime);
    }
}
