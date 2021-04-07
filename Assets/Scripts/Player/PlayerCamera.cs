using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCamera : MonoBehaviour
{
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public Transform cameraFollowPos;
    public Transform cameraLookAtPos;
    public float rotateSpeedX = 2f;
    public float rotateSpeedY = 2f;
    
    public Vector3 cameraOffset = Vector3.zero;
    	
    public bool allowPlayerControl = true;
    // Start is called before the first frame update

    private void Start()
    {
        BindCinema();
    }

    

    public void BindCinema()
    {
        cinemachineVirtualCamera.Follow = cameraFollowPos;
        cameraOffset = Vector3.forward;
    }

    public void SetCinemaOffset(Vector3 offset)
    {
        cameraOffset = offset;
    }
    
    // Update is called once per frame
    void LateUpdate()
    {
        //ScrollView();
        cameraLookAtPos.transform.position = cameraFollowPos.position + cameraOffset;
        if (allowPlayerControl)
        {
            RotateView();
        }
    }
    

    private void RotateView()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;
        var originalPosition = cameraFollowPos.position;
        var originalRotation = cameraLookAtPos.rotation;
        
        cameraLookAtPos.transform.RotateAround(originalPosition,cameraFollowPos.right,-Input.GetAxis("Mouse Y") * rotateSpeedY);
        var newPosition = cameraLookAtPos.position - originalPosition;
        var angleX = Vector3.Angle(newPosition, new Vector3(newPosition.x, 0, newPosition.z));
        if (angleX > 85f)
        {
            cameraLookAtPos.transform.position = cameraFollowPos.position + cameraOffset;
            cameraLookAtPos.rotation = originalRotation;
        }
        else
        {
            cameraOffset = cameraLookAtPos.position - originalPosition;
        }
        
        originalRotation = cameraLookAtPos.rotation;
        cameraLookAtPos.transform.RotateAround(originalPosition,Vector3.up,Input.GetAxis("Mouse X") * rotateSpeedX);
        
        newPosition = cameraLookAtPos.position - originalPosition;
        angleX =  Vector3.Angle(newPosition, new Vector3(newPosition.x, 0, newPosition.z));
        if (angleX > 85f)
        {
            cameraLookAtPos.transform.position = cameraFollowPos.position + cameraOffset;
            cameraLookAtPos.rotation = originalRotation;
        }
        else
        {
            cameraOffset = cameraLookAtPos.position - originalPosition;
        }
        cameraFollowPos.LookAt(cameraLookAtPos);
    }
}
