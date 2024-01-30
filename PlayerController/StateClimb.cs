using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateClimb : MonoBehaviour, Istate
{
    public DetailState state;
    public enum DetailState
    {
        climbing,
        climbingLeft,
        climbingRight
    }

    [Header("References")]
    private PlayerMovement pm;
    private Transform orientation;
    private Rigidbody rigid;

    [Header("Input")]
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    private bool leftClimbng;
    private bool rightClimbng;

    [Header("Climbing")]
    private float moveSpeed;
    public float climbSpeed;
    public float climbSideSpeed;
    public float slidDrag;
    private bool previousGravity;
    private float previousDrag;
    private bool climbing;

    [Header("Jump")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

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
        climbing = true;

        previousGravity = rigid.useGravity;
        rigid.useGravity = false;

        previousDrag = rigid.drag;
        rigid.drag = 0;

        rigid.velocity = Vector3.zero; // 이전 좌우 값 방지
    }

    public void OperateUpdate()
    {
        KeyInput();
        StateHandler();
        Drag();
        TextStuff();
    }

    public void OperateFixedUpdate()
    {
        if (!climbing) return; 

        ClimbingMovement();
        SpeedControl();
    }

    private void KeyInput()
    {   
        // 키 입력
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // 왼쪽, 오른쪽
        leftClimbng = (horizontalInput == 1) ? true : false;
        rightClimbng = (horizontalInput == -1) ? true : false;

        if (Input.GetKeyDown(jumpKey)) ClimbJump();
    }

    private void StateHandler()
    {
        // Mode - Wallrunning Up Or Down
        if (leftClimbng || rightClimbng) {
            if (horizontalInput < 0)
                state = DetailState.climbingLeft;
            else 
                state = DetailState.climbingRight;
        }

        // Mode - Wallrunning
        else if (climbing) {
            state = DetailState.climbing;
            moveSpeed = climbSpeed;
        }
    }

    private void ClimbingMovement()
    {
        // 오를 때 움직임
        rigid.velocity = new Vector3 (rigid.velocity.x, moveSpeed, rigid.velocity.z);
        
        // 좌, 우 움직임
        moveDirection = orientation.right * horizontalInput;
        rigid.AddForce(moveDirection.normalized * climbSideSpeed * 100f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        //속도 제한- Climb => 사이드 따로
        
        Vector3 floatVel = new Vector3(0f, rigid.velocity.y, 0f);

        if (floatVel.magnitude > climbSideSpeed) {
            Vector3 limitedVel = floatVel.normalized * climbSideSpeed;
            rigid.velocity = new Vector3(limitedVel.x, rigid.velocity.y, limitedVel.z);
        }

        floatVel = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        
        if (floatVel.magnitude > moveSpeed) {
            Vector3 limitedVel = floatVel.normalized * moveSpeed;
            rigid.velocity = new Vector3(rigid.velocity.x, limitedVel.y, rigid.velocity.z);
        }
    }

    private void Drag()
    {
        if (rigid.velocity.x != 0f || rigid.velocity.z != 0f) 
            rigid.drag = slidDrag;
        else 
            rigid.drag = 0;
    }

    private void ClimbJump()
    {
        Vector3 forceToApply = transform.up * climbJumpUpForce + pm.frontWallHit.normal * climbJumpBackForce;

        rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
        rigid.AddForce(forceToApply, ForceMode.Impulse);
    }

    public void OperateExit()
    {
       climbing = false;

       rigid.useGravity = previousGravity;
       rigid.drag = previousDrag;
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
