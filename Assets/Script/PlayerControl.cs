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

    bool isSprint;
    bool isCrouch;

    Vector3 move;
    Vector3 input;

    float speed;
    public float runSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float airSpeed;

    float gravity;
    public float normalGravity;
    Vector3 Yvelocity;

    public float jumpHeight;
    int multiJump;

    float startHeight;
    float crouchHeight = 0.5f;
    Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    Vector3 standingCenter = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        startHeight = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();

        if (isGrounded)
        {
            GroundMove();
        }
        else
        {
            AirMove();
        }

        controller.Move(move * Time.deltaTime);
        checkGround();
        GravityExist();
    }

    void HandleInput()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        input = transform.TransformDirection(input);
        input = Vector3.ClampMagnitude(input, 1f);

        if(Input.GetKeyDown(KeyCode.Space) && multiJump > 0)
        {
            Jump();
        }

        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
        if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            ExitCrouch();
        }

        if(Input.GetKeyDown(KeyCode.LeftShift) && isGrounded)
        {
            isSprint = true;
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprint = false;
        }
    }

    void GroundMove()
    {
        speed = isSprint ? sprintSpeed : isCrouch ? crouchSpeed : runSpeed;
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
        else
        {
            multiJump = 0;
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

    void AirMove()
    {
        move.x += input.x * airSpeed;
        move.z += input.z * airSpeed;

        move = Vector3.ClampMagnitude(move, speed);
    }

    void Crouch()
    {
        controller.height = crouchHeight;
        controller.center = crouchingCenter;
        transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
        isCrouch = true;
    }

    void ExitCrouch()
    {
        controller.height = (startHeight * 2);
        controller.center = standingCenter;
        transform.localScale = new Vector3(transform.localScale.x, startHeight, transform.localScale.z);
        isCrouch = false;
    }
}
