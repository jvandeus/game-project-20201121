// created from brackeys tutorial code, modified with other code in mind. https://www.youtube.com/watch?v=dwcT-Dch0bA
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
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings

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
	private bool m_wasHanging = false;
	private GameObject currentGrapple;
	public GameObject characterGrappleHook;

	public float anchorR;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		if (!characterHanger ) {
			characterHanger = gameObject.AddComponent(typeof(HingeJoint2D)) as HingeJoint2D;
			characterHanger.enabled = false;
		}

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnHangEvent == null)
			OnHangEvent = new BoolEvent();
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

	public void Move(Vector2 move, bool jump, bool hang)
	{
		// Check if the character is touching a ceiling
		// NOTE: not using this yet, just keeping it here for notes.
		// if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround)) { // ... do something }

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{
			// Update the hanging states and event based on the input.
			if (hang) {
				if (!m_wasHanging) {
					m_wasHanging = true;
					OnHangEvent.Invoke(true);
					HangStart();
				}
			} else {
				if (m_wasHanging) {
					m_wasHanging = false;
					OnHangEvent.Invoke(false);
					HangEnd();
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

	public void HangStart()
	{
		characterHanger.enabled = true;
        //float anchorR = 2; //length of hingeJoint arm
        //For straight up, we just need to add anchorR to the y coordinate of player position (world)
        Vector3 playerPos = transform.position; //World coordinates.  Transform.localPosition gives position in parent transform coordinates.
        Vector3 desiredAnchorWorld = new Vector3(playerPos[0], playerPos[1] + anchorR, playerPos[2]); //Is there a way to do this without new? or without the temporary variable?
        Vector3 desiredAnchorLocal = transform.InverseTransformPoint(desiredAnchorWorld); //transform from world to local
        characterHanger.anchor = new Vector2(desiredAnchorLocal[0], desiredAnchorLocal[1]); //hingeJoint2D's anchor only wants Vector2
        characterHanger.enableCollision = true;  //False by default?  Preposterous. 

        //For creating a sprite for the hook.  Rope between hook and player will come later
        //Should look into doing this as a "prefab", but for now, piecemeal
        currentGrapple = Instantiate(characterGrappleHook, desiredAnchorWorld, Quaternion.identity);
	}

	public void HangEnd()
	{
		// Disable HingeJoint2D component on player
		characterHanger.enabled = false;
		// Destory The Prefab to the grapple sprite
        Destroy(currentGrapple);
	}


	//Testing a new direction for the hanger


}
