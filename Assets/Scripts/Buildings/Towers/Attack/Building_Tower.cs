using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_Tower : Building
{
    public ProjectileInfo projectileInfo;
    public Transform shootPos;
    [HideInInspector]public Vector3 position;
    public float towerShotSpeed;
    public float range;
    public float accuracy;
    float lastFire = 0;

    public override void buildingStart(Vector3 pos, Vector2Int coords)
    {
        position = pos;
    }

    public override void buildingUpdate()
    {

        List<Enemy> targets = WaveManager.Instance.getEnemiesInRange(this.transform.position, range);

        if (targets.Count == 0) return;

        GameObject target = GetCurrentTarget(targets);

        UtilityGizmo.Instance.DrawSphere(target.GetComponent<EnemyController>().GetShotPosition(), 0.1f, Color.red);
        
        if ((Time.time - lastFire) < towerShotSpeed) return;

        if (target == null) return;
        FireProjectile(target);
        lastFire = Time.time;
    }

    private void FireProjectile(GameObject target)
    {
        GameObject projectile = Instantiate(projectileInfo.GetProjectile(element));
        projectile.transform.position = shootPos.position;

        TowerProjectile towerProjectile = projectile.GetComponent<TowerProjectile>();
        //TODO - change this dir by some amount to aim up a little bit

        Vector3 targetDir = (target.GetComponent<EnemyController>().GetShotPosition() - shootPos.transform.position);

        float targetSpeed = target.GetComponent<EnemyController>().enemyInfo.speed;
        if (target.GetComponent<EnemyMovement>().atNexus) targetSpeed = 0;
        float timeToTarget = (targetDir.magnitude / (projectileInfo.speed - targetSpeed));

        Vector3 predictedPos = target.GetComponent<EnemyController>().GetShotPosition() + (target.transform.forward * targetSpeed * timeToTarget);
        Vector3 fireDir = (predictedPos - shootPos.position).normalized;

        towerProjectile.Shoot(projectileInfo, fireDir, target.GetComponent<EnemyController>(), accuracy);
    }

    private GameObject GetCurrentTarget(List<Enemy> targets)
    {
        GameObject closest = targets[0].obj;
        if (targets[0].obj == null) return null;
        float closestAmount = (GameManager.Instance.GetNexusPosition() - targets[0].obj.transform.position).magnitude;
        for (int i = 1; i < targets.Count; i++)
        {
            float amount = (GameManager.Instance.GetNexusPosition() - targets[i].obj.transform.position).magnitude;
            if (amount < closestAmount)
            {
                closest = targets[i].obj;
                closestAmount = amount;
            }
        }

        return closest;
    }

    public override bool doesBuildingUpdate()
    {
        return false;
    }

    public override bool doesBuildingUpdateInDay()
    {
        return false;
    }

    public override bool isTower()
    {
        return true;
    }
}
