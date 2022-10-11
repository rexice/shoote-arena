using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    CharacterController controller;

    public Transform groundCheck;
    public float checkRange = 0.2f;
    bool isGrounded;
    public LayerMask groundMask;

    Vector3 move;
    Vector3 input;

    float speed;
    public float runSpeed;

    float gravity;
    public float normalGravity;
    Vector3 Yvelocity;

    public float jumpHeight;
    int multiJump;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        GroundMove();
        controller.Move(move * Time.deltaTime);
        checkGround();
        GravityExist();
    }

    void HandleInput()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        input = transform.TransformDirection(input);
        input = Vector3.ClampMagnitude(input, 1f);

        if(Input.GetKeyUp(KeyCode.Space) && multiJump > 0)
        {
            Jump();
        }
    }

    void GroundMove()
    {
        speed = runSpeed;
        if(input.x !=0)
        {
            move.x += input.x * speed;
        }
        else
        {
            move.x = 0;
        }
        if(input.z !=0)
        {
            move.z += input.z * speed;
        }
        else
        {
            move.z = 0;
        }

        move = Vector3.ClampMagnitude(move, speed);
    }

    void checkGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, checkRange, groundMask);
        if(isGrounded)
        {
            multiJump = 1;
        }
    }

    void Jump()
    {
        Yvelocity.y = Mathf.Sqrt(jumpHeight * -2f * normalGravity);
    }

    void GravityExist()
    {
        gravity = normalGravity;
        Yvelocity.y += gravity * Time.deltaTime;
        controller.Move(Yvelocity * Time.deltaTime);
    }
}
