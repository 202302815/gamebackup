using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkingSpeed = 7f;
    [SerializeField] private float vehicleSpeed = 10f;
    [SerializeField] private float runningSpeed = 10f;
    private bool sprintHeld;
    public VehicleRoadRules vehicleRules;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    public Vector2 FacingDirection { get; private set; } = Vector2.down;
    public Vector2 MovementInput => moveInput;
    public MovementAreaRules movementAreaRules;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        if (moveInput.sqrMagnitude > 0.001f)
            FacingDirection = moveInput.normalized;

        // Clear sprint when paused or riding.
        if (Time.timeScale == 0f ||
            (vehicleRules != null && vehicleRules.vehicleMode))
        {
            sprintHeld = false;
        }
    }

    void FixedUpdate()
    {
        bool riding =
             vehicleRules != null && vehicleRules.vehicleMode;

        float speed = riding
            ? vehicleSpeed
            : (sprintHeld ? runningSpeed : walkingSpeed);

        Vector2 velocity =
            Vector2.ClampMagnitude(moveInput, 1f) * speed;

        // Preserve your existing wrong-way allowance.
        if (vehicleRules != null)
        {
            velocity = vehicleRules.LimitVehicleVelocity(
                rb.position,
                velocity,
                Time.fixedDeltaTime);
        }

        // Apply road/sidewalk boundaries afterward.
        if (movementAreaRules != null)
        {
            velocity = movementAreaRules.LimitMovement(
                rb.position,
                velocity,
                Time.fixedDeltaTime);
        }

        rb.linearVelocity = velocity;
    }

    public void Move(InputAction.CallbackContext context)
    {
       
        animator.SetBool("isWalking", true);
        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }
    
    public void MoveUpButton()
    {
        moveInput = Vector2.up;
        animator.SetBool("isWalking", true);
        animator.SetFloat("InputX", 0);
        animator.SetFloat("InputY", 1);
    }

    public void MoveDownButton()
    {
        moveInput = Vector2.down;
        animator.SetBool("isWalking", true);
        animator.SetFloat("InputX", 0);
        animator.SetFloat("InputY", -1);
    }

    public void MoveLeftButton()
    {
        moveInput = Vector2.left;
        animator.SetBool("isWalking", true);
        animator.SetFloat("InputX", -1);
        animator.SetFloat("InputY", 0);
    }

    public void MoveRightButton()
    {
        moveInput = Vector2.right;
        animator.SetBool("isWalking", true);
        animator.SetFloat("InputX", 1);
        animator.SetFloat("InputY", 0);
    }
    public void StopButton()
    {
        sprintHeld = false;
        animator.SetBool("isWalking", false);
        animator.SetFloat("LastInputX", moveInput.x);
        animator.SetFloat("LastInputY", moveInput.y);
        moveInput = Vector2.zero;
    }
    public void StartSprint()
    {
        if (Time.timeScale == 0f)
            return;

        if (vehicleRules != null && vehicleRules.vehicleMode)
            return;

        sprintHeld = true;
    }

    public void StopSprint()
    {
        sprintHeld = false;
    }

    void OnDisable()
    {
        sprintHeld = false;
    }

}
