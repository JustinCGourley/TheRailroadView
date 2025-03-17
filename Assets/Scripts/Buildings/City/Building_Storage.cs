using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StorageType
{
    mana
}

public class Building_Storage : Building
{
    public int storageAmount;
    public StorageType storageType;

    public override void buildingStart(Vector3 pos, Vector2Int coords)
    {

    }

    public override void buildingUpdate()
    {

    }

    public override bool doesBuildingUpdate()
    {
        return false;
    }

    public override bool isTower()
    {
        return true;
    }
    public override bool doesBuildingUpdateInDay()
    {
        return false;
    }
}
