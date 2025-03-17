
using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    
    public float maxSpeed;
    public float maxMaxSpeed;
    private float speedUpStart;
    public float acceleration;

    public float rotationSpeed;

    public float panBorderDistance = 10f;
    public Vector2 panLimit;

    [SerializeField] Transform trackerObj; 

    public float scrollSpeed = 20f;
    public float minY = 20f;
    public float maxY = 120f;


    public float slowDownSpeed;
    public float stopThreshold;
    private Vector2 velocity;

    bool moving = false;

    bool mouseRotation = false;
    Vector2 mouseRotationStart;
    float mouseRotationStartRotation;

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = new Color(0, 0, 0, 0.5f);
    //    Gizmos.DrawSphere(trackerObj.transform.position, 0.05f);
    //}

    private void Start()
    {
        this.transform.Rotate(new Vector3(90, 0, 0));
    }

    private void Update()
    {
        UpdatePosition();
        if (mouseRotation) 
            UpdateMouseRotation();
        
        if (velocity.magnitude > 0 && !moving)
        {
            SlowDown();
        }

        //UtilityGizmo.Instance.DrawSphere(trackerObj.transform.position, 1f, Color.black);
    }

    public void Move(int moveDir)
    {
        switch (moveDir)
        {
            case 0:
                moving = false;
                break;
            case 1:
                ApplyForce(new Vector2(0, acceleration));
                break;
            case 2:
                ApplyForce(new Vector2(0, -acceleration));
                break;
            case 3:
                ApplyForce(new Vector2(acceleration, 0));
                break;
            case 4:
                ApplyForce(new Vector2(-acceleration, 0));
                break;
        }
    }

    private void ApplyForce(Vector2 force)
    {
        velocity += ((new Vector2(trackerObj.forward.x, trackerObj.forward.z) * force.y)
            + (new Vector2(trackerObj.right.x, trackerObj.right.z) * force.x)) * Time.deltaTime;
        velocity = Vector2.ClampMagnitude(velocity, (Time.time - speedUpStart >= Constants.CAMERA_SPEED_UP_TIME) ? maxMaxSpeed : maxSpeed);
        if (!moving)
        {
            moving = true;
            speedUpStart = Time.time;
        }
    }

    private void UpdatePosition()
    {
        Vector3 newPos = trackerObj.transform.position + (new Vector3(velocity.x, 0, velocity.y) * Time.deltaTime);

        newPos.x = Mathf.Clamp(newPos.x, -panLimit.x, panLimit.x);
        newPos.z = Mathf.Clamp(newPos.z, -panLimit.y, panLimit.y);

        trackerObj.transform.position = newPos;
    }

    public void Rotate(int dir)
    {
        if (dir == 1)
        {
            trackerObj.transform.Rotate(0, rotationSpeed, 0);
        }
        else if (dir == 2)
        {
            trackerObj.transform.Rotate(0, -rotationSpeed, 0);
        }
    }

    public void UpdateMouseRotation()
    {
        float yRot = mouseRotationStartRotation - (((mouseRotationStart.x - Input.mousePosition.x) / Constants.CAMERA_MOUSE_FULL_ROTATION_AMOUNT) * 360);

        this.trackerObj.transform.eulerAngles = new Vector3(0, yRot, 0);
    }

    public void StartRotate()
    {
        mouseRotation = true;
        mouseRotationStart = Input.mousePosition;
        mouseRotationStartRotation = this.trackerObj.rotation.eulerAngles.y;

    }

    public void StopRotate()
    {
        mouseRotation = false;
    }

    private void SlowDown()
    {
        Vector2 slowForce = -velocity.normalized * slowDownSpeed;
        velocity += slowForce;

        if (velocity.magnitude <= stopThreshold)
        {
            velocity = Vector2.zero;
        }
    }

    public void UpdateCameraScroll(float scroll)
    {
        Vector3 pos = transform.position;
        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;

        this.transform.forward = (trackerObj.transform.position - this.transform.position).normalized;
    }
}
