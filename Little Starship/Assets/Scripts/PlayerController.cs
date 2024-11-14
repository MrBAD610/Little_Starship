using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Position Settings")]
    [SerializeField] private float yPosition = 0f;
    //[SerializeField] private float yCorrectionSpeed = 1f;

    [Header("Camera Settings")]
    [SerializeField] private float cameraOffset = 5.0f;

    private Rigidbody rb;

    private PlayerInputHandler inputHandler;

    private Camera mainCamera;

    private Transform playerTransform;

    private float horizontalInput;
    private float verticalInput;
    private float mouseXPosition;
    private float mouseYPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        inputHandler = PlayerInputHandler.Instance;
        playerTransform = transform;
    }

    // Update is called once per frame
    private void Update()
    {
        horizontalInput = inputHandler.MoveInput.x;
        verticalInput = inputHandler.MoveInput.y;
        mouseXPosition = inputHandler.LookInput.x;
        mouseYPosition = inputHandler.LookInput.y;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
        HandleYPosition();
    }

    void HandleMovement()
    {
        float speed = moveSpeed;
        Vector3 offset = new Vector3(0f, cameraOffset, 0f);
        //rb.velocity = new Vector3(horizontalInput * speed, 0f, verticalInput * speed);
        rb.AddForce(new Vector3(horizontalInput * speed, 0f, verticalInput * speed));
        mainCamera.transform.localPosition = playerTransform.position + offset;
    }

    void HandleRotation()
    {
        Vector3 mousePos = (Vector3)mainCamera.ScreenToWorldPoint(new Vector3(mouseXPosition, mouseYPosition, cameraOffset));

        Vector3 directionToFace = mousePos - rb.position;
        float angle = (180 / Mathf.PI) * Mathf.Atan2(directionToFace.z, directionToFace.x) - 90;
        rb.MoveRotation(Quaternion.Euler(0f, -angle, 0f));

        Debug.DrawLine(playerTransform.position, mousePos, Color.green, Time.deltaTime);        
    }

    void HandleYPosition()
    {
        Vector3 desiredPosition = new Vector3(playerTransform.position.x, yPosition, playerTransform.position.z);

        if ((desiredPosition - transform.position) != Vector3.zero)
        {
            playerTransform.position = desiredPosition;
        }

        //rb.velocity = desiredPosition - rb.position;
        //rb.AddForce((desiredPosition - rb.position) * yCorrectionSpeed, ForceMode.Force);
    }
}
