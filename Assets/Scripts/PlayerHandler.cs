using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{

    Rigidbody2D playerRigidBody;
    bool isHanging = false;  //Is player hanging right now?
    HingeJoint2D playerHanger;  //Tether that we create and destroy depending on player input
    GameObject grappleVariable;


    public GameObject playerGrappleHook;

    // Start is called before the first frame update
    void Start()
    {
        playerRigidBody = GetComponent<Rigidbody2D>();
        //Instantiate(playerGrappleHook, new Vector3(0, 0, 0), Quaternion.identity);//, desiredAnchorWorld, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        //float horizontalInput2 = Input.GetAxis("Horizontal");

        float inputForceScale = 5;
        float verticalForceScale = 12; //For debugging, don't think we'll really want a vertical force in the end result.
        //Define the speed at which the object moves.

        float horizontalInput = Input.GetAxis("Horizontal");
        //Get the value of the Horizontal input axis.

        float verticalInput = Input.GetAxis("Vertical");
        //Get the value of the Vertical input axis.

        //transform.Translate(new Vector3(horizontalInput2, 0, verticalInput) * moveSpeed * Time.deltaTime);
        //Move the object to XYZ coordinates defined as horizontalInput, 0, and verticalInput respectively.

        playerRigidBody.AddForce(new Vector2(horizontalInput, 0) * inputForceScale);
        playerRigidBody.AddForce(new Vector2(0, verticalInput) * verticalForceScale);  //Could probably combine into one, but since this one is temporary I have them separable


        //Handle Tether
        bool hangingInput = Input.GetButton("Fire1");

        if (hangingInput && !isHanging)
        {
            //For creating the hingejoint component on player.
            playerHanger = gameObject.AddComponent(typeof(HingeJoint2D)) as HingeJoint2D;
            float anchorR = 2; //length of hingeJoint arm
            //For straight up, we just need to add anchorR to the y coordinate of player position (world)
            Vector3 playerPos = transform.position; //World coordinates.  Transform.localPosition gives position in parent transform coordinates.
            Vector3 desiredAnchorWorld = new Vector3(playerPos[0], playerPos[1] + anchorR, playerPos[2]); //Is there a way to do this without new? or without the temporary variable?
            Vector3 desiredAnchorLocal = transform.InverseTransformPoint(desiredAnchorWorld); //transform from world to local
            playerHanger.anchor = new Vector2(desiredAnchorLocal[0], desiredAnchorLocal[1]); //hingeJoint2D's anchor only wants Vector2
            playerHanger.enableCollision = true;  //False by default?  Preposterous. 

            //For creating a sprite for the hook.  Rope between hook and player will come later
            //Should look into doing this as a "prefab", but for now, piecemeal
            grappleVariable = Instantiate(playerGrappleHook, desiredAnchorWorld, Quaternion.identity);
            //Instantiate(playerGrappleHook, desiredAnchorWorld, Quaternion.identity);


            isHanging = true;
        }
        else if (!hangingInput && isHanging)
        {
            //Destory HingeJoint2D component on player
            Destroy(playerHanger);
            Destroy(grappleVariable);
            //Destroy(playerGrappleHook);
            isHanging = false;
        }
    }
}
