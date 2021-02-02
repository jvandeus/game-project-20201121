using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OoomfBoxComponent : MonoBehaviour
{

    public float targetVelocity;
    private float launchDir;
    private Vector2 launchVector;

    // Start is called before the first frame update
    void Start()
    {
        //launchDir = transform.up.magnitude + transform.eulerAngles[2] + 90; //Z Euler Angle, +90 degrees so that it's up and not right.
        //launchVector = new Vector2(Mathf.Cos(launchDir), Mathf.Sin(launchDir));

        launchVector = transform.up;
        //launchVector.
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   
      private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        
        Vector2 initVel = otherCollider.attachedRigidbody.velocity;
        Vector2 appliedImpulse = new Vector2(0, 0);

        //Get initial velocity component along final launch vector
        float initVelComponent = 0f;
        initVelComponent = Vector2.Dot(initVel, launchVector.normalized); //A dot B = |A||B|cos(theta).  For unit vector B, |B| = 1;

        //Clear existing velocity
        otherCollider.attachedRigidbody.velocity = new Vector2(0, 0);

        //If there was initially momentum in the launch direction, we'll want to add to it, so add it in
        if (initVelComponent > 0)
        {

            //I really liked being able to maintain some momentum...but it was too unpredictable.
            //Maybe we can maintain some X component...but not Y because we need all we can against gravity

            //Imp += m*v*unitVector
            //appliedImpulse += otherCollider.attachedRigidbody.mass * initVelComponent * launchVector.normalized;
        }

        // Add impulse to get desired final velocity
        appliedImpulse += otherCollider.attachedRigidbody.mass * targetVelocity * launchVector;

        //Apply the change in momentum as an impulse force
        otherCollider.attachedRigidbody.AddForce(appliedImpulse, ForceMode2D.Impulse);

        //A better approach may just be to do special removal of downward velocity.  Right now a 
        //straight up oomf box removes all horizonal momentum, which doesn't feel great.


        //Alternative, never exceeds a max velocity



    }
}
