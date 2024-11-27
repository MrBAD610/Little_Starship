using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Position Settings")]
    [SerializeField] private float yPosition = 0f;

    [Header("Tractor Beam Settings")]
    [SerializeField] private float attractSpeed = 1.0f;

    [Header("Colonist Slot Cycling Settings")]
    [SerializeField] private float cycleSlotCooldown = 0.2f;

    [Header("Medical Emergency Scroll Settings")]
    [SerializeField] private float scrollCooldown = 0.2f;

    [Header("Camera Settings")]
    [SerializeField] private float cameraOffset = 5.0f;

    private Rigidbody playerRb;

    private PlayerInputHandler inputHandler;
    private PlayerInventory playerInventory;
    private PlayerHealth playerHealth;

    public Camera mainCamera;

    private Transform playerTransform;

    private float horizontalInput;
    private float verticalInput;
    public bool grabInput;
    private float cycleInput;
    private float scrollInput;
    private bool selectInput;
    private bool ejectInput;

    private float timeOfLastCycle = 0f;
    private float timeOfLastScroll = 0f;

    private bool hasEjected = false; // Prevent multiple ejections on a single press

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

    // Awake is called when the object is loaded before start
    private void Awake()
    {
        playerTransform = transform;
        playerRb = GetComponent<Rigidbody>();
        playerInventory = GetComponent<PlayerInventory>();
        playerHealth = GetComponent<PlayerHealth>();
    }
    
    //Start is called before the first frame update
    private void Start()
    {
        inputHandler = PlayerInputHandler.Instance;
        //mainCamera = inputHandler.Camera;
    }

    // Update is called once per frame
    private void Update()
    {
        horizontalInput = inputHandler.MoveInput.x;
        verticalInput = inputHandler.MoveInput.y;
        curScreenPos = inputHandler.LookInput;
        grabInput = inputHandler.GrabInput;
        cycleInput = inputHandler.CycleInput;
        scrollInput = inputHandler.ScrollInput;
        selectInput = inputHandler.SelectInput;
        ejectInput = inputHandler.EjectInput;
    }

    private void FixedUpdate()
    {
        if (playerHealth.isAlive)
        {
            HandleMovement();
            HandleRotation();
            HandleYPosition();

            if (CanGrab)
            {
                StartCoroutine(Drag());
            }

            if (Time.time - timeOfLastCycle > cycleSlotCooldown)
            {
                if (cycleInput > 0f) // Cycle to next colonist
                {
                    playerInventory.SelectNextColonist();
                    timeOfLastCycle = Time.time;
                }
                else if (cycleInput < 0f) // Cycle to previous colonist
                {
                    playerInventory.SelectPreviousColonist();
                    timeOfLastCycle = Time.time;
                }
            }

            if (Time.time - timeOfLastScroll > scrollCooldown)
            {
                if (scrollInput > 0f) // scroll up to next medical emergency/body region
                {
                    timeOfLastCycle = Time.time;
                    Debug.Log($"Scrolled up at {timeOfLastCycle}");
                }
                else if (scrollInput < 0f) // scroll down to next medical emergency/body region
                {
                    timeOfLastCycle = Time.time;
                    Debug.Log($"Scrolled down at {timeOfLastCycle}");
                }
            }

            if (selectInput)
            {
                Debug.Log("Has hit select button");
            }

            if (ejectInput && !hasEjected)
            {
                Eject();
                hasEjected = true; // Prevent multiple colonists from being ejected
            }
            if (!ejectInput)
            {
                hasEjected = false; // Allow another colonist to be ejected
            }
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
    }

    void Eject()
    {
        if (playerInventory.slotList.Count > 0)
        {
            //var colonistToEject = playerInventory.storedColonists[0]; // Drop the first colonist
            //playerInventory.EjectColonist(colonistToEject);

            playerInventory.EjectColonist();
        }
        else
        {
            Debug.Log("No colonists to eject");
        }
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
