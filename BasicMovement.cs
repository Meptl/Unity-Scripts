/**
 * Allows for movement of an object. Includes horizontal movement and jumping.
 * Movement is done using CharacterController.move()
 * Gravity is applied using this as well.
 */
using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]
public class BasicMovement : MonoBehaviour {
    public float mass = 5f;
    public float moveSpeed = 6f;
    public float moveSide = 0.86f;
    public float moveBack = 0.7f;
    public float jumpControlPenalty = 0.05f; // movement speed penalty while in air.
    public float jumpForce = 8f;
    public float jumpDelay = 0.3f; // Delay between being able to jump
    public float gravity = 3f;

    // normalForce: prevents users from sticking to walls by adding force towards them
    public float normalForce = 0.025f;
	public float rayLength = 0.3f;

	private const float baseGrav = 0.1f;
    private CharacterController controller;
	private Vector3 movementVec;
	private Vector3 groundNorm;
	private bool canJump;
	private float accumGravity;

	// User Input
	private float hori, vert;
	private bool jumpKey;

	private Ray groundRay;

    void Awake()
    {
		movementVec = Vector3.zero;
		groundNorm = Vector3.up;
		canJump = true;
		groundRay = new Ray(transform.position, Vector3.down); // origin is wrong
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
		snapToGround();

        bool grounded = controller.isGrounded;
		Debug.Log(grounded.ToString());

        Vector3 inputVec = createInputVector(hori, vert, grounded);
		if (grounded) {
			movementVec = inputVec;
			if (jumpKey && canJump) {
				StartCoroutine(applyJump());
			}
		} else {
			// This may cause really fast air movements over time
			// We'll see.
			movementVec += inputVec;
		}

		applyGravity(grounded);
		DebugUtilities.debugVector(transform.position + new Vector3(0, -1, 0), movementVec.normalized);
        controller.Move(movementVec * Time.deltaTime);
    }

	/**
	 * Sends a raycast downward. If it hits, move downward
	 * Used to allow smooth downward traversal of slopes
	 */
	private void snapToGround() {
		// Make sure we didn't recently just jump by checking the canJump flag.
		if (!controller.isGrounded) {
			Vector3 controllerBase = transform.position - new Vector3(0, controller.height / 2, 0);
			groundRay.origin = controllerBase;

			RaycastHit hit;
			if (Physics.Raycast(groundRay, out hit, rayLength)) {
				// Don't snap for two reasons: 
				// 1. We've recently jumped.
				// 2. We have a velocity going nearly down which indicates we're
				//    going to hit the ground (likely not going down a slope)
				if (canJump) {
					Vector3 delta = hit.point - controllerBase;
					Debug.Log("Snapping!");
					controller.Move(delta);
				}
			}
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

		// Applies a normal force from anything above the slope limit of the charController.
		/*
        if (hit.normal.y > 0 && (controller.collisionFlags & CollisionFlags.Sides) == 0) {
            // slopeAngle for flat surface is 90. Will translate this later.
            float slopeAngle = Vector3.Angle(hit.normal, transform.right);
            if (slopeAngle > 90) {
                // 92 degrees should translate to 88 degrees
                slopeAngle -= 90;
                slopeAngle = 90 - slopeAngle;
            }

            // inverse one last time to get true angle of a slope.
            slopeAngle = 90 - slopeAngle;

            if (slopeAngle > controller.slopeLimit) {
				//Debug.Log("This happened!");
                // Only horizontal expulsion.
                Vector3 expulsionVec = hit.normal;
                expulsionVec.y = 0;
            }
        }
		*/

		if ((controller.collisionFlags & CollisionFlags.Below) != 0) {
			float slopeAngle = calcSlopeAngle(hit.normal);
			groundNorm = hit.normal;

			if (slopeAngle > controller.slopeLimit) {
			} else {
			}
		}


		//snapToGround(hit);
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
     * Create the flat movement vector resulting from: user input, constant modification
     * from moving forward/backward, ground state
     */
    private Vector3 createInputVector(float hori, float vert, bool grounded)
    {
        Vector3 movement = new Vector3(hori, 0, vert);

        // Prevents diagonal movements being as fast as forward movements.
        Vector3.Normalize(movement);

        movement = transform.TransformDirection(movement);

        if (hori != 0) { movement *= moveSide; }
        if (vert < 0) { movement *= moveBack; }

        movement *= moveSpeed;

		if (!grounded) {
            movement *= jumpControlPenalty;
        }

        return movement;
    }

    /**
     * Coroutine implements jumping and delay between jumps.
     * Modifies class variable movementVec
     */
	private IEnumerator applyJump()
    {
		movementVec += Vector3.up * jumpForce;

        canJump = false;
        yield return new WaitForSeconds(jumpDelay);
        canJump = true;
    }

	/**
	 * Adds gravity to movementVec, modifiying accumGravity if needed.
	 */
	private void applyGravity(bool grounded) {
		if (grounded) {
			// Applying 0 downward force on the controller prevents the grounded flag from flipping
			accumGravity = baseGrav;
		} else {
			accumGravity += gravity * Time.deltaTime;
		}

		movementVec += Vector3.down * accumGravity;
	}
}
