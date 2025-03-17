using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_ControlledTower : Building
{
    [HideInInspector] public Vector3 position;
    [SerializeField] public float towerRange;

    [SerializeField] GameObject marker;
    [SerializeField] public ProjectileInfo projectile;
    [SerializeField] public int manaCost;

    float lastExplodeTime = -1;

    public override void buildingStart(Vector3 pos, Vector2Int coords)
    {
        position = pos;
        towerType = TowerType.controlled;
        lastExplodeTime = 0;
    }

    public override void buildingUpdate()
    {
        
    }

    public void didClick()
    {
        Vector3 pos = Utility.Instance.GetWorldPositionFromMousePosition();
        if (/*(position - pos).magnitude <= this.towerRange && */ GameManager.Instance.HasEnoughManaToSpend(manaCost))
        {
            if (Time.time - lastExplodeTime >= projectile.speed)
            {
                lastExplodeTime = Time.time;
                GameManager.Instance.SpendMana(manaCost);
                projectile.GetProjectile(this.element).GetComponent<ControlledTowerProjectile>().Shoot(projectile, pos);
            }
        }
    }

    public float timeLeftUntilFire()
    {
        float timeLeft = projectile.speed - (Time.time - lastExplodeTime);
        if (timeLeft < 0)
            timeLeft = 0;
        return timeLeft;
    }

    public float percentLeftUntilFire()
    {
        return timeLeftUntilFire() / projectile.speed;
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
