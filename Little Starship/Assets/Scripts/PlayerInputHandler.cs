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
    [SerializeField] private string cycle = "Cycle";
    [SerializeField] private string scroll = "Scroll";
    [SerializeField] private string select = "Select";
    [SerializeField] private string eject = "Eject";

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction grabAction;
    private InputAction cycleAction;
    private InputAction scrollAction;
    private InputAction selectAction;
    private InputAction ejectAction;

    //public Camera Camera { get; private set; }
    public Camera Camera;
    public GameObject ObjectUnderPoint { get; private set; }

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool GrabInput { get; private set; }
    public float CycleInput { get; private set; } // -1 for previous slot, 1 for next slot
    public float ScrollInput { get; private set; } // -1 for previous slot, 1 for next slot
    public bool SelectInput { get; private set; }
    public bool EjectInput { get; private set; }

    //private bool isDraging;
    //private Vector3 WorldPos
    //{
    //    get
    //    {
    //        float z = cam.WorldToScreenPoint(transform.position).z;
    //        return cam.ScreenToWorldPoint(curScreenPos + new Vector3(0, 0, z));
    //    }
    //}

    //public bool CanGrab
    //{
    //    get
    //    {
    //        Ray ray = Camera.ScreenPointToRay(LookInput);
    //        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.gameObject.TryGetComponent<Rigidbody>(out Rigidbody hitRb))
    //        {
    //            return hit.transform.gameObject.layer == LayerMask.NameToLayer("Grabbable");
    //        }
    //        return false;
    //    }
    //}

    public static PlayerInputHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        moveAction = playerControls.FindActionMap(actionMapName).FindAction(move);
        lookAction = playerControls.FindActionMap(actionMapName).FindAction(look);
        grabAction = playerControls.FindActionMap(actionMapName).FindAction(grab);
        cycleAction = playerControls.FindActionMap(actionMapName).FindAction(cycle);
        scrollAction = playerControls.FindActionMap(actionMapName).FindAction(scroll);
        selectAction = playerControls.FindActionMap(actionMapName).FindAction(select);
        ejectAction = playerControls.FindActionMap(actionMapName).FindAction(eject);
        RegisterInputActions();
    }

    void RegisterInputActions()
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        lookAction.performed += context => LookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => LookInput = Vector2.zero;

        grabAction.performed += context => GrabInput = true;
        grabAction.canceled += context => GrabInput = false;
        //grabAction.performed += _ => { if (canGrab) StartCoroutine(Drag()); };
        //grabAction.canceled += _ => { isDraging = false; };

        cycleAction.performed += context => CycleInput = context.ReadValue<float>();
        cycleAction.canceled += context => CycleInput = 0;

        scrollAction.performed += context => ScrollInput = context.ReadValue<float>();
        scrollAction.canceled += context => ScrollInput = 0;

        selectAction.performed += context => SelectInput = true;
        selectAction.canceled += context => SelectInput = false;

        ejectAction.performed += context => EjectInput = true;
        ejectAction.canceled += context => EjectInput = false;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        grabAction.Enable();
        cycleAction.Enable();
        scrollAction.Enable();
        selectAction.Enable();
        ejectAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        grabAction.Disable();
        cycleAction.Disable();
        scrollAction.Disable();
        selectAction.Disable();
        ejectAction.Disable();
    }

    //private IEnumerator Drag()
    //{
    //    isDraging = true;
    //    Vector3 offset = transform.position - WorldPos;
    //    // grabing game object
    //    while(isDraging)
    //    {
    //        // dragging game object
    //        transform.position = WorldPos + offset;
    //        yield return null;
    //        Debug.Log("Has grabbed");
    //    }
    //}
}
