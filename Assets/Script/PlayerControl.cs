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
    public LayerMask wallMask;

    bool isSprint;
    bool isCrouch;
    bool isSlide;
    bool isWallrun;

    Vector3 move;
    Vector3 input;

    float speed;
    public float runSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float airSpeed;

    float gravity;
    public float normalGravity;
    public float wallRunGravity;
    Vector3 Yvelocity;

    Vector3 forwardDirection;
    float slideTimer;
    public float maxSlideTime;
    public float slideSpeedIncrease;
    public float slideSpeedDecrease;

    public float wallSpeedIncrease;
    public float wallSpeedDecrease;
    bool leftWall;
    bool rightWall;
    bool hasWallRun = false;
    RaycastHit leftWallHit;
    RaycastHit rightWallHit;
    Vector3 wallNormal;
    Vector3 lastWallNormal;

    public float jumpHeight;
    int multiJump;

    float startHeight;
    float crouchHeight = 0.5f;
    Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    Vector3 standingCenter = new Vector3(0, 0, 0);

    public Camera playerCamera;
    float normalFov;
    public float specialFov;
    public float cameraChangeTime;
    public float wallRunTilt;
    public float tilt;
    

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        startHeight = transform.localScale.y;
        normalFov = playerCamera.fieldOfView;
    }

    void IncreaseSpeed(float speedIncrease)
    {
        speed += speedIncrease;
    }

    void DecreaseSpeed(float speedDecrease)
    {
        speed -= speedDecrease * Time.deltaTime;
    }

    void CameraEfects()
    {
        float fov = isWallrun ? specialFov : isSlide ? specialFov : normalFov;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, cameraChangeTime * Time.deltaTime);

        if(isWallrun)
        {
            if(rightWall)
            {
                tilt = Mathf.Lerp(tilt, wallRunTilt, cameraChangeTime * Time.deltaTime);
            }
            if(leftWall)
            {
                tilt = Mathf.Lerp(tilt, -wallRunTilt, cameraChangeTime * Time.deltaTime);
            }
        }

        if(!isWallrun)
        {
            tilt = Mathf.Lerp(tilt, 0f, cameraChangeTime * Time.deltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();

        CheckWalls();

        if (isGrounded && !isSlide)
        {
            GroundMove();
        }
        else if (!isGrounded && !isWallrun)
        {
            AirMove();
        }
        else if (isSlide)
        {
            Sliding();
            DecreaseSpeed(slideSpeedDecrease);
            slideTimer -= 1f * Time.deltaTime;
            if(slideTimer < 0)
            {
                isSlide = false;
            }
        }
        else if(isWallrun)
        {
            WallRunMove();
            DecreaseSpeed(wallSpeedDecrease);
        }

        controller.Move(move * Time.deltaTime);
        checkGround();
        GravityExist();
        CameraEfects();
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
            hasWallRun = false;
        }
        else
        {
            //multiJump = 0;
        }
    }

    void Jump()
    {
        if(!isGrounded && !isWallrun)
        {
            multiJump -= 1;
        }
        else if(isWallrun)
        {
            ExitWallRun();
            IncreaseSpeed(wallSpeedIncrease);
        }
        Yvelocity.y = Mathf.Sqrt(jumpHeight * -2f * normalGravity);
    }

    void GravityExist()
    {
        gravity = isWallrun ? wallRunGravity : normalGravity;
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

        if(speed > runSpeed)
        {
            isSlide = true;
            forwardDirection = transform.forward;

            if(isGrounded)
            {
                IncreaseSpeed(slideSpeedIncrease);
            }
            slideTimer = maxSlideTime;
        }
    }

    void ExitCrouch()
    {
        controller.height = (startHeight * 2);
        controller.center = standingCenter;
        transform.localScale = new Vector3(transform.localScale.x, startHeight, transform.localScale.z);
        isCrouch = false;
        isSlide = false;
    }

    void Sliding()
    {
        move += forwardDirection;
        move = Vector3.ClampMagnitude(move, speed);
    }

    void CheckWalls()
    {
        leftWall = Physics.Raycast(transform.position, -transform.right, out leftWallHit, 0.7f, wallMask);
        rightWall = Physics.Raycast(transform.position, transform.right, out rightWallHit, 0.7f, wallMask);

        if ((rightWall || leftWall) && !isWallrun)
        {
            TestWallRun();
        }
        if ((!rightWall || !leftWall) && isWallrun)
        {
            ExitWallRun();
        }
    }

    void WallRun()
    {
        isWallrun = true;
        multiJump = 1;
        IncreaseSpeed(wallSpeedIncrease);
        Yvelocity = new Vector3(0f, 0f, 0f);

        forwardDirection = Vector3.Cross(wallNormal, Vector3.up);

        if(Vector3.Dot(forwardDirection, transform.forward) < 0)
        {
            forwardDirection = -forwardDirection;
        }

    }

    void ExitWallRun()
    {
        isWallrun = false;
        lastWallNormal = wallNormal;
    }

    void WallRunMove()
    {
        if(input.z > (forwardDirection.z -10f) && input.z < (forwardDirection.z + 10f))
        {
            move += forwardDirection;
        }
        else if (input.z < (forwardDirection.z - 10f) && input.z > (forwardDirection.z + 10f))
        {
            move.x += 0f;
            move.z += 0f;
            ExitWallRun();
        }

        move.x += input.x * airSpeed;

        move = Vector3.ClampMagnitude(move, speed);
    }

    void TestWallRun()
    {
        wallNormal = leftWall ? leftWallHit.normal : rightWallHit.normal;
        if(hasWallRun)
        {
            float wallAngle = Vector3.Angle(wallNormal, lastWallNormal);
            if (wallAngle > 15)
            {
                WallRun();
            }
        }
        else
        {
            WallRun();
            hasWallRun = true;
        }
    }
}
