// created from brackeys tutorial code, modified with other code in mind. https://www.youtube.com/watch?v=dwcT-Dch0bA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerInput: MonoBehaviour {

    private CharacterController2D controller;

    //public float moveSpeed = 40f;
    //Since we're adding a force, we don't set a movespeed, we set a force
    public float horiMoveForce;
    public Vector2 directionalInput = Vector2.zero;

    // float horizontalMove = 0f;
    bool jump = false;
    bool hang = false;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController2D>();
        //horiMoveForce = 10;
    }
    
    // Update is called once per frame
    void Update () {
        //directionalInput = new Vector2(Input.GetAxisRaw("Horizontal") * moveSpeed, Input.GetAxisRaw("Vertical"));
        directionalInput = new Vector2(Input.GetAxisRaw("Horizontal") * horiMoveForce, Input.GetAxisRaw("Vertical"));
        //directionalInput = new Vector2(Input.GetAxisRaw("Horizontal") * 10, Input.GetAxisRaw("Vertical"));

        // only care about the button press
        if (Input.GetButtonDown("Jump")) {
            jump = true;
        }

        // holdable input
        if (Input.GetButtonDown("Fire1")) {
            hang = true;
        } else if (Input.GetButtonUp("Fire1")) {
            hang = false;
        }

    }

    void FixedUpdate ()
    {
        // Move our character
        //controller.Move(directionalInput * Time.fixedDeltaTime, jump, hang);
        controller.Move(directionalInput, jump, hang); //Force should not be multiplied by time
        // reset the jump
        jump = false;
    }
}