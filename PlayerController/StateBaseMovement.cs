using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StateBaseMovement : MonoBehaviour, Istate
{
    public DetailState state;
    public enum DetailState
    {
        walking,
        sprinting,
        crouching,
        swinging,
        air,
    }

    [Header("---Reference---")]
    private PlayerMovement pm;
    private Transform playerObj;
    private Transform orientation;
    private Rigidbody rigid;

    [Header("---Input---")]
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;

    [Header("---Movement---")]
    private Coroutine smoothlyLerpMoveSpeed;
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float swingSpeed;
    public float groundDrag;
    public float airMultiplier;
    public float gravityForce;

    [Header("---Jump---")]
    public float jumpForce;
    public float jumpColldown;
    public bool readyToJump;

    [Header("---Crouching---")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    private bool isCrouch;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("---Keybinds---")]
    public KeyCode sprintkey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;

    public void Awake()
    {
        TryGetComponent<PlayerMovement>(out pm);
        this.playerObj = pm.playerObj;
        this.orientation = pm.orientation;

        TryGetComponent<Rigidbody>(out rigid);
    }

    private void Start()
    {
        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    public void OperateEnter()
    {
        
    }

    public void OperateExit()
    {
        
    }

    public void OperateUpdate()
    {
        KeyInput();
        StateHandler();
        SpeedControl();
        Drag();
        TextStuff();
    }
    public void OperateFixedUpdate()
    {
        MovePlayer();
    }

    private void KeyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey) && readyToJump && pm.grounded) Jump();

        if (Input.GetKeyDown(crouchKey) && pm.grounded) {
            isCrouch = true;
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rigid.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(crouchKey)) {
            isCrouch = false;
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        // Mode - Crouching
        if (isCrouch && pm.grounded) {
            state = DetailState.crouching;
            moveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (Input.GetKey(sprintkey) && pm.grounded) {
            state = DetailState.sprinting;
            moveSpeed = sprintSpeed;
        }
        
        // Mode - walking
        else if (pm.grounded) {
            state = DetailState.walking;
            moveSpeed = walkSpeed;
        }

        // Mode = Swinging
        else if (pm.swinging){
            state = DetailState.swinging;
            moveSpeed = swingSpeed;
        }

        // Mode = Air
        else {
            state = DetailState.air;
            moveSpeed = walkSpeed;
        }
    }

    #region Movement
    private void MovePlayer()
    {
        if (pm.swinging) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        // OnSlope
        if (OnSlope() && !exitingSlope) {
            rigid.AddForce(pm.GetSlopeMoveDirection(moveDirection, slopeHit) * moveSpeed * 20f, ForceMode.Force);

            // 경사면 중력 보완
            rigid.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // 경사면 중력 제거
        rigid.useGravity = !OnSlope();

        // Ground
        if (pm.grounded) {
            rigid.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
            
        // Air
        else if (!pm.grounded) {
            rigid.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            rigid.AddForce(-transform.up * gravityForce * 10f, ForceMode.Force);
        }
            
    }

    private void SpeedControl()
    {
        // 경사면 속도 제한
        if (OnSlope() && !exitingSlope) {
            if (rigid.velocity.magnitude > moveSpeed)
                rigid.velocity = rigid.velocity.normalized * moveSpeed;
        }
        //속도 제한 - WallRun => Y
        else {
            Vector3 floatVel = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);

            if (floatVel.magnitude > moveSpeed) {
                Vector3 limitedVel = floatVel.normalized * moveSpeed;
                rigid.velocity = new Vector3(limitedVel.x, rigid.velocity.y, limitedVel.z);
            }
        }
    }

    private void Drag()
    {
        if (pm.grounded) 
            rigid.drag = groundDrag;
        else 
            rigid.drag = 0;
    }
    #endregion

    #region Jump
    private void Jump()
    {
        exitingSlope = true;

        // coolDown
        readyToJump = false;
        Invoke(nameof(ResetJump), jumpColldown);

        rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        rigid.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }
    #endregion

    #region Slope
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, pm.playerHeight * 0.5f + 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
    #endregion

    #region Debug
    [Header("---ETC---")]
    public Text text_speed;
    public Text text_mode;
    private void TextStuff()
    {
        Vector3 flatVel = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);

        text_speed.text = "Speed: " + Round(flatVel.magnitude, 1) + " / " + Round(moveSpeed, 1);

        text_mode.text = state.ToString();
    }
    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }
    #endregion
}