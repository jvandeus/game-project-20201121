using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blockBackAndForth_automove : MonoBehaviour
{

    public float initialMoveDirection; //Define the direction that the block moves from starting position, in degrees, counter clockwise from +x (right)
    public float moveSpeed; //Define speed of block as it moves
    public float reverseTime; //How long before block reverses direction

    private float moveDir;
    private Vector3 moveVector;
    Rigidbody2D myRigidBody;
    Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        moveDir = 1f;

        //moveVector = transform.up;

        moveVector = Quaternion.AngleAxis(initialMoveDirection, Vector3.forward) * Vector3.right;
        myRigidBody = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;

        myRigidBody.AddForce(moveVector * myRigidBody.mass * moveSpeed * moveDir, ForceMode2D.Impulse);

        InvokeRepeating("flipVelocity", reverseTime, reverseTime);
        InvokeRepeating("resetPosition", 2 * reverseTime, 2 * reverseTime);
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Works, but bypasses physics.  Comment out forces and uncomment this to test
        //transform.Translate(moveVector * moveDir * moveSpeed * Time.deltaTime);
    }

    void flipVelocity()
    {
        moveDir = -1 * moveDir;
        myRigidBody.AddForce(2 * moveVector * myRigidBody.mass * moveSpeed * moveDir, ForceMode2D.Impulse);

    }

    //Although the mass is large, technically it can be pulled away if you wait long enough
    //This function is a bandaid to prevent this, but putting it back where it started every time
    //the cycle repeats.  This technically cheats the physics but is neglegible.
    void resetPosition() 
    {
        transform.position = initialPosition;
    }
}
