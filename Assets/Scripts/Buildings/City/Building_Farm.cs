using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_Farm : Building
{
    [SerializeField] float startY;
    [SerializeField] float endY;
    [SerializeField] GameObject wheatObj;

    float startTime = -1;

    public override void buildingStart(Vector3 pos, Vector2Int coords)
    {
        Vector3 wheatPos = wheatObj.transform.position;
        wheatPos.y = startY;
        wheatObj.transform.position = wheatPos;
        startTime = -1;
    }

    public override void buildingUpdate()
    {
        if (startTime != -1)
        {
            Vector3 pos = wheatObj.transform.position;
            pos.y = Mathf.Lerp(startY, endY, Time.time - startTime / Constants.FARM_GROW_TIME);
            wheatObj.transform.position = pos;
        }
    }

    public void StartFarming()
    {
        startTime = Time.time;
    }

    public bool IsDone()
    {
        return Time.time-startTime >= Constants.FARM_GROW_TIME;
    }

    public void CutFarm()
    {
        startTime = -1;
        Vector3 pos = wheatObj.transform.position;
        pos.y = startY;
        wheatObj.transform.position = pos;
    }

    public override bool doesBuildingUpdate()
    {
        return false;
    }

    public override bool doesBuildingUpdateInDay()
    {
        return true;
    }

    public override bool isTower()
    {
        return false;
    }
}
