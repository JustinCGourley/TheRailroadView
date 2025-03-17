using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericBuilding : Building
{
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

    public override bool doesBuildingUpdateInDay()
    {
        return false;
    }

    public override bool isTower()
    {
        return false;
    }
}
