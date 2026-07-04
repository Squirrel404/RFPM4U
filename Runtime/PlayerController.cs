using NaughtyAttributes;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 2.18f;
    [SerializeField] float sprintSpeed = 3.3f;
    float MoveSpeed { get { return sprinting ? sprintSpeed : walkSpeed; } }
    [MinValue(0)] [SerializeField] float accelerationTime = 0.1f;
    float accelerationVelocity;
    [HideInInspector] public float currentMoveSpeed;
    [HideInInspector] public bool sprinting;

    [Header("Walk Cycle")]
    [SerializeField] bool walkCycleDips = true;
    [MinValue(0)] [SerializeField] float walkStepInterval = 0.6f;
    public float StepInterval { get { return walkStepInterval * (walkSpeed / MoveSpeed); } }
    [MinValue(0)] [SerializeField] float stepDipAmount = 0.3f;
    float stepPhaseTime;
    float legMult = 1;
    float randomStepIntervalMult = 1;
    float previousPhase;
    float walkCycleMult;
    [HideInInspector] public float walkPhase;
    [HideInInspector] public bool tookStepThisFrame;

    [Dropdown("legs")]
    [SerializeField] string dominantLeg = "Right";
    [MinValue(0)]
    [Tooltip("Determines how much stronger the dominant leg is. Set to 0 for equal balance.")]
    [SerializeField] float legDominance = 0.05f;
    [Tooltip("The higher, the more random the leg dominance is each step.")]
    [SerializeField] float legVariability = 0.02f;
    [HideInInspector] public bool leftStep;
    bool DominantStep { get { return (leftStep && dominantLeg == "Left") || (!leftStep && dominantLeg == "Right"); } }
    string[] legs = new string[] { "Left", "Right" };

    [Tooltip("Determines how much control the player has over move direction between steps.")]
    [Range(0f, 2f)] [SerializeField] float stepControl = 0.75f;
    Vector3 stepDir;

    [Header("References")]
    [Foldout("References")] public Rigidbody rb;
    [Tooltip("Should be main camera.")]
    Transform orientation;

    [Header("Input")]
    Vector2 moveInput;
    MovementInput controls;
    public bool Moving { get { return moveInput != Vector2.zero; } }
    Vector3 MoveDir
    {
        get
        {
            Vector3 moveDir = orientation.right * moveInput.x + orientation.forward * moveInput.y;
            moveDir.y = 0;
            return moveDir.normalized;
        }
    }

    void Awake()
    {
        controls = new MovementInput();

        controls.Movement.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
        controls.Movement.Move.canceled += ctx => StopMoving();
        controls.Movement.Sprint.performed += ctx => Sprint();
        controls.Movement.Sprint.canceled += ctx => Sprint(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        orientation = Camera.main.transform;
    }

    void OnEnable()
    {
        controls.Movement.Enable();
    }

    void OnDisable()
    {
        controls.Movement.Disable();
    }

    void Update()
    {
        if (Moving)
            currentMoveSpeed = Mathf.SmoothDamp(currentMoveSpeed, MoveSpeed, ref accelerationVelocity, accelerationTime);
        else
            currentMoveSpeed = 0;

        WalkCycleUpdate();
    }

    void WalkCycleUpdate()
    {
        tookStepThisFrame = false;

        if (!Moving || !walkCycleDips)
        {
            walkCycleMult = 1;
            stepPhaseTime = 0;
            previousPhase = 0;
            leftStep = false;
            return;
        }

        currentMoveSpeed = Mathf.Max(0.001f, currentMoveSpeed); // Might fix a NaN error, might not do anything at all

        walkPhase = stepPhaseTime / StepInterval % 1;

        if (walkPhase < previousPhase)
        {
            Step();
        }
        previousPhase = walkPhase;

        float dotMult = 1 / (Mathf.Max(0.1f, Vector3.Dot(MoveDir, stepDir) + 1) / 2);

        float dipMult = Mathf.Min(0.75f, walkSpeed / currentMoveSpeed) * legMult * dotMult;
        float dip = Mathf.SmoothStep(0, 1, Mathf.Pow(Mathf.Sin(walkPhase * Mathf.PI), 1.5f)) * dipMult;

        walkCycleMult = 1 - Mathf.Clamp01(stepDipAmount * dip);
        float speedCorrection = 1 - stepDipAmount * 0.5f;
        walkCycleMult *= 1 / speedCorrection;

        stepPhaseTime += Time.deltaTime * currentMoveSpeed / MoveSpeed * randomStepIntervalMult * dotMult;
    }

    void FixedUpdate()
    {
        if (!Moving)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 moveDir = MoveDir;
        float dot = (Vector3.Dot(moveDir, stepDir) + 1) / 2;
        moveDir = Vector3.Lerp(stepDir, moveDir, Mathf.Abs(dot) * stepControl);

        rb.linearVelocity = moveDir * MoveSpeed * walkCycleMult; // ISSUE: velocity can be (NaN, NaN, NaN) when mashing movement buttons. Very rare and can be ignored.
    }

    void Sprint(bool toggle = true)
    {
        sprinting = toggle;
        stepPhaseTime = walkPhase * StepInterval;
        previousPhase = 0;
    }

    void Step()
    {
        leftStep = !leftStep;

        float _legVariability = legVariability;
        if (_legVariability > legDominance)
            _legVariability = legDominance;
        legMult = DominantStep
            ? Random.Range(1 + legDominance - _legVariability, 1 + legDominance + _legVariability)
            : Random.Range(1 - legDominance - _legVariability, 1 - legDominance + _legVariability);
        randomStepIntervalMult = Random.Range(0.95f, 1.05f);

        stepDir = MoveDir;

        tookStepThisFrame = true;

        // Play footstep sound
    }

    void Move(Vector2 input)
    {
        bool wasMoving = Moving;
        moveInput = input;
        if (!wasMoving)
            StartMoving();
    }

    void StartMoving()
    {
        stepDir = MoveDir;
    }

    void StopMoving()
    {
        moveInput = Vector2.zero;

        // Play dampened, low pitch footstep sound
    }

#if UNITY_EDITOR
    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        orientation = Camera.main.transform;
    }
#endif
}
