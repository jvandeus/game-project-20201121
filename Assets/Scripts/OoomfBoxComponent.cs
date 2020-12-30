using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OoomfBoxComponent : MonoBehaviour
{

    public float targetVelocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision Occured");

        Vector2 initVel = collision.otherRigidbody.velocity;

        Vector2 appliedImpulse = new Vector2(0, 0);

        //For now, assuming we always want straight up.  In the future we may want to generalize to any direction

        //If initial Y velocity is negative, we want the impulse to add enough force to overcome that, AND achieve the desired velocity
        if (initVel[1] < 0)  //Y component
        {
            appliedImpulse[1] += collision.otherRigidbody.mass * -1 * initVel[1];
        }
        appliedImpulse[1] += collision.otherRigidbody.mass * targetVelocity;

        collision.otherRigidbody.AddForce(appliedImpulse, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        
        Debug.Log("trigger Occured");


        
        Vector2 initVel = otherCollider.attachedRigidbody.velocity;

        Vector2 appliedImpulse = new Vector2(0, 0);

        //For now, assuming we always want straight up.  In the future we may want to generalize to any direction

        //If initial Y velocity is negative, we want the impulse to add enough force to overcome that, AND achieve the desired velocity
        if (initVel[1] < 0)  //Y component
        {
            appliedImpulse[1] += otherCollider.attachedRigidbody.mass * -1 * initVel[1];
        }

        //Add some impulse to achieve the desired additional impulse
        appliedImpulse[1] += otherCollider.attachedRigidbody.mass * targetVelocity;

        otherCollider.attachedRigidbody.AddForce(appliedImpulse, ForceMode2D.Impulse);
    }
}
