using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Swing : MonoBehaviour
{
    [Header("References")]
    public List<LineRenderer> lineRenderers;
    public List<Transform> swingTips;
    public Transform cam;
    public Transform player;
    public LayerMask whatIsGrappleable;
    public PlayerMovement pm;

    [Header("Swinging")]
    public float maxSwingDistance;
    public List<Vector3> swingPoints;
    private List<SpringJoint> joints;

    private List<Vector3> currentGrapplePositions;

    [Header("OdmGear")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;

    public RaycastHit hit;

    [Header("Input")]
    float horizontalInput;
    float verticalInput;
    public KeyCode swingKey1 = KeyCode.Q;
    public KeyCode swingKey2 = KeyCode.E;
    public KeyCode reduceKey = KeyCode.Space;
    private bool reduceed;

    [Header("DualSwinging")]
    public int amountOfSwingPoints = 2;
    public List<bool> swingsActive;

    private void Start()
    {
        ListSetup();
    }

    private void ListSetup()
    {
        swingPoints = new List<Vector3>();
        joints = new List<SpringJoint>();

        swingsActive = new List<bool>();

        currentGrapplePositions = new List<Vector3>();

        for (int i = 0; i < amountOfSwingPoints; i++) {
            joints.Add(null);
            swingPoints.Add(Vector3.zero);
            swingsActive.Add(false);
            currentGrapplePositions.Add(Vector3.zero);
        }
    }

    private void Update()
    {
        MyInput();
        CheckForSwingPoints();

        if (joints[0] != null || joints[1] != null) OdmGearMovement();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(swingKey1)) StartSwing(0);
        if (Input.GetKeyDown(swingKey2)) StartSwing(1);
        
        if (Input.GetKeyUp(swingKey1)) StopSwing(0);
        if (Input.GetKeyUp(swingKey2)) StopSwing(1);

        reduceed = Input.GetKey(reduceKey);
    }

    private void CheckForSwingPoints()
    {   
        Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, whatIsGrappleable);
    }

    #region Swinging

    private void StartSwing(int swingIndex)
    {
        if (hit.point == Vector3.zero) return;

        pm.swinging = true;
        
        swingsActive[swingIndex] = true;

        swingPoints[swingIndex] = hit.point;
        joints[swingIndex] = player.gameObject.AddComponent<SpringJoint>();
        joints[swingIndex].autoConfigureConnectedAnchor = false;
        joints[swingIndex].connectedAnchor = swingPoints[swingIndex];

        float distanceFromPoint = Vector3.Distance(player.position, swingPoints[swingIndex]);

        joints[swingIndex].maxDistance = distanceFromPoint * 0.8f;
        joints[swingIndex].minDistance = distanceFromPoint * 0.25f;

        joints[swingIndex].spring = 15f;
        joints[swingIndex].damper = 7f;
        joints[swingIndex].massScale = 4.5f;

        currentGrapplePositions[swingIndex] = swingTips[swingIndex].position;
    }

    public void StopSwing(int swingIndex)
    {
        pm.swinging = false;

        swingsActive[swingIndex] = false;

        Destroy(joints[swingIndex]);
    }
    #endregion


    private Vector3 pullPoint;
    private void OdmGearMovement()
    {
        if (swingsActive[0] && !swingsActive[1]) pullPoint = swingPoints[0];
        if (swingsActive[1] && !swingsActive[0]) pullPoint = swingPoints[1];
        
        if (swingsActive[0] && swingsActive[1]) {
            Vector3 dirToGrapplePoint1 = swingPoints[1] - swingPoints[0];
            pullPoint = swingPoints[0] + dirToGrapplePoint1 * 0.5f;
        }

        // right
        if (horizontalInput == 1) rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        // left
        if (horizontalInput == -1) rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        // forward
        if (verticalInput == 1) rb.AddForce(orientation.forward * forwardThrustForce * Time.deltaTime);

        if (reduceed) {
            Vector3 directionToPoint = pullPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, pullPoint);

            UpdateJoints(distanceFromPoint);
        }

        if (verticalInput == -1) {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, pullPoint) + extendCableSpeed;

            UpdateJoints(extendedDistanceFromPoint);
        }
    }

    private void UpdateJoints(float distanceFromPoint)
    {
        for (int i = 0; i < joints.Count; i++) {
            if (joints[i] != null) {
                joints[i].maxDistance = distanceFromPoint * 0.8f;
                joints[i].minDistance = distanceFromPoint * 0.25f;
            }
        }
    }
}