// created from brackeys tutorial code, modified with other code in mind. https://www.youtube.com/watch?v=dwcT-Dch0bA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerInput: MonoBehaviour {

    public CharacterController2D controller;

    public float moveSpeed = 40f;

    float horizontalMove = 0f;
    bool jump = false;
    bool hang = false;

    
    // Update is called once per frame
    void Update () {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal") * moveSpeed, Input.GetAxisRaw("Vertical"));

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
        controller.Move(directionalInput * Time.fixedDeltaTime, jump, hang);
        // reset the jump
        jump = false;
    }
}