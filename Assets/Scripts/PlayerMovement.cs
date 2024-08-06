using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    public float crouchTransitionDuration = 0.2f;

    [Header("Dashing")]
    public float dashForce;
    public float dashDuration = 0.2f;
    public float dashCooldown;
    bool readyToDash = true; 

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode dashKey = KeyCode.E;

    [Header("Ground Check")]
    private float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    
    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rigidBody;

    public MovementState state;

    public enum MovementState {
        walking,
        sprinting,
        crouching,
        air
    }

    private TrailRenderer trailRenderer;

    private Coroutine crouchCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;

        startYScale = transform.localScale.y;
        playerHeight = transform.localScale.y;

        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false; // Disable trail emission at the start
        }
        else
        {
            Debug.LogError("TrailRenderer component not found on the GameObject.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //checking if grounded
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.05f, whatIsGround);

        Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.05f), Color.red);
        
        Debug.Log(moveSpeed);
        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded) {
            rigidBody.drag = groundDrag;
        } else {
            rigidBody.drag = 0;
        }
    }

    void FixedUpdate() {
        MovePlayer();
    }

    private void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey)) {
            Debug.Log("Jump key pressed");
        }

        if (Input.GetKey(jumpKey) && readyToJump && grounded) {
            Jump();
            readyToJump = false;
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey)) {
            if (crouchCoroutine != null) {
                StopCoroutine(crouchCoroutine);
            }
            crouchCoroutine = StartCoroutine(Crouch());
        }

        if (Input.GetKeyUp(crouchKey)) {
            if (crouchCoroutine != null) {
                StopCoroutine(crouchCoroutine);
            }
            crouchCoroutine = StartCoroutine(Uncrouch());
        }

        if (Input.GetKey(dashKey) && readyToDash) {
            StartCoroutine(Dash());
            readyToDash = false;
            Invoke(nameof(ResetDash), dashCooldown);
        }
    }

    private void StateHandler() {
        if (Input.GetKey(crouchKey)) {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        } else if (grounded && Input.GetKey(sprintKey)) {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        } else if (grounded) {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        } else {
            state = MovementState.air;
        }
    }

    private void MovePlayer() {
        //calculate movement direction and walk where you are looking
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded || state == MovementState.crouching) {
            rigidBody.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);
        } else if (!grounded) {
            rigidBody.AddForce(10f * moveSpeed * airMultiplier * moveDirection.normalized, ForceMode.Force);
        }
    }

    private void SpeedControl() {
        Vector3 flatVel = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);

        if (flatVel.magnitude > moveSpeed) {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rigidBody.velocity = new Vector3(limitedVel.x, rigidBody.velocity.y, limitedVel.z);
        }
    }

    private void Jump() {
        //Debug.Log("Jump called");
        // reset y velocity
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
        rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() {
        readyToJump = true;
    }

    private IEnumerator Dash() {
        // Vector3 dashDirection = orientation.forward; dashes forward only
        Vector3 dashDirection = moveDirection.normalized;
        if (dashDirection == Vector3.zero) {
            dashDirection = orientation.forward;
        }
        trailRenderer.emitting = true;

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration) {
            rigidBody.AddForce(dashDirection * (dashForce * Time.deltaTime / dashDuration), ForceMode.Impulse);
            yield return null;
        }
        trailRenderer.emitting = false;
    }

    private void ResetDash() {
        readyToDash = true;
    }

    private IEnumerator Crouch() {
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = new Vector3(originalScale.x, startYScale * crouchYScale, originalScale.z);

        while (elapsedTime < crouchTransitionDuration) {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / crouchTransitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }
    private IEnumerator Uncrouch() {
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = new Vector3(originalScale.x, startYScale, originalScale.z);

        while (elapsedTime < crouchTransitionDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / crouchTransitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }
}
