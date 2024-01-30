using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class FirstPersonCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform player;
    public Transform playerObj;
    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        player.rotation = Quaternion.Euler(0, yRotation, 0);
        playerObj.rotation = Quaternion.Euler(0, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}