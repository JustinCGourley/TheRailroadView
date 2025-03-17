using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    CameraController cameraController;

    // Start is called before the first frame update
    void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();    
    }

    // Update is called once per frame
    void Update()
    {
        CheckCameraInput();
        CheckDebug();
        CheckClick();
    }

    private void CheckCameraInput()
    {
        // --- deal with movement input
        int moveCount = 0;
        if (Input.GetKey(KeyCode.W))
        {
            cameraController.Move(1);
            moveCount++;
        }
        if (Input.GetKey(KeyCode.S))
        {
            cameraController.Move(2);
            moveCount++;
        }
        if (Input.GetKey(KeyCode.D))
        {
            cameraController.Move(3);
            moveCount++;
        }
        if (Input.GetKey(KeyCode.A))
        {
            cameraController.Move(4);
            moveCount++;
        }

        //if no presses
        if (moveCount == 0)
        {
            cameraController.Move(0);
        }

        // --- deal with rotation
        if (Input.GetKey(KeyCode.E))
        {
            cameraController.Rotate(2);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            cameraController.Rotate(1);
        }
        //middle mouse clicks
        if (Input.GetMouseButtonDown(2))
        {
            cameraController.StartRotate();
        }
        if (Input.GetMouseButtonUp(2))
        {
            cameraController.StopRotate();
        }


        // --- deal with scrolling
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cameraController.UpdateCameraScroll(scroll);
    }

    private void CheckDebug()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.D))
        {
            GameManager.Instance.ActivateDebug();
        }
    }

    private void CheckClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameManager.Instance.DidClick(true);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            GameManager.Instance.DidStopClick(true);
        }
        if (Input.GetMouseButtonDown(1))
        {
            GameManager.Instance.DidClick(false);
        }
    }
}
