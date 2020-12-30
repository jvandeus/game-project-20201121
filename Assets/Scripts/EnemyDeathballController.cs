using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeathballController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Can't think of anything I need this for
    }

    // Update is called once per frame
    void Update()
    {
        //Don't think I need this yet
    }

    private void OnTriggerEnter2D(Collider2D otherCollider2D)
    {
        //Since only a player will have a CharacterController2D component, then
        //This basically makes sure the trigger is caused by a player.
        //Could also turn off some layers for this collider to eliminate some
        //of the problem
        if (otherCollider2D.GetComponent<CharacterController2D>() != null)
        {
            otherCollider2D.GetComponent<CharacterController2D>().killPlayer();
        }
    }
}
