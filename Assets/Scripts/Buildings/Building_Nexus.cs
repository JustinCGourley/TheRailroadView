using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_Nexus : Building
{

    AIManager aIManager;
    GameManager gameManager;

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void buildingStart(Vector3 pos, Vector2Int coords)
    {
        aIManager = GameObject.Find(Constants.GAMEOBJECT_AIMANAGER).GetComponent<AIManager>();
        gameManager = GameObject.Find(Constants.GAMEOBJECT_GAMEMANAGER).GetComponent<GameManager>();

        int numToSpawn = 5;

        for (int i = 0; i < numToSpawn; i++)
        {
            aIManager.GenerateAI(pos);
        }
    }

    public override void buildingUpdate(){}

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
