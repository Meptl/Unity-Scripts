/**
 * Allows for movement of an object. Includes horizontal movement and jumping.
 * Movement is done using CharacterController.move()
 * Gravity is applied using this as well.
 */
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent (typeof (CharacterController))]
public class ControllerMovement : NetworkBehaviour {
    public float moveSpeed = 6f;
    public float moveSide = 0.86f;
    public float moveBack = 0.7f;
    public float jumpControlPenalty = 0.1f; // movement speed penalty while in air.
    public float jumpForce = 8f;
    public float jumpDelay = 0.3f; // Delay between being able to jump
    public float gravityModifier = 0.3f;
    public float slopeForce = 5f;


    // baseGravStr applied when on ground
    private const float gravityBaseStr = -0.1f;
    private CharacterController controller;
    private Vector3 finalMovement, groundNormal;
    private Vector3 gravityAccum, gravityBase;
    private bool canJump;

    // User Input
    private float hori, vert;
    private bool jumpKey;

    void Awake()
    {
        finalMovement = Vector3.zero;
        canJump = true;
        gravityBase = new Vector3(0, gravityBaseStr, 0);
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // We use raw input to have a more responsive movement
        // We may need to check for joysticks and use GetAxis instead.
        hori = Input.GetAxisRaw("Horizontal"); // Only evaluates to -1, 0, or 1
        vert = Input.GetAxisRaw("Vertical"); // Only evaluates to -1, 0, or 1
        jumpKey = Input.GetButtonDown("Jump");
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        bool grounded = controller.isGrounded;
        Vector3 input = createInputVector(hori, vert);
        float groundAngle = calcSlopeAngle(groundNormal);

        inputStateModification(ref input, grounded, groundAngle);

        inputToMovement(ref input, ref finalMovement, grounded);

        trySlopeForce(ref finalMovement, grounded, groundAngle);

        tryJump(ref finalMovement, grounded, groundAngle);

        applyGravity(ref finalMovement, grounded);

        Vector3 debugVec = Vector3.ClampMagnitude(finalMovement, 1f);
        DebugUtilities.debugVector(transform.position + new Vector3(0, -1, 0), debugVec);
        controller.Move(finalMovement * Time.deltaTime);
    }

    public override void OnStartLocalPlayer()
    {
        // Modifications!
    }

    /**
     * When controller is standing on a slope larger than the slopeLimit
     * apply a normal force. Side collisions are checked since walls give a
     * slopeAngle larger than the slope limit (90). Ignore ceiling collisions.
     */
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Causes movements that hit a ceiling to stop early.
        if ((controller.collisionFlags & CollisionFlags.Above) != 0 && finalMovement.y > 0) {
            finalMovement.y = 0;
        }

        if ((controller.collisionFlags & CollisionFlags.Below) != 0) {
            groundNormal = hit.normal;
        }
    }


    /**
     * Apply jumping to movement
     */
    private void tryJump(ref Vector3 movement, bool grounded, float groundAngle) {
        if (grounded && groundAngle < controller.slopeLimit && jumpKey && canJump) {
            movement += Vector3.up * jumpForce;
            StartCoroutine(startJumpDelay());
        }
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
     * Modifies the input vector after checking particular states of the controller
     */
    private void inputStateModification(ref Vector3 input, bool grounded, float groundAngle) {
        if (grounded) {
            if (groundAngle > controller.slopeLimit) {
                // Disable input toward slope
                Vector3 dirToSlope = -groundNormal;
                dirToSlope.y = 0;
                Vector3.Normalize(dirToSlope);

                float dot = Vector3.Dot(dirToSlope, input);
                if (dot > 0) {
                    input -= dirToSlope * dot;
                }
            } else {
                // Only plane project downhill. if input == 0 then dot == 0.
                bool goingDownhill = Vector3.Dot(input, groundNormal) > 0;
                if (goingDownhill) {
                    input = Vector3.ProjectOnPlane(input, groundNormal);
                }
            }
        } else {
            input *= jumpControlPenalty;
        }
    }

    /**
     * Creates the movement vec from the input vec.
     */
    private void inputToMovement(ref Vector3 input, ref Vector3 movement, bool grounded) {
        if (grounded) {
            movement = input;
        } else {
            Vector3 result = movement + input;
            if (result.magnitude < this.moveSpeed) {
                movement = result;
            }
        }
    }

    /**
     * Apply a force down a slope which is beyond the slope limit
     */
    private void trySlopeForce(ref Vector3 movement, bool grounded, float groundAngle) {
        if (grounded && groundAngle > controller.slopeLimit) {
            // Adds a force that will cause the controller to slide down slopes
            Vector3 downSlope = Vector3.ProjectOnPlane(Vector3.down, groundNormal);

            movement += downSlope * slopeForce;
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
     * Coroutine falsifies a canJump for jumpDelay seconds.
     */
    private IEnumerator startJumpDelay()
    {
        canJump = false;
        yield return new WaitForSeconds(jumpDelay);
        canJump = true;
    }

    /**
     * Adds gravity to supplied vector, modifiying accumGravity if needed.
     */
    private void applyGravity(ref Vector3 movement, bool grounded) {
        if (grounded) {
            // Applying 0 downward force on the controller prevents the grounded flag from flipping
            gravityAccum = gravityBase;
        } else {
            // Physics.gravity is already negative.
            gravityAccum += gravityModifier * Physics.gravity * Time.deltaTime;
        }

        movement += gravityAccum;
    }
}
