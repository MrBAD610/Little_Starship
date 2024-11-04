using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Look Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2.0f;

    private Rigidbody rb;
    private PlayerInputHandler inputHandler;
    private float horizontalInput;
    private float verticalInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputHandler = PlayerInputHandler.Instance;
    }

    // Update is called once per frame
    private void Update()
    {
        horizontalInput = inputHandler.MoveInput.x;
        verticalInput = inputHandler.MoveInput.y;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float speed = moveSpeed;
        rb.velocity = new Vector3(horizontalInput * speed, 0f, verticalInput * speed);
    }
}
