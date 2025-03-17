using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_UnitTower : Building
{
    public int maxUnits; //set in inspector per building
    public int currentUnits;
    public Vector3 position;
    float towerRange;
    float lastFire = 0;
    private WaveManager waveManager;
    AIManager aiManager;
    AIUnitController leader;
    public AIUnitType towerUnitType;

    //used to max out troops no matter what
    [SerializeField] bool overrideUnits;

    public override void buildingStart(Vector3 pos, Vector2Int coords)
    {
        waveManager = GameObject.Find(Constants.GAMEOBJECT_GAMEMANAGER).GetComponent<WaveManager>();
        position = pos;

        //this is temporary to test the units
        aiManager = GameObject.Find(Constants.GAMEOBJECT_AIMANAGER).GetComponent<AIManager>();

        towerType = TowerType.unit;
    }

    public override void buildingUpdate()
    {
        
    }

    public void SetUnitCount(int unitCount)
    {
        currentUnits = unitCount;
    }

    public void UnitHasDied()
    {
        currentUnits--;
    }

    public void SpawnUnits()
    {
        if (currentUnits == 0 && !overrideUnits) return; //no units to spawn
        if (aiManager == null)
            aiManager = GameObject.Find(Constants.GAMEOBJECT_AIMANAGER).GetComponent<AIManager>();


        Debug.Log($"Spawning units - Current units: {currentUnits}");
        leader = aiManager.GenerateUnitAIGroup(this, towerUnitType, overrideUnits ? maxUnits : currentUnits);
    }

    public void KillUnits()
    {
        Debug.Log("trying to kill units");
        if ((leader == null || currentUnits == 0) && !overrideUnits) return;

        Debug.Log("Leader exists");
        foreach (AIUnitController unit in leader.units)
        {
            Debug.Log("In theory killing off leader unit");
            aiManager.KillUnit(unit);
        }
        Debug.Log("Killing off leader");
        aiManager.KillUnit(leader);
    }

    public void didClick(Vector3 pos)
    {
        leader.MoveToClickPos(pos);
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
