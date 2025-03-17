using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{

    private Vector3 velocity;
    private Vector3 position;
    public Vector3 Position { get { return position; } }
    private float maxSpeed = Constants.AI_BASE_MAX_SPEED;
    private float acceleration = Constants.AI_BASE_ACCELERATION;

    AIController controller;

    Vector3 seekPos;
    float seekRadius;
    private bool activeSeek = false;
    public bool ActiveSeek { get { return activeSeek; } }

    List<AIUnitController> flockGroup = new List<AIUnitController>();

    public enum MovementType
    {
        walking,
        running,
        sprinting
    }

    public MovementType movementType;

    public float MovementTypeAdjust()
    {
        switch (movementType)
        {
            case MovementType.sprinting:
                return 1.5f;
            case MovementType.running:
                return 1.0f;
            case MovementType.walking:
                return 0.15f;
            default:
                return 1.0f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        velocity = Vector3.zero;

        controller = this.GetComponent<AIController>();
        position = this.transform.position;
    }

    public void UpdateMovement()
    {
        if (activeSeek)
        {
            if ((seekPos - this.transform.position).magnitude >= seekRadius)
            {
                SeekToPos(seekPos);
            }
            else
            {
                seekPos = Vector3.zero;
                activeSeek = false;
            }

            if (seekPos != Vector3.zero)
            {
                UtilityGizmo.Instance.DrawLine(this.transform.position + new Vector3(0f, 0.1f, 0f), seekPos + new Vector3(0f, 0.1f, 0f), Color.grey);
                UtilityGizmo.Instance.DrawSphere(seekPos, seekRadius, new Color(0.5f, 0.5f, 0.5f, 0.4f));
            }
        }
        else if (velocity.magnitude > Constants.AI_MINIMUM_MOVEMENT_VALUE)
        {
            SlowStop();
        }
            
        UpdatePosition();
    }

    private void SlowStop()
    {
        velocity *= 0.5f;
        if (velocity.magnitude <= Constants.AI_MINIMUM_MOVEMENT_VALUE)
        {
            velocity = Vector3.zero;
        }
    }

    private void UpdatePosition()
    {
        position += velocity * Time.deltaTime;
        this.transform.position = position;
        if (velocity.magnitude > 0)
        {
            this.transform.forward = velocity.normalized;
        }
    }

    /// <summary>
    /// Called by outside classes to get the AI to move to a given position
    /// </summary>
    /// <param name="pos"></param>
    public void MoveToPosition(Vector3 pos, float arrivedRadius = Constants.AI_SEEK_ARRIVED_DISTANCE)
    {
        seekPos = pos;
        seekRadius = arrivedRadius;
        activeSeek = true;
    }

    private void SeekToPos(Vector3 pos)
    {
        float speed = this.controller.info.speed * (acceleration * MovementTypeAdjust());

        Vector3 desiredDir = (pos - this.transform.position).normalized;
        Vector3 desiredVel = desiredDir * speed;

        this.velocity += desiredVel;
        this.velocity = Vector3.ClampMagnitude(velocity, (maxSpeed * MovementTypeAdjust() * (((seekPos - this.transform.position).magnitude > Constants.AI_SEEK_CLOSE_DISTANCE) ? 1f : 0.75f)));

        UtilityGizmo.Instance.DrawRay(position + new Vector3(0, 0.1f, 0), velocity / 4, Color.gray);
        UtilityGizmo.Instance.DrawSphere(position + new Vector3(0, 0.1f, 0) + (velocity/4), Constants.DEBUG_AI_MOVE_VECTOR_SPHERE_RADIUS, Color.gray);
    }

    public void TeleportToPos(Vector3 pos)
    {
        position = pos;
        UpdatePosition();
    }

}
