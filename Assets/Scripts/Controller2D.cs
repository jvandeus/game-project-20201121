using UnityEngine;

// if we want to extend off a base class, we can do so. I coped this from code where we might not need that, so i removed it.
// public class Controller2D : RaycastController
public class Controller2D : MonoBehaviour
{
    // custom struct to hold basic collision info
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public Vector2 slopeNormal;
        public int faceDir;

        public void Reset()
        {
            above = below = false;
            left = right = false;

            slopeNormal = Vector2.zero;
        }
    }

    // new variable for the information above
    public CollisionInfo collisions;
    [HideInInspector]
    public Vector2 objectInput;

    // NOTE: because we aren't extending, we don't need the "override" or the base.Start yet. If we do, thas how we get the parent to run, THEN this one.
    // public override void Start()
    public void Start()
    {
        // Calls start for the parent class
        // NOTE: because we aren't extending, we don't need this yet
        // base.Start();
        collisions.faceDir = 1;

    }

    public void Move(Vector2 moveAmount, bool isGrounded)
    {
        Move(moveAmount, Vector2.zero, isGrounded);
    }

    public void Move(Vector2 moveAmount, Vector2 input, bool isGrounded = false)
    {
        collisions.Reset();
        objectInput = input;

        if (moveAmount.x != 0)
        {
            collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
        }

        // pass the movement magnitude by reference into collision controller function(s)
        HorizontalCollisions(ref moveAmount);
        if (moveAmount.y != 0)
        {
            VerticalCollisions(ref moveAmount);
        }

        // with the movement now adjusted in the collision controllers, we move this object.
        transform.Translate(moveAmount);

        if (isGrounded)
        {
            collisions.below = true;
        }
    }

    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = collisions.faceDir;
        // TODO: calculate all collisions and such. Maybe merge into one collision function
    }

    void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        // TODO: calculate all collisions and such. Maybe merge into one collision function
    }

}
