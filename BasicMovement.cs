/**
 * Allows for movement of an object. Includes horizontal movement and jumping.
 * Movement is done using CharacterController.move()
 * Gravity is applied using this as well.
 */
using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]
public class BasicMovement : MonoBehaviour {
    public float moveSpeed = 6f;
    public float moveSide = 0.86f;
    public float moveBack = 0.7f;
    public float jumpControlPenalty = 0.05f; // movement speed penalty while in air.
    public float jumpForce = 12f;
    public float jumpDelay = 0.3f; // Delay between being able to jump
    public float gravityModifier = 0.5f;

    // normalForce: prevents users from sticking to walls by adding force away from
    public float normalForce = 0.025f;

    // baseGravStr applied when on ground
    private const float gravityBaseStr = -0.1f;
    private const float groundCheckLength = 0.5f;
    private CharacterController controller;
    private Vector3 movementVec, slopeWallForce;
    private Vector3 gravityAccum, gravityBase;
    private bool canJump;

    // User Input
    private float hori, vert;
    private bool jumpKey;

    private Ray groundRay;

    void Awake()
    {
        movementVec = Vector3.zero;
        canJump = true;
        groundRay = new Ray(transform.position, Vector3.down); // origin is wrong
        gravityBase = new Vector3(0, gravityBaseStr, 0);
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // We use raw input to have a more responsive movement
        hori = Input.GetAxisRaw("Horizontal"); // Only evaluates to -1, 0, or 1
        vert = Input.GetAxisRaw("Vertical"); // Only evaluates to -1, 0, or 1
        jumpKey = Input.GetButton("Jump");
    }

    void FixedUpdate()
    {
        bool grounded = controller.isGrounded;

        Vector3 inputVec = createInputVector(hori, vert);

        if (grounded) {
            RaycastHit hitInfo;
            if (groundCheck(out hitInfo)) {
            } else {
                //Debug.Log("ground check while grounded failed.");
            }

            // Only project if going downhill. There are stuttering issues going uphill.
            Vector3 groundNorm = getGroundNormal();
            if (groundNorm != Vector3.right && inputVec != Vector3.zero && Vector3.Dot(groundNorm, inputVec) >= 0) {
                inputVec = Vector3.ProjectOnPlane(inputVec, groundNorm);
            }

            movementVec = inputVec;
            if (jumpKey && canJump) {
                movementVec += Vector3.up * jumpForce;
                StartCoroutine(startJumpDelay());
            }
        } else {
            // This may cause really fast air movements over time, we'll see.
            inputVec *= jumpControlPenalty;
            movementVec += inputVec;
        }

        applyGravity(ref movementVec, grounded);

        //movementVec += slopeWallForce;
        //slopeWallForce = Vector3.zero;

        Vector3 debugVec = Vector3.ClampMagnitude(movementVec, 1f);
        DebugUtilities.debugVector(transform.position + new Vector3(0, -1, 0), debugVec);
        controller.Move(movementVec * Time.deltaTime);
    }

    /**
     * Raycasts toward the ground from the base of the controller.
     */
    private bool groundCheck(out RaycastHit hitInfo) {
        Vector3 controllerBase = transform.position - new Vector3(0, controller.height / 2, 0);
        groundRay.origin = controllerBase;

        RaycastHit hit;
        if (Physics.Raycast(groundRay, out hit, groundCheckLength)) {
            hitInfo = hit;
            return true;
        }

        return false;
    }

    /**
     * Ray casts below the controller returning the hit normal or Vector3.right
     */
    private Vector3 getGroundNormal() {
        Vector3 controllerBase = transform.position - new Vector3(0, controller.height / 2, 0);
        groundRay.origin = controllerBase;

        RaycastHit hit;
        if (Physics.Raycast(groundRay, out hit, groundCheckLength)) {
            return hit.normal;
        }
        return Vector3.right;
    }

    /**
     * Given the ground point, moves controller toward it.
     */
    private void snapToGround(Vector3 point) {
        // Don't snap if:
        // 1. We've recently jumped.
        if (canJump) {
            Vector3 controllerBase = transform.position - new Vector3(0, controller.height / 2, 0);
            Vector3 delta = point - controllerBase;
            controller.Move(delta);
        }
    }

    /**
     * When controller is standing on a slope larger than the slopeLimit
     * apply a normal force. Side collisions are checked since walls give a
     * slopeAngle larger than the slope limit (90). Ignore ceiling collisions.
     */
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Causes movements that hit a ceiling to stop early.
        if ((controller.collisionFlags & CollisionFlags.Above) != 0 && movementVec.y > 0) {
            movementVec.y = 0;
        }

        if ((controller.collisionFlags & CollisionFlags.Below) != 0) {
            float slopeAngle = calcSlopeAngle(hit.normal);
            if (slopeAngle > controller.slopeLimit) {
                // Remove movementVec towards the wall
                slopeWallForce = hit.normal * normalForce;
            }
        }
    }

    /**
     * Returns the angle of a slope that a CharController hit given the slope's normal.
     */
    private float calcSlopeAngle(Vector3 normal)
    {
        Vector3 flatNormal = normal;
        flatNormal.y = 0;

        float slopeAngle = Vector3.Angle(normal, flatNormal);
        slopeAngle = Mathf.Abs(slopeAngle - 90);

        return slopeAngle;
    }

    /**
     * Create the movement vector resulting from: user input, constant modification
     * from moving forward/backward, ground state
     */
    private Vector3 createInputVector(float hori, float vert)
    {
        Vector3 movement = new Vector3(hori, 0, vert);

        // Prevents diagonal movements being as fast as forward movements.
        Vector3.Normalize(movement);

        movement = transform.TransformDirection(movement);

        if (hori != 0) { movement *= moveSide; }
        if (vert < 0) { movement *= moveBack; }

        movement *= moveSpeed;

        return movement;
    }

    /**
     * Coroutine implements jumping and delay between jumps.
     * Modifies class variable movementVec
     */
    private IEnumerator startJumpDelay()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpDelay);
        canJump = true;
    }

    /**
     * Adds gravity to movementVec, modifiying accumGravity if needed.
     */
    private void applyGravity(ref Vector3 vec, bool grounded) {
        if (grounded) {
            // Applying 0 downward force on the controller prevents the grounded flag from flipping
            gravityAccum = gravityBase;
        } else {
            // Physics.gravity is already negative.
            gravityAccum += gravityModifier * Physics.gravity * Time.deltaTime;
        }

        vec += gravityAccum;
    }
}
