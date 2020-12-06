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

    // Start is called before the first frame update
    void Start()
    {
        moveDir = 1f;

        //moveVector = transform.up;

        moveVector = Quaternion.AngleAxis(initialMoveDirection, Vector3.forward) * Vector3.right;

        InvokeRepeating("flipVelocity", reverseTime, reverseTime);
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(moveVector * moveDir * moveSpeed * Time.deltaTime);
    }

    void flipVelocity()
    {
        moveDir = -1 * moveDir;
    }
}
