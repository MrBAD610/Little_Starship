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

    [Header("Tractor Beam Settings")]
    [SerializeField] private float attractSpeed = 1.0f;

    [Header("Camera Settings")]
    [SerializeField] private float cameraOffset = 5.0f;

    private Rigidbody playerRb;

    private PlayerInputHandler inputHandler;

    private Camera mainCamera;

    private Transform playerTransform;
    //private Transform beamTransform;

    private float horizontalInput;
    private float verticalInput;
    public bool grabInput;
    //public bool canGrab;

    public bool CanGrab
    {
        get
        {
            Ray ray = mainCamera.ScreenPointToRay(curScreenPos);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.gameObject.TryGetComponent<Rigidbody>(out Rigidbody hitRb))
            {
                return hit.transform.gameObject.layer == LayerMask.NameToLayer("Grabbable");
            }
            return false;
        }
    }

    public Vector3 curScreenPos;
    public Vector3 MousePos
    {
        get
        {
            float z = mainCamera.WorldToScreenPoint(playerTransform.position).z;
            return mainCamera.ScreenToWorldPoint(new Vector3(curScreenPos.x, curScreenPos.y, z));
        }
    }

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        inputHandler = PlayerInputHandler.Instance;
        mainCamera = inputHandler.Camera;
        playerTransform = transform;
    }

    // Update is called once per frame
    private void Update()
    {
        horizontalInput = inputHandler.MoveInput.x;
        verticalInput = inputHandler.MoveInput.y;
        curScreenPos = inputHandler.LookInput;
        grabInput = inputHandler.GrabInput;
        //canGrab = inputHandler.CanGrab;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
        HandleYPosition();

        if (CanGrab)
        {
            StartCoroutine(Drag());
        }
    }

    void HandleMovement()
    {
        float speed = moveSpeed;
        Vector3 camOffset = new Vector3(0f, cameraOffset, 0f);
        playerRb.velocity = new Vector3(horizontalInput * speed, 0f, verticalInput * speed);
        //playerRb.AddForce(new Vector3(horizontalInput * speed, 0f, verticalInput * speed));
        mainCamera.transform.localPosition = playerTransform.position + camOffset;
    }

    void HandleRotation()
    {
        Vector3 directionToFace = MousePos - playerTransform.position;
        float angle = (180 / Mathf.PI) * Mathf.Atan2(directionToFace.z, directionToFace.x) - 90;
        playerRb.MoveRotation(Quaternion.Euler(0f, -angle, 0f));

        Debug.DrawLine(playerTransform.position, MousePos, Color.green, Time.deltaTime);        
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

    private IEnumerator Drag()
    {
        if (grabInput)
        {
            Ray ray = mainCamera.ScreenPointToRay(curScreenPos);
            Physics.Raycast(ray, out RaycastHit hit);
            Transform hitTransform = hit.transform;
            Rigidbody hitRb = hitTransform.gameObject.GetComponent<Rigidbody>();
            while (grabInput)
            {
                hitRb.velocity = (MousePos - hitTransform.position) * attractSpeed;
                //hitRb.AddForce((MousePos - hitTransform.position) * attractSpeed);
                yield return null;
            }
        }
    }
}
