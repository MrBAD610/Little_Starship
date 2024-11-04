using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Camera Settings")]
    [SerializeField] private float cameraOffset = 5.0f;

    private Rigidbody rb;
    private PlayerInputHandler inputHandler;
    private Camera mainCamera;
    private float horizontalInput;
    private float verticalInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
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
        HandleRotation();
    }

    void HandleMovement()
    {
        float speed = moveSpeed;
        Vector3 offset = new Vector3(0f, cameraOffset, 0f);
        //rb.velocity = new Vector3(horizontalInput * speed, 0f, verticalInput * speed);
        rb.AddForce(new Vector3(horizontalInput * speed, 0f, verticalInput * speed));
        mainCamera.transform.localPosition = transform.position + offset;
    }

    void HandleRotation()
    {
        float mouseXPosition = inputHandler.LookInput.x;
        float mouseYPosition = inputHandler.LookInput.y;
        Vector3 mousePos = (Vector3)mainCamera.ScreenToWorldPoint(new Vector3(mouseXPosition, mouseYPosition, cameraOffset));

        Vector3 directionToFace = mousePos - rb.position;
        float angle = (180 / Mathf.PI) * Mathf.Atan2(directionToFace.z, directionToFace.x) - 90;
        rb.MoveRotation(Quaternion.Euler(0f, -angle, 0f));

        Debug.DrawLine(transform.position, mousePos, Color.green, Time.deltaTime);        
        
        //float angRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        //float angDeg = (180 / Mathf.PI) * angRad; // Offset by 90 Degrees
        //rb.MoveRotation(Quaternion.Euler(0f, angDeg, 0f));
    }
}
