using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingUpgrades
{
    public float damage;
    public float towerShotSpeed;
    public float range;
    public float accuracy;
    public int maxUnits;
    public int health;

    public void Add(BuildingUpgrades bU)
    {
        damage += bU.damage;
        towerShotSpeed += bU.towerShotSpeed;
        range += bU.range;
        accuracy += bU.accuracy;
        maxUnits = bU.maxUnits;
        health = bU.health;
    }
}
