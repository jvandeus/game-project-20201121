using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{

    Rigidbody2D playerRigidBody;
    bool isHanging = false;
    HingeJoint2D playerHanger;

    // Start is called before the first frame update
    void Start()
    {
        playerRigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //float horizontalInput2 = Input.GetAxis("Horizontal");

        float inputForceScale = 2;
        float verticalForceScale = 12;
        //Define the speed at which the object moves.

        float horizontalInput = Input.GetAxis("Horizontal");
        //Get the value of the Horizontal input axis.

        float verticalInput = Input.GetAxis("Vertical");
        //Get the value of the Vertical input axis.

        //transform.Translate(new Vector3(horizontalInput2, 0, verticalInput) * moveSpeed * Time.deltaTime);
        //Move the object to XYZ coordinates defined as horizontalInput, 0, and verticalInput respectively.

        playerRigidBody.AddForce(new Vector2(horizontalInput, 0) * inputForceScale);
        playerRigidBody.AddForce(new Vector2(0, verticalInput) * verticalForceScale);


        //Handle Tether
        bool hangingInput = Input.GetButton("Fire1");

        if (hangingInput && !isHanging)
        {
            playerHanger = gameObject.AddComponent(typeof(HingeJoint2D)) as HingeJoint2D;
            //Vector3 playerPos = transform.position;
            //playerHanger.anchor = new Vector2(0, 2);

            //For now, I always want the anchor directed straight up, so I have to rotate
            float anchorR = 2;
            //double anchorAngle = 0;

            Vector3 playerPos = transform.position;


            playerHanger.anchor = new Vector2(yCoord, xCoord);


            playerHanger.enableCollision = true;
            //transform.position
            isHanging = true;
        }
        else if (!hangingInput && isHanging)
        {
            Destroy(playerHanger);
            isHanging = false;
        }
    }
}
