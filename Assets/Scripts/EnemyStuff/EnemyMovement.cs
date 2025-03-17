using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    Vector3 position;
    Vector3 directon;
    Vector3 velocity;

    float seekRadius;

    float maxSpeed;

    List<Vector3> movePath = new List<Vector3>();
    GameManager gameManager;
    EnemyController controller;

    private float acceleration = Constants.AI_BASE_ACCELERATION;

    AIUnitController unitTarget;

    public bool atNexus = false;

    // Start is called before the first frame update
    public void MovementStart()
    {
        gameManager = GameObject.Find(Constants.GAMEOBJECT_GAMEMANAGER).GetComponent<GameManager>();
        controller = this.GetComponent<EnemyController>();
        seekRadius = Constants.AI_SEEK_ARRIVED_DISTANCE;
        maxSpeed = controller.enemyInfo.maxSpeed;
        position = this.transform.position;
    }

    public void UpdateMove(bool isSlow = false)
    {
        //at nexus
        if (IsNearNexus())
        {
            atNexus = true;
            controller.AttackNexus();
            return;
        }

        if (movePath.Count == 0) return;

        Vector3 seekPos = movePath[0];

        if (unitTarget == null)
        {
            //move to nexus
            if (movePath == null) return; //havnt yet set a path
            if (movePath.Count == 0) return; //reached nexus


            seekPos = movePath[0];
            if (controller.enemyInfo.isFlying && movePath.Count > 1)
            {
                seekPos.y = controller.enemyInfo.flyingHeight;
            }

            if ((position - seekPos).magnitude < seekRadius)
            {
                movePath.RemoveAt(0);
                if (movePath.Count == 0) return; //reached nexus
            }
        }
        else
        {
            seekPos = unitTarget.aiMovement.Position;
            if ((position - seekPos).magnitude <= Constants.ENEMY_HIT_ENEMY_DISTANCE)
            {
                seekPos = this.transform.position;
            }
        
        }



        SeekToPos(seekPos, isSlow);

        UpdatePosition();
    }

    public void UpdatePath()
    {
        PathFinding.Instance.FindPathAsync(position, gameManager.GetNexusPosition(), (path) =>
        {
            this.movePath = path;
        });
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

    private void SeekToPos(Vector3 pos, bool isSlow)
    {
        float speed = this.controller.enemyInfo.speed * acceleration;
        if (isSlow) 
            speed *= Constants.ENEMY_IS_SLOW_AMOUNT;

        Vector3 desiredDir = (pos - this.transform.position).normalized;
        Vector3 desiredVel = desiredDir * speed;

        this.velocity += desiredVel;
        this.velocity = Vector3.ClampMagnitude(velocity, (isSlow) ? maxSpeed * Constants.ENEMY_IS_SLOW_AMOUNT : maxSpeed);

        UtilityGizmo.Instance.DrawRay(position + new Vector3(0, 0.1f, 0), velocity / 4, Color.gray);
        UtilityGizmo.Instance.DrawSphere(position + new Vector3(0, 0.1f, 0) + (velocity / 4), Constants.DEBUG_AI_MOVE_VECTOR_SPHERE_RADIUS, Color.gray);
    }

    private bool IsNearNexus()
    {
        return (this.transform.position - gameManager.GetNexusPosition()).magnitude <= Constants.ENEMY_NEAR_NEXUS_DISTANCE;
    }

    public void SeekTarget(AIUnitController target)
    {
        unitTarget = target;
    }

}
