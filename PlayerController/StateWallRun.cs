using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateWallRun : MonoBehaviour, Istate
{
    public DetailState state;
    public enum DetailState
    {
        wallrunning,
        wallrunningUp,
        wallrunningDown
    }

    [Header("References")]
    private PlayerMovement pm;
    private Transform orientation;
    private Rigidbody rigid;
    
    public CamManager cam;

    [Header("Input")]
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    private bool upRunning;
    private bool downRunning;

    [Header("Wallrunning")]
    private float moveSpeed;
    public float wallrunSpeed;
    public float wallSideSpeed;
    public bool wallrunning;

    [Header("Jump")]
    public float wallJumpUpForce;
    public float wallJumpSideForce;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("Exiting")]
    private bool exitingWall;

    [Header("---Keybinds---")]
    public KeyCode jumpKey = KeyCode.Space;

    private void Awake()
    {
        TryGetComponent<PlayerMovement>(out pm);
        TryGetComponent<Rigidbody>(out rigid);

        this.orientation = pm.orientation;
    }

    public void OperateEnter()
    {
        wallrunning = true; /////클라임 추가 시 수정필요

        if (exitingWall) pm.SetLastState();

        rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);

        // 카메라 이펙트
        cam.DoFov(50f);
        if (pm.wallLeft) cam.DoFTilt(-5f);
        if (pm.wallRight) cam.DoFTilt(5f);
    }

    public void OperateUpdate()
    {
        KeyInput();
        StateHandler();
        SpeedControl();
        TextStuff();
    }

    public void OperateFixedUpdate()
    {
        if (wallrunning) WallRunningMovement();
    }

    private void KeyInput()
    {   
        // 키 입력
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // 점프
        if (Input.GetKeyDown(jumpKey)) WallJump();
    }

    private void StateHandler()
    {
        // Mode - Wallrunning Up Or Down
        if (upRunning ^ downRunning) {
            if (upRunning)
                state = DetailState.wallrunningUp;
            else 
                state = DetailState.wallrunningDown;
        }

        // Mode - Wallrunning
        else if (wallrunning) {
            state = DetailState.wallrunning;
            moveSpeed = wallrunSpeed;
        }
    }

    private void WallRunningMovement()
    {
        rigid.useGravity = useGravity;

        // 약한 중력
        if (useGravity)
            rigid.AddForce (transform.up * gravityCounterForce, ForceMode.Force);

        // 벽 앞, 뒤 움직임
        Vector3 wallNormal = pm.wallRight ? pm.rightWallhit.normal : pm.leftWallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        float wallSideForward = wallSideSpeed;
        bool wallForwarding = false;

        // 벽 방향
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude) {
            wallForwarding = true;
            wallForward = -wallForward;
            wallSideForward = -wallSideForward;
        }

        rigid.AddForce(wallForward * moveSpeed * 10f, ForceMode.Force);
        
        upRunning = (horizontalInput == (wallForwarding ? 1 : -1)) ? true : false;
        downRunning = (horizontalInput ==  (wallForwarding ? -1 : 1)) ? true : false;

        // 벽 위, 아래 움직임
        if (upRunning)
            rigid.velocity = new Vector3(rigid.velocity.x, (wallForwarding ? -wallSideForward : wallSideForward), rigid.velocity.z);
        if (downRunning)
            rigid.velocity = new Vector3(rigid.velocity.x, (wallForwarding ? wallSideForward : -wallSideForward), rigid.velocity.z);
    }

    private void SpeedControl()
    {
        //속도 제한 - WallRun => All
        
        if (rigid.velocity.magnitude > moveSpeed)
                rigid.velocity = rigid.velocity.normalized * moveSpeed;
    }

    private void WallJump()
    { 
        exitingWall = true;

        Vector3 wallNormal = pm.wallRight ? pm.rightWallhit.normal : pm.leftWallhit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // Jump
        rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        rigid.AddForce(forceToApply, ForceMode.Impulse);
    }

    public void OperateExit()
    {
        exitingWall = false;
        wallrunning = false;

        // 카메라 이펙트
        cam.DoFov();
        cam.DoFTilt(0f);
    }

    #region Debug
    [Header("---ETC---")]
    public Text text_speed;
    public Text text_mode;
    private void TextStuff()
    {
        Vector3 flatVel = new Vector3(rigid.velocity.x, rigid.velocity.y, rigid.velocity.z);

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