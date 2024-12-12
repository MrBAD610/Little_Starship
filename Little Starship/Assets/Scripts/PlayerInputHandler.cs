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
    [SerializeField] private string heal = "Heal";
    [SerializeField] private string eject = "Eject";
    [SerializeField] private string quit = "Quit";

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction grabAction;
    private InputAction cycleAction;
    private InputAction scrollAction;
    private InputAction healAction;
    private InputAction ejectAction;
    private InputAction quitAction;

    //public Camera Camera { get; private set; }
    public Camera Camera;
    public GameObject ObjectUnderPoint { get; private set; }

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool GrabInput { get; private set; }
    public float CycleInput { get; private set; } // -1 for previous slot, 1 for next slot
    public float ScrollInput { get; private set; } // -1 for previous slot, 1 for next slot
    public bool HealInput { get; private set; }
    public bool EjectInput { get; private set; }
    public bool QuitInput { get; private set; }


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
        healAction = playerControls.FindActionMap(actionMapName).FindAction(heal);
        ejectAction = playerControls.FindActionMap(actionMapName).FindAction(eject);
        quitAction = playerControls.FindActionMap(actionMapName).FindAction(quit);  
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

        cycleAction.performed += context => CycleInput = context.ReadValue<float>();
        cycleAction.canceled += context => CycleInput = 0;

        scrollAction.performed += context => ScrollInput = context.ReadValue<float>();
        scrollAction.canceled += context => ScrollInput = 0;

        healAction.performed += context => HealInput = true;
        healAction.canceled += context => HealInput = false;

        ejectAction.performed += context => EjectInput = true;
        ejectAction.canceled += context => EjectInput = false;

        quitAction.performed += context => QuitInput = true;
        quitAction.canceled += context => QuitInput = false;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        grabAction.Enable();
        cycleAction.Enable();
        scrollAction.Enable();
        healAction.Enable();
        ejectAction.Enable();
        quitAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        grabAction.Disable();
        cycleAction.Disable();
        scrollAction.Disable();
        healAction.Disable();
        ejectAction.Disable();
        quitAction.Disable();
    }
}
