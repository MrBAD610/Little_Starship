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
    private EmergencyUIHandler emergencyUIHandler;

    public Camera mainCamera;

    private Transform playerTransform;

    private float horizontalInput;
    private float verticalInput;
    public bool grabInput;
    private float cycleInput;
    private float scrollInput;
    private bool healInput;
    private bool ejectInput;
    private bool quitInput;

    private float timeOfLastCycle = 0f;
    private float timeOfLastScroll = 0f;

    private bool hasHealed = false; // Prevent multiple selections on a single press
    private bool hasEjected = false;  // Prevent multiple ejections on a single press

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
        emergencyUIHandler = GetComponent<EmergencyUIHandler>();
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
        healInput = inputHandler.HealInput;
        ejectInput = inputHandler.EjectInput;
        quitInput = inputHandler.QuitInput;
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
                    //emergencyUIHandler.Scroll(-1);
                    timeOfLastScroll = Time.time;
                    Debug.Log($"Scrolled up at {timeOfLastCycle}");
                }
                else if (scrollInput < 0f) // scroll down to next medical emergency/body region
                {
                    //emergencyUIHandler.Scroll(1);
                    timeOfLastScroll = Time.time;
                    Debug.Log($"Scrolled down at {timeOfLastCycle}");
                }
            }

            if (healInput && !hasHealed)
            {
                hasHealed = true; // Prevent use of heal ore more than once per press 
                playerHealth.UseHealthOre(); // Use health ore to heal colonist
                Debug.Log("Has hit heal button");
            }
            else if (!healInput)
            {
                hasHealed = false; // Allow an emergency/region to be selected
            }

            if (ejectInput && !hasEjected)
            {
                Eject();
                hasEjected = true; // Prevent multiple colonists from being ejected
            }
            else if (!ejectInput)
            {
                hasEjected = false; // Allow another colonist to be ejected
            }

            if (quitInput)
            {
                Application.Quit();
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
        playerInventory.EjectColonist();
    }

    private IEnumerator Drag() // Coroutine to drag objects
    {
        if (grabInput) // Check if the grab input is held down
        {
            Ray ray = mainCamera.ScreenPointToRay(curScreenPos); // Create a ray from the camera to the mouse position
            if (Physics.Raycast(ray, out RaycastHit hit)) // Check if the raycast hits an object
            {
                Transform hitTransform = hit.transform; // Get the transform of the object hit
                Rigidbody hitRb = hitTransform.gameObject.GetComponent<Rigidbody>(); // Get the rigidbody of the object hit
                BreakableAsteroid asteroid = hitTransform.gameObject.GetComponent<BreakableAsteroid>(); // Get the asteroid component of the object hit (if it has one)
                if (asteroid != null) // Check if the object hit is an asteroid
                {
                    asteroid.beingHeld = true; // Set the asteroid to being held
                }
                while (grabInput) // While the grab input is held down
                {
                    if (hitTransform == null || hitRb == null) // Check if the object or rigidbody is destroyed
                    {
                        yield break; // Exit the coroutine if the object is destroyed
                    }

                    hitRb.velocity = (MousePos - hitTransform.position) * attractSpeed; // Move the object towards the mouse position
                    yield return null; // Wait for the next frame
                }
                if (asteroid != null) // Check if the object hit is an asteroid
                {
                    asteroid.beingHeld = false; // Set the asteroid to not being held
                }
            }
        }
    }
}
