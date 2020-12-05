// created from brackeys tutorial code, modified with other code in mind. https://www.youtube.com/watch?v=dwcT-Dch0bA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private LayerMask m_WhatCanBeGrappled;					    // A mask determining what can be grabbed via Raycast to Cursor
	[SerializeField] private LayerMask m_grappleAreas;					    	// A mask determining what can be grabbed via Cursor position
	[SerializeField] public float m_maxGrappleDistance;							// Max distance the grapple can reach

	// just for temporary visuals
	public LineRenderer grappleLine;

	// for storing collision checks of this character (struct definition at the bottom)
	public CollisionInfo isTouching = new CollisionInfo();
	private Collider2D characterCollider;
	private Rigidbody2D characterRigidbody;
	public float touchCheckSize = 0.1f;
	private static readonly Vector2[] touchDirections = new [] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

	// TODO: rework flip logic and move it to the the collision struct
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.

	// private Vector3 m_Velocity = Vector3.zero;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	private HingeJoint2D characterHanger;  //Tether that we create and destroy depending on input
	public BoolEvent OnHangEvent;
	// private bool m_isHanging = false; // idk if we need this?
	private bool m_wasHangHeld = false;
	private GameObject currentGrapple;
	public GameObject characterGrappleHook;

	//public float anchorR; // Length of hingeJoint2D / Rope.  Set in inspector in Unity
   
    /// </summary>

	private void Awake()
	{
		characterRigidbody = GetComponent<Rigidbody2D>();
		characterCollider = GetComponent<Collider2D>();
		// try to grab it from components on this object.
		if (!characterHanger) {
			characterHanger = GetComponent<HingeJoint2D>();
		}
		// otherwise slap a new one on.
		if (!characterHanger ) {
			characterHanger = gameObject.AddComponent(typeof(HingeJoint2D)) as HingeJoint2D;
			characterHanger.enabled = false;
		}
		characterHanger.enableCollision = true;  //False by default?  Preposterous. 

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnHangEvent == null)
			OnHangEvent = new BoolEvent();

		grappleLine.enabled = false;
	}

	private void FixedUpdate()
	{
		updateTouching();
	}

	//Draw the BoxCast as a gizmo to show where it currently is testing. Click the Gizmos button to see this
    void OnDrawGizmos()
    {
    	Vector3 target = characterCollider ? characterCollider.bounds.center : transform.position;
    	Vector3 size = characterCollider ? characterCollider.bounds.size : transform.localScale;
        // loop through all directions we check for want to check for what it is touching.
		for (int i = 0; i < touchDirections.Length; i++) {
			//Check if there has been a hit yet
	        if (isTouching[i]) {
	        	Gizmos.color = Color.green;
	        } else {
	        	Gizmos.color = Color.red;
	        }
			//Draw a Ray forward from GameObject toward the maximum distance
	        Gizmos.DrawRay(target, (Vector3)touchDirections[i] * touchCheckSize);
	        //Draw a cube at the maximum distance
	        Gizmos.DrawWireCube(target + (Vector3)touchDirections[i] * touchCheckSize, size);
		} 
    }

	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void Move(Vector2 move, Vector2 cursorPosition, bool jump, bool hang)
	{
		// Check if the character is touching a ceiling
		// NOTE: not using this yet, just keeping it here for notes.
		// if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround)) { // ... do something }

		//only control the player if grounded or airControl is turned on
		if (isTouching.down || m_AirControl)
		{
			// Update the hanging states and event based on the input.
			if (hang) {
				if (!m_wasHangHeld) {
					m_wasHangHeld = true;
					StartCoroutine(grappleToPoint(cursorPosition));
				}
			} else {
				if (m_wasHangHeld) {
					m_wasHangHeld = false;
					// no need to end hang function, the coroutine will do that, looking at the "wasHanging" state
				}
			}

			// Move the character by finding the target velocity
			//Vector3 targetVelocity = new Vector2(move.x * 10f, characterRigidbody.velocity.y);
			// And then smoothing it out and applying it to the character
			// characterRigidbody.velocity = Vector3.SmoothDamp(characterRigidbody.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			// move the character with physics forces
			characterRigidbody.AddForce(move); //, ForceMode2D.Impulse);

			// // If the input is moving the player right and the player is facing left...
			// if (move.x > 0 && !m_FacingRight)
			// {
			// 	// ... flip the player.
			// 	Flip();
			// }
			// // Otherwise if the input is moving the player left and the player is facing right...
			// else if (move.x < 0 && m_FacingRight)
			// {
			// 	// ... flip the player.
			// 	Flip();
			// }
		}
		// If the player should jump...
		if (isTouching.down && jump)
		{
			// Add a vertical force to the player.
			isTouching.down = false;
			characterRigidbody.AddForce(new Vector2(0f, m_JumpForce));
		}
	}

	private void HangStart(Vector2 targetWorldPoint)
	{
		// Enable HingeJoint2D component on player
		characterHanger.enabled = true;
		// trigger the event
        OnHangEvent.Invoke(true);

		// vector 3 just gets implicitly converted to vector 2 here, so it will comply with the anchor fine.
        Vector2 desiredAnchorLocal = transform.InverseTransformPoint(targetWorldPoint); //transform from world to local
        characterHanger.anchor = desiredAnchorLocal;

        //For creating a sprite for the hook.  Rope between hook and player will come later
        currentGrapple = Instantiate(characterGrappleHook, targetWorldPoint, Quaternion.identity);
	}

	private void HangEnd()
	{
		// Disable HingeJoint2D component on player
		characterHanger.enabled = false;
		// end the event
		OnHangEvent.Invoke(false);
		// Destory The Prefab to the grapple sprite
		if (currentGrapple) {
	        Destroy(currentGrapple);
	    }
	}

	public void updateTouching()
	{
		bool wasGrounded = isTouching.down;
		RaycastHit2D hitResult;
		// loop through all directions we check for want to check for what it is touching.
		for(int i = 0; i < touchDirections.Length; i++) {
			hitResult = Physics2D.BoxCast(characterCollider.bounds.center, characterCollider.bounds.size, 0f, touchDirections[i], touchCheckSize, m_WhatIsGround);
			// as long as the collision object is not THIS object, then we can proceed
			if (hitResult.collider != null) {
				// Debug.Log(i + ' ' + hitResult.collider.name);
				isTouching[i] = true;
				if (!wasGrounded && i == (int)CharacterController2D.CollisionInfo.TouchDirection.down) {
					OnLandEvent.Invoke();
				}
			} else {
				isTouching[i] = false;
			}
		} 
	}


	// using coroutines (to avoid a lot of update calls), attempt to grapple to the given point.
    IEnumerator grappleToPoint (Vector2 target)
    {
    	// cast a ray from player position to the target
    	Vector2 rayDirection = target - (Vector2)transform.position;
    	Vector2 lineEnd = transform.position;
    	bool is_valid;
    	float grappleRadius = 0.2f;
    	float timer = 0f;
    	float timeOut = 0.5f; // for when the grapple misses, only show the line for a short time

    	// Just trying to see the direction im trowing the raycast in...
    	Debug.DrawRay(transform.position, rayDirection, Color.green, 0);

    	RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, rayDirection, m_maxGrappleDistance, m_WhatCanBeGrappled);

    	// check for raycast hit
    	if (hitInfo.collider != null) {
    		is_valid = true;
    		// set the line end to the point
    		lineEnd = hitInfo.point;
		} else {
			is_valid = false;
			// no hit, set the line render to connect to the target, or the max distance it CAN traval.
			lineEnd = transform.position + Vector3.ClampMagnitude(rayDirection, m_maxGrappleDistance);
		}
		// check for background layer hit
		Collider2D hitCollider = Physics2D.OverlapCircle(target, grappleRadius, m_grappleAreas);
		if (hitCollider != null) {
			is_valid = true;
		}
		// if we have a hit...
		if (is_valid) {
			// start the hang
			HangStart(lineEnd);
		}
		grappleLine.enabled = true;

		// while the grapple holds, show the line
        while(m_wasHangHeld && timer < timeOut) {
        	// if the grapple was a miss...
        	if (!is_valid) {
	        	// increment the timer
	        	timer += Time.deltaTime;
	        }
        	// update the line renderer
        	grappleLine.SetPosition(0, transform.position);
			grappleLine.SetPosition(1, lineEnd);
			// wait for next frame
            yield return null;
        }
        // disable the line renderer
        grappleLine.enabled = false;
        // end the grapple and finish this coroutine
        HangEnd();
    }

    // struct (sorta like a class, but these are passed by value instead of reference)
    
    // NOTE: source logic:
    // https://stackoverflow.com/questions/52158432/how-to-iterate-through-a-structs-fields
    // this is to create an iterable storage for collision info in a more organized way.
    public struct CollisionInfo
    {
	    public enum TouchDirection { up, right, down, left }

	    public static bool[] touchList = { false, false, false, false };

	    public bool up {
	        get { return this[0]; }
	        set { this[0] = value; }
	    }

	    public bool right {
	        get { return this[1]; }
	        set { this[1] = value; }
	    }

	    public bool down {
	        get { return this[2]; }
	        set { this[2] = value; }
	    }

	    public bool left {
	        get { return this[3]; }
	        set { this[3] = value; }
	    }	    

	    public bool this[TouchDirection dirn] {
	        get { return this[(int)dirn]; }
	        set { this[(int)dirn] = value; }
	    }

	    public bool this[int dirn] {
	        get { return touchList[dirn]; }
	        set { touchList[dirn] = value; }
	    }

	    public bool any {
	        get { return this.up || this.right || this.down || this.left; }
	        // set does nothing
	        set {}
	    }
	}
}
