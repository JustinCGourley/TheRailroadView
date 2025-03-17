using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_Test : Building
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

    public override bool isTower()
    {
        return true;
    }
    public override bool doesBuildingUpdateInDay()
    {
        return false;
    }
}
