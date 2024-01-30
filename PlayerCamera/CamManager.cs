using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class CamManager : MonoBehaviour
{
    [Header("References")]
    public CameraStyle currentStyle;
    public enum CameraStyle{
        First,
        ThirdPerson,
        Topdown,
        Combat,
    }

    [Header("---Camera---")]
    public GameObject firstCam;
    public GameObject thirdPersonCam;
    public GameObject topDownCam;
    public GameObject combatCam;

    [Header("---Keybinds---")]
    public KeyCode firstCamKey = KeyCode.Alpha1;
    public KeyCode thirdPersonCamKey = KeyCode.Alpha2;
    public KeyCode topDownCamKey = KeyCode.Alpha3;
    public KeyCode combatCamKey = KeyCode.Alpha4;

    [Header("---FirstCam---")]
    CinemachineVirtualCamera firstCamCinema;
    private float defaultFOV = 0;
    Coroutine fovCoroutine;

    private void Awake()
    {
        firstCam.TryGetComponent<CinemachineVirtualCamera>(out firstCamCinema);
    } 

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        defaultFOV = firstCamCinema.m_Lens.FieldOfView;

        SwitchCameraStyle(CameraStyle.First);
    }

    private void Update()
    {
        if (Input.GetKeyDown(firstCamKey)) SwitchCameraStyle(CameraStyle.First);
        if (Input.GetKeyDown(thirdPersonCamKey)) SwitchCameraStyle(CameraStyle.ThirdPerson);
        if (Input.GetKeyDown(topDownCamKey)) SwitchCameraStyle(CameraStyle.Topdown);
        if (Input.GetKeyDown(combatCamKey)) SwitchCameraStyle(CameraStyle.Combat);
    }

    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        firstCam.SetActive(false);
        thirdPersonCam.SetActive(false);
        topDownCam.SetActive(false);
        combatCam.SetActive(false);

        switch(newStyle) {
            case CameraStyle.First:
                firstCam.SetActive(true);
                break;
            case CameraStyle.ThirdPerson:
                thirdPersonCam.SetActive(true);
                break;
            case CameraStyle.Topdown:
                topDownCam.SetActive(true);
                break;
            case CameraStyle.Combat:
                combatCam.SetActive(true);
                break;
        }
        currentStyle = newStyle;
    }

    public void DoFov(float endValue = -1f)
    {
        if (fovCoroutine != null) StopCoroutine(fovCoroutine);
        
        if (endValue == -1f)
            fovCoroutine = StartCoroutine(SmoothlyLerp_Fov(defaultFOV));
        else
            fovCoroutine = StartCoroutine(SmoothlyLerp_Fov(endValue));
    }

    public void DoFTilt(float zTilt)
    {
        firstCam.transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }

    private IEnumerator SmoothlyLerp_Fov(float endValue)
    {
        float time = 0;
        float duration = 0.25f;
        float startValue = firstCamCinema.m_Lens.FieldOfView;

        while (time <= duration)
        {
            float currentValue = Mathf.Lerp(startValue, endValue, time / duration);
            firstCamCinema.m_Lens.FieldOfView = currentValue;
            time += Time.deltaTime;
            yield return null;
        }
    }
}
