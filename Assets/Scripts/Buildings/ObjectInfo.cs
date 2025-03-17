using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{
    city,
    attack,
    support
}

[CreateAssetMenu(fileName = "ObjectInfo", menuName = "ObjectInfo")]
public class ObjectInfo : ScriptableObject
{
    public GameObject obj;
    public int upgradeCost;
    public ObjectType objType;
    public BuildingType buildingType;
    public bool isStartBuilding;
    public int researchCost;
    public string buildingName;

    public int woodCost;
    public int stoneCost;
    public int manaCost;
    public int workerCost;

    int curUpgrade;
    public int CurUpgrade { get { return curUpgrade; } }
    public bool isUpgradable;
    public bool isPipeable;


    public ObjectInfo(GameObject obj, ObjectType objType, BuildingType buildingType, bool isStartBuilding, int researchCost, string buildingName, int woodCost, int stoneCost, int manaCost, int workerCost)
    {
        this.obj = obj;
        this.objType = objType;
        this.buildingType = buildingType;
        this.isStartBuilding = isStartBuilding;
        this.researchCost = researchCost;
        this.buildingName = buildingName;
        this.woodCost = woodCost;
        this.stoneCost = stoneCost;
        this.manaCost = manaCost;
        this.workerCost = workerCost;

        this.curUpgrade = 0;

    }

    public void SetUpgrade(int upgrade)
    {
        Debug.Log($"Setting upgrade of {buildingName} to {upgrade}");
        curUpgrade = upgrade;
    }

    public override string ToString()
    {
        return $"${buildingName} | rCost: {researchCost} | cost: [W: {woodCost} S: {stoneCost} M: {manaCost}] | isStart? {isStartBuilding}";
    }
}