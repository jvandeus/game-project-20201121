using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointHandler : MonoBehaviour
{
    private Renderer rend;
    public Color activeColor;
    public Color inactiveColor;

    // Start is called before the first frame update
    void Start()
    {
        //Think this will be empty, should probably delete
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //Think this will be empty, should probably delete
    }

    public void setAsPlayerActiveCheckpoint()
    {
        rend.material.color = activeColor;
    }

    public void setAsInctiveCheckpoint()
    {
        rend.material.color = inactiveColor;
    }

    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        //Since only a player will have a CharacterController2D component, then
        //This basically makes sure the trigger is caused by a player.
        //Could also turn off some layers for this collider to eliminate some
        //of the problem
        if (otherCollider.GetComponent<CharacterController2D>() != null)
        {
            //Debug.Log("Collision Worked");

            //First, we need to grab the player's CharacterController2D component
            CharacterController2D playerController = otherCollider.GetComponent<CharacterController2D>();

            //Next, we need to tell the old checkpoint it's not longer the current checkpoint
            playerController.activeCheckpoint.GetComponent<CheckpointHandler>().setAsInctiveCheckpoint();

            //Okay, now it's time to tell the player that it has a new checkpoint
            playerController.activeCheckpoint = this.gameObject;
            
            //Finally, tell this checkpoint it's the current checkpoint
            setAsPlayerActiveCheckpoint();
        }

    }

}
