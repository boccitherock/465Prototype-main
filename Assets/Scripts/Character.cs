using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Character : MonoBehaviour
{


    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;



    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;




    [Header("Keybinds")]

    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;



    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public LayerMask whatisPlatform;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    [Header("Platform Handling")]
    public KeyCode PlatCon = KeyCode.Mouse1;

    public GameObject currentPlatform = null;
    public bool isOnPlatform = false;
    public bool platty = false;




    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    Rigidbody rb2;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        platform,
        air
    }

    public bool sliding;



    RaycastHit hitInfo;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb2 = null;

        rb.freezeRotation = true;
    
        readyToJump = true;

        startYScale = transform.localScale.y;


    }

    private void Update()
    {

        if (Input.GetKeyUp(PlatCon) && currentPlatform != null)
        {
            if (rb.TryGetComponent<FixedJoint>(out FixedJoint joint))
            {
                Destroy(joint);
            }

            platty = false;
            currentPlatform = null;
            rb2 = null;
        }


        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.7f * transform.localScale.y, whatIsGround);

        if (Input.GetKeyDown(PlatCon))
        {



            isOnPlatform = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.7f * transform.localScale.y, whatisPlatform);
            if (isOnPlatform && Physics.Raycast(transform.position, Vector3.down, out hitInfo, playerHeight * 0.7f * transform.localScale.y))
            {
                rb.linearVelocity = Vector3.zero;

                currentPlatform = hitInfo.collider.gameObject;


                platty = true;

                rb2 = hitInfo.rigidbody;
                
                FixedJoint joint = rb.gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = rb2;

            }

            


        }


        



        MyInput();
        SpeedControl();
        StateHandler();
        
        if (grounded)
        {
            rb.linearDamping = groundDrag;
            Invoke(nameof(ResetJump), jumpCooldown);

        }
        else
        {
            rb.linearDamping = 0;
        }


    }


    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");



        // jump stuff
        if(Input.GetKey(jumpKey) && readyToJump)
        {
            readyToJump = false;
            
            Jump();

            
        }


        //crouching
        if(Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        //uncrouching
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        }

    }


    private void StateHandler()

    {

        // Mode - Platform
        if(platty)
        {
            state = MovementState.platform;
            desiredMoveSpeed = walkSpeed;

        }


        // Mode - Sliding
        else if(sliding)
        {
            state = MovementState.sliding;

            if(onSlope())
            {
                desiredMoveSpeed = slideSpeed;
            }

            else
            {
                desiredMoveSpeed = sprintSpeed;
            }

        }




        // Mode - Crouching
        else if(Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }




        // Mode - sprinting
        else if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        // Mode - Walking

        else if(grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        // check if desiredMoveSpeed has changed drastically

        if (MathF.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4 && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }

        else
        {
            moveSpeed = desiredMoveSpeed;
        }


            lastDesiredMoveSpeed = desiredMoveSpeed;

    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = MathF.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        //moveSpeed = desiredMoveSpeed;



    }


    private void MovePlayer()
    {
        // Calc movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
    

        if(currentPlatform != null)
        {
            // Okay so right now this is, for some reason, not applying force to the rigidbody
            // The rigidbody is not moving and the player just walks off. 
            // Need to:
            // 1. Make the platform move
            // 2. Make the player's position stay synced to the platform's position
            //
            // 3. Make the player unparent the platform when they disconnect- this new (null) version does work
            // It's just nor being called for some reason. If worse comes to worse, bind disconnecting from a platform to left click maybe?
            // The parent thing CAN solve problem 2, but to even test it we need 1 solved
            // 1 then 2 then 3



            moveDirection = transform.TransformDirection(moveDirection);


            rb2.AddForce(moveDirection.normalized * desiredMoveSpeed * 10f, ForceMode.Force);

        }

        else if (onSlope())
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * desiredMoveSpeed * 10f, ForceMode.Force);
        }



        // Apply force
        else if(grounded)
        {
            rb.AddForce(moveDirection.normalized * desiredMoveSpeed * 10f, ForceMode.Force);
        }

        // in air
        else if(!grounded)
        {
            rb.AddForce(moveDirection.normalized * desiredMoveSpeed * 10f * airMultiplier, ForceMode.Force);

        }


    }


    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > desiredMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * desiredMoveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }


    }

    private void Jump()
    {
        // Reset Y velocity

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }


    private void ResetJump()
    {
        readyToJump = true;
    }


    public bool onSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.7f * transform.localScale.y))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;

    }


    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }












}