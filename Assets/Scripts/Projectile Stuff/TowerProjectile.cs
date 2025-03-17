using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerProjectile : MonoBehaviour
{

    ProjectileInfo projectileInfo;
    public Element element;
    Vector3 targetDir;

    Vector3 position;
    Vector3 velocity;

    float hitGroundTime = 0;

    EnemyController target;

    bool deadProjectile = false;
    float startTime;
    float accuracyTime;

    public void Shoot(ProjectileInfo projectileInfo, Vector3 dir, EnemyController intendedTarget, float accuracyTime)
    {
        this.projectileInfo = projectileInfo;

        this.position = this.transform.position;
        this.targetDir = dir;
        target = intendedTarget;
        deadProjectile = false;
        startTime = Time.time;
        this.accuracyTime = accuracyTime;
    }

    // Update is called once per frame
    public void FixedUpdate()
    {
        if (projectileInfo == null) return;

        if (deadProjectile) return;
        
        MoveArrow();
        
    }

    void MoveArrow()
    {

        //TODO add gravity
        //targetDir += (Constants.PROJECTILE_GRAVITY_FORCE * Time.deltaTime);

        //update targetDir to 'cheat' / reposition to actually hit the thing
        if (target != null && ((Time.time - startTime) <= accuracyTime))
            UpdateDirectionToTarget();

        Vector3 force = (targetDir * projectileInfo.speed);

        //UtilityGizmo.Instance.DrawLine(this.transform.position, this.transform.position + (force * 10), Color.grey);
        velocity += force;

        if (projectileInfo.isHoming)
        {
            if (target != null)
            {
                Vector3 homingForce = (target.transform.position - position).normalized * (projectileInfo.speed * Constants.PROJECTILE_HOMING_MULTIPLIER);
                velocity += homingForce;
            }
            else
            {
                List<Enemy> t =  WaveManager.Instance.getEnemiesInRange(position, Constants.PROJECTILE_HOMING_RADIUS);
                if (t.Count != 0)
                {
                    target = t[0].controller;
                }
            }
        }

        velocity = Vector3.ClampMagnitude(velocity, projectileInfo.speed);

        position += velocity * Time.deltaTime;
        this.transform.position = position;
        this.transform.forward = targetDir;

        if (position.y <= Constants.PROJECTILE_DESTROY_HEIGHT)
        {
            ElementManager.Instance.ApplyElement(position, null, projectileInfo, element);
            Destroy(this.gameObject);
        }
    }

    private void UpdateDirectionToTarget()
    {
        Vector3 updatedDir = ((target.GetShotPosition()) - this.transform.position);

        float targetSpeed = target.GetComponent<EnemyController>().enemyInfo.speed;
        if (target.GetComponent<EnemyMovement>().atNexus) targetSpeed = 0;
        float timeToTarget = (updatedDir.magnitude / (projectileInfo.speed - targetSpeed));

        Vector3 predictedPos = target.GetShotPosition() + (target.transform.forward * targetSpeed * timeToTarget);
        Vector3 fireDir = (predictedPos - this.transform.position).normalized;


        targetDir = fireDir;
    }

    IEnumerator DestroyProjectile(float waitTime)
    {

        yield return new WaitForSeconds(waitTime);

        if (this.gameObject == null)
        {
            yield return null;
        }
        else
        {
            Destroy(this.gameObject);
        }

        yield return null;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == Constants.TAG_ENEMY)
        {
            EnemyController enemyController = col.gameObject.GetComponent<EnemyController>();

            ElementManager.Instance.ApplyElement(position, enemyController, projectileInfo, element);
            this.transform.parent = col.gameObject.transform;
            StartCoroutine(DestroyProjectile(Constants.PROJECTILE_DESTROY_TIME));
        }
        else if (col.gameObject.name == Constants.GAMEOBJECT_LEVELMAP)
        {
            ElementManager.Instance.ApplyElement(position, null, projectileInfo, element);
            StartCoroutine(DestroyProjectile(Constants.PROJECTILE_DESTROY_TIME));
        }
        else
        {
            StartCoroutine(DestroyProjectile(Constants.PROJECTILE_DESTROY_TIME));
        }

        deadProjectile = true;
    }
}
