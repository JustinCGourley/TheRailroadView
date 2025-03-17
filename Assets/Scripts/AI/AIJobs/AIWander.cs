using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWander
{
    private Vector3 centerPoint;
    private Vector3 seekPoint;
    LevelManager levelManager;

    public AIWander()
    {
        levelManager = LevelManager.Instance;

        centerPoint = Vector3.zero;
        seekPoint = Vector3.zero;
    }

    public void StartWander(Vector3 pos)
    {
        centerPoint = pos;
    }

    public void Wander(AIMovement aiMovement)
    {
        if (seekPoint == Vector3.zero)
        {
            SetSeekPoint(aiMovement);
        }

        //Debug.Log(aiMovement.ActiveSeek);
        if (!aiMovement.ActiveSeek)
        {
            SetSeekPoint(aiMovement);
        }

        UtilityGizmo.Instance.DrawSphere(centerPoint, (Constants.LEVEL_TILE_WIDTH > Constants.LEVEL_TILE_HEIGHT) ? Constants.LEVEL_TILE_HEIGHT/3f : Constants.LEVEL_TILE_WIDTH/3f, new Color(0.2f, 0.2f, 0.2f, 0.1f));
        UtilityGizmo.Instance.DrawSphere(seekPoint, Constants.DEBUG_AI_WANDER_SEEK_POINT_RADIUS, new Color(0.2f, 0.2f, 0.2f, 0.6f));
    }

    private void SetSeekPoint(AIMovement aiMovement, float radiusOverwrite = 0)
    {
        float radius = (Constants.LEVEL_TILE_WIDTH > Constants.LEVEL_TILE_HEIGHT) ? Constants.LEVEL_TILE_HEIGHT/3f : Constants.LEVEL_TILE_WIDTH/3f;
        if (radiusOverwrite != 0)
        {
            radius = radiusOverwrite;
        }

        seekPoint = new Vector3(centerPoint.x + Random.Range(-radius, radius), 0, centerPoint.z + Random.Range(-radius, radius));
        aiMovement.MoveToPosition(seekPoint);
    }

}
