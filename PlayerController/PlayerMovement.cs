using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public enum MovementState
    {
        StateBase,
        StateWallRun,
        StateClimeb,
        Dead
    }
    private StateMachine stateMachine;
    private Dictionary<MovementState, Istate> dicState = new Dictionary<MovementState, Istate>();

    [Header("---Reference---")]
    public Transform playerObj;
    public Transform orientation;
    private Rigidbody rigid;
    private CapsuleCollider col;

    [Header("---Input---")]
    float horizontalInput;
    float verticalInput;

    [Header("---Check Ground---")]
    public float playerHeight;
    public LayerMask groundMask;
    public bool grounded;

    [Header("---Check Wall---")]
    public float detectionLength;
    [Space(20f)]

    /* WallRun */
    public LayerMask wallMask;
    public bool wallLeft;
    public bool wallRight;
    protected internal RaycastHit leftWallhit;
    protected internal RaycastHit rightWallhit;
    public float minJumpHeight;
    
    [Space(20f)]
    
    /* Climb */
    public float sphereCastRadius;
    public float maxWallLookAngle;
    protected internal RaycastHit frontWallHit;
    private float wallLookAngle;
    public bool wallFront;

    public bool swinging;

    private void Awake()
    {
        TryGetComponent<Rigidbody>(out rigid);
        playerObj.TryGetComponent<CapsuleCollider>(out col);
    }

    private void Start()
    {
        rigid.freezeRotation = true;

        // 상태 설정
        Istate stateBase = GetComponent<StateBaseMovement>();
        Istate stateWallRun = GetComponent<StateWallRun>();
        Istate stateClimb = GetComponent<StateClimb>();

        // 상태 보관
        dicState.Add(MovementState.StateBase, stateBase);
        dicState.Add(MovementState.StateWallRun, stateWallRun);
        dicState.Add(MovementState.StateClimeb, stateClimb);

        //기본상태 설정
        stateMachine = new StateMachine(stateBase);
    }

    public void SetBaseState() => stateMachine.SetBaseState();
    public void SetLastState() => stateMachine.SetLastState();

    private void Update()
    {
        GroundCheck();
        WallCheck();
        KeyInput();
        StateHandler();
        TextStuff();

        stateMachine.DoOperateUpdate();
    }

    private void FixedUpdate()
    {
        stateMachine.DoOperateFixedUpdate();
    }

    private void KeyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight  * 0.5f + 0.2f, groundMask);
    }

    private void WallCheck()
    {
        // WallRun (left, right)
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, detectionLength, wallMask);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, detectionLength, wallMask);

        // Climb (front)
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, wallMask);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundMask);
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction, RaycastHit ray)
    {
        return Vector3.ProjectOnPlane(direction, ray.normal).normalized;
    }

    private void StateHandler()
    {
        // StateMode - Climb
        if (wallFront && verticalInput > 0 && wallLookAngle < maxWallLookAngle) {
            if (stateMachine.CurrentState != dicState[MovementState.StateClimeb]) {
                stateMachine.SetState(dicState[MovementState.StateClimeb]);
            }
        }

        // StateMode - WallRun
        else if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround()) {
            if (stateMachine.CurrentState != dicState[MovementState.StateWallRun]) {
                stateMachine.SetState(dicState[MovementState.StateWallRun]);
            }
        }

        // StateMode - Ground(Base)
        else {
            if (stateMachine.CurrentState != dicState[MovementState.StateBase]) {
                stateMachine.SetState(dicState[MovementState.StateBase]);
            }
        }
        
    }

    #region Debug
    [Header("---ETC---")]
    public Text text_mode;
    private void TextStuff()
    {
        text_mode.text = stateMachine.CurrentState.ToString();
    }
    #endregion
}
