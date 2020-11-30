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
	[SerializeField] private LayerMask m_WhatCanBeGrappled;					    // A mask determining what can be grabbed by the character
	[SerializeField] public float m_maxGrappleDistance;							// Max distance the grapple can reach
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings

	// just for temporary visuals
	public LineRenderer grappleLine;

	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player is touching a ceiling
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;

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

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
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
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			// as long as the collision object is not THIS object, then we can proceed
			if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;
				if (!wasGrounded)
					OnLandEvent.Invoke();
			}
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
		if (m_Grounded || m_AirControl)
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
			//Vector3 targetVelocity = new Vector2(move.x * 10f, m_Rigidbody2D.velocity.y);
			// And then smoothing it out and applying it to the character
			// m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			// move the character with physics forces
			m_Rigidbody2D.AddForce(move); //, ForceMode2D.Impulse);

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
		if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
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


	// using coroutines (to avoid a lot of update calls), attempt to grapple to the given point.
    IEnumerator grappleToPoint (Vector2 target)
    {
    	// cast a ray from player position to the target
    	Vector2 rayDirection = target - (Vector2)transform.position;
    	Vector2 lineEnd;
    	bool is_valid;
    	float timer = 0f;
    	float timeOut = 0.5f; // for when the grapple misses, only show the line for a short time

    	// Just trying to see the direction im trowing the raycast in...
    	Debug.DrawRay(transform.position, rayDirection, Color.green, 0);

    	RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, rayDirection, m_maxGrappleDistance, m_WhatCanBeGrappled);

    	// if we have a hit...
    	if (hitInfo.collider != null) {
    		is_valid = true;
    		// set the line end to the point
    		lineEnd = hitInfo.point;
			// start the hang
			HangStart(hitInfo.point);
		} else {
			is_valid = false;
			// no hit, set the line render to connect to the target, or the max distance it CAN traval.
			lineEnd = transform.position + Vector3.ClampMagnitude(rayDirection, m_maxGrappleDistance);
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
}
