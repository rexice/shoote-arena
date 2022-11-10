using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    CharacterController controller;

    [Header("Camera")]
    public Camera playerCamera;
    float normalFov;
    public float specialFov;
    public float cameraChangeTime;
    public float wallRunTilt;
    public float tilt;

    [Header("Movement")]
    public float runSpeed;
    public float sprintSpeed;
    Vector3 move;
    Vector3 input;
    float speed;
    bool isSprint;

    [Header("Checks")]
    public LayerMask groundMask;
    public LayerMask wallMask;
    public Transform groundCheck;
    public float checkRange = 0.2f;
    bool isGrounded;

    [Header("Jump")]
    public float normalGravity;
    public float wallRunGravity;
    float gravity;
    Vector3 Yvelocity;
    public float jumpHeight;
    int multiJump;
    public float airSpeed;

    [Header("Crouch")]
    public float crouchSpeed;
    bool isCrouch;
    float startHeight;
    float crouchHeight = 0.5f;
    Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    Vector3 standingCenter = new Vector3(0, 0, 0);

    [Header("Slide")]
    public float maxSlideTime;
    Vector3 forwardDirection;
    bool isSlide;
    float slideTimer;
    public float slideSpeedIncrease;
    public float slideSpeedDecrease;

    [Header("Wallrun")]
    bool isWallrun;
    public float wallSpeedIncrease;
    public float wallSpeedDecrease;
    bool leftWall;
    bool rightWall;
    bool hasWallRun = false;
    RaycastHit leftWallHit;
    RaycastHit rightWallHit;
    Vector3 wallNormal;
    Vector3 lastWallNormal;

    [Header("Wall Jump")]
    bool isWallJump;
    float wallJumpTimer;
    public float maxWalljumpTimer;

    [Header("Climbing")]
    public float climbSpeed;
    bool isClimb;
    bool canClimb;
    bool hasClimb;
    RaycastHit wallHit;
    float climbTimer;
    public float maxClimbTimer;

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

    // Update is called once per frame
    void Update()
    {
        HandleInput();

        CheckWalls();

        CheckClimb();

        if (isGrounded && !isSlide)
        {
            GroundMove();
        }
        else if (!isGrounded && !isWallrun && !isClimb)
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
        else if (isClimb)
        {
            ClimbMove();
            climbTimer -= 1f * Time.deltaTime;
            if(climbTimer < 0)
            {
                isClimb = false;
                hasClimb = true;
            }
        }

        controller.Move(move * Time.deltaTime);
        GravityExist();
        CameraEfects();
    }

    void FixedUpdate()
    {
        checkGround();
    }

    void HandleInput()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        if (!isWallrun)
        {
            input = transform.TransformDirection(input);
            input = Vector3.ClampMagnitude(input, 1f);
        }

        if (Input.GetKeyDown(KeyCode.Space) && multiJump > 0)
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
            hasClimb = false;
            climbTimer = maxClimbTimer;
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
        }
        Yvelocity.y = Mathf.Sqrt(jumpHeight * -2f * normalGravity);

        hasClimb = false;
        climbTimer = maxClimbTimer;
    }

    void GravityExist()
    {
        gravity = isWallrun ? wallRunGravity : isClimb ? 0f : normalGravity;
        Yvelocity.y += gravity * Time.deltaTime;
        controller.Move(Yvelocity * Time.deltaTime);
    }

    void AirMove()
    {
        move.x += input.x * airSpeed;
        move.z += input.z * airSpeed;

        if(isWallJump)
        {
            move += forwardDirection * airSpeed;
            wallJumpTimer -= 1f * Time.deltaTime;
            if (wallJumpTimer <= 0)
            {
                isWallJump = false;
            }
        }

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
        rightWall = Physics.Raycast(transform.position, transform.right, out rightWallHit, 0.7f, wallMask);
        leftWall = Physics.Raycast(transform.position, -transform.right, out leftWallHit, 0.7f, wallMask);

        if ((rightWall || leftWall) && !isWallrun)
        {
            TestWallRun();
        }
        if ((!rightWall || !leftWall) && isWallrun)
        {
            ExitWallRun();
        }
    }

    void ExitWallRun()
    {
        isWallrun = false;
        lastWallNormal = wallNormal;
        IncreaseSpeed(wallSpeedIncrease);
        forwardDirection = wallNormal;
        isWallJump = true;
        wallJumpTimer = maxWalljumpTimer;
        //tilt = Mathf.Lerp(tilt, 0f, cameraChangeTime * Time.deltaTime);
    }

    void WallRunMove()
    {
        if(input.z > (forwardDirection.z -10f) && input.z < (forwardDirection.z + 10f))
        {
            move += forwardDirection;
        }
        else if (input.z < (forwardDirection.z - 10f) && input.z > (forwardDirection.z + 10f))
        {
            move.x += 0;
            move.z += 0;
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
    void WallRun()
    {
        isWallrun = true;
        multiJump = 1;
        IncreaseSpeed(wallSpeedIncrease);
        Yvelocity = new Vector3(0f, 0f, 0f);

        forwardDirection = Vector3.Cross(wallNormal, Vector3.up);

        if (Vector3.Dot(forwardDirection, transform.forward) < 0)
        {
            forwardDirection = -forwardDirection;
        }

    }

    void CheckClimb()
    {
        canClimb = Physics.Raycast(transform.position, transform.forward, out wallHit, 0.7f, wallMask);
        float wallAngle = Vector3.Angle(-wallHit.normal, transform.forward);
        if(wallAngle < 15 && !hasClimb && canClimb)
        {
            isClimb = true;
        }
        else
        {
            isClimb = false;
        }
    }

    void ClimbMove()
    {
        forwardDirection = Vector3.up;
        move.x += input.x * airSpeed;
        move.z += input.z * airSpeed;

        Yvelocity += forwardDirection;
        speed = climbSpeed;

        move = Vector3.ClampMagnitude(move, speed);
        Yvelocity = Vector3.ClampMagnitude(Yvelocity, speed);
    }

    void CameraEfects()
    {
        float fov = isWallrun ? specialFov : isSlide ? specialFov : normalFov;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, cameraChangeTime * Time.deltaTime);

        /*if (isWallrun)
        {
            if (rightWall)
            {
                tilt = Mathf.Lerp(tilt, wallRunTilt, cameraChangeTime * Time.deltaTime);
            }
            if (leftWall)
            {
                tilt = Mathf.Lerp(tilt, -wallRunTilt, cameraChangeTime * Time.deltaTime);
            }
        }*/

        if (rightWall)
        {
            tilt = Mathf.Lerp(tilt, wallRunTilt, cameraChangeTime * Time.deltaTime);
        }
        if (leftWall)
        {
            tilt = Mathf.Lerp(tilt, -wallRunTilt, cameraChangeTime * Time.deltaTime);
        }
        else
        {
            tilt = Mathf.Lerp(tilt, 0f, cameraChangeTime * Time.deltaTime);
        }

        /*if (!isWallrun)
        {
            tilt = Mathf.Lerp(tilt, 0f, cameraChangeTime * Time.deltaTime);
        }*/
    }
}
