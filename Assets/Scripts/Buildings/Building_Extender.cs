using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_Extender : Building
{
    GameManager gameManager;
    public override void buildingStart(Vector3 pos, Vector2Int coords)
    {
        gameManager = GameObject.Find(Constants.GAMEOBJECT_GAMEMANAGER).GetComponent<GameManager>();
        //do something to extend land here
        gameManager.AddPlacementBounds(coords, 5);
    }

    public override void buildingUpdate()
    {
        //nothing
    }

    public override bool doesBuildingUpdate()
    {
        return false;
    }

    public override bool isTower()
    {
        return false;
    }

    public override bool doesBuildingUpdateInDay()
    {
        return false;
    }
}
