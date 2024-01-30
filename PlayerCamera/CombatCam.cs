using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCam : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform playerObj;
    public Transform orientation;

    public Transform combatLookAt;

    private void OnEnable()
    {
        
    }

    private void Update()
    {
        // rotation orientation
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
        orientation.forward = dirToCombatLookAt.normalized;

        playerObj.forward = dirToCombatLookAt.normalized;
    }
}
