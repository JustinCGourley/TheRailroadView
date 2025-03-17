using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertUIElement : MonoBehaviour
{
    private void Update()
    {
        // point ourselves towards the camera
        Vector3 dir = this.transform.position - Camera.main.transform.position;
        this.transform.forward = dir;
    }
}
