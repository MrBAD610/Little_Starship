using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action Map Name References")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Action Name References")]
    [SerializeField] private string move = "Move";
    [SerializeField] private string look = "Look";
    [SerializeField] private string grab = "Grab";

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction grabAction;

    private Vector3 curScreenPos;

    Camera cam;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    //public ButtonControl GrabInput { get; private set; }

    private bool isDraging;
    private Vector3 worldPos
    {
        get
        {
            float z = cam.WorldToScreenPoint(transform.position).z;
            return cam.ScreenToWorldPoint(curScreenPos + new Vector3(0, 0, z));
        }
    }
    private bool canGrab
    {
        get
        {
            Ray ray = cam.ScreenPointToRay(curScreenPos);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                return hit.transform.gameObject.layer == LayerMask.NameToLayer("Grabbable");
            }
            return false;
        }
    }
    

    public static PlayerInputHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        cam = Camera.main;

        moveAction = playerControls.FindActionMap(actionMapName).FindAction(move);
        lookAction = playerControls.FindActionMap(actionMapName).FindAction(look);
        grabAction = playerControls.FindActionMap(actionMapName).FindAction(grab);
        RegisterInputActions();
    }

    void RegisterInputActions()
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        lookAction.performed += context => LookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => LookInput = Vector2.zero;

        grabAction.performed += _ => { if (canGrab) StartCoroutine(Drag()); };
        grabAction.canceled += _ => { isDraging = false; };
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
    }

    private IEnumerator Drag()
    {
        isDraging = true;
        // grabing game object
        while(isDraging)
        {
            // dragging game object
            yield return null;
            Debug.Log("Has grabbed");
        }
    }
}
