using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICollectStone : AIJobBase
{

    bool atJob = false;
    bool atStone = false;
    Job job;
    float workStartTime = -1;
    StoneTracker stoneTracker;
    bool returning = false;
    bool hasCollected = false;

    public void StartJob(AIController controller, AIMovement movement)
    {
        job = controller.info.job;
        stoneTracker = job.jobObj.GetComponent<StoneTracker>();

        movement.TeleportToPos(job.building.transform.position);
        
    }

    public bool WorkJob(AIController controller, AIMovement movement)
    {
        if (returning)
        {
            if (!movement.ActiveSeek)
            {
                if (atJob)
                {
                    controller.PutObjectsIntoStorage(null); //TODO: this should change to the actual storage script or whatever
                    hasCollected = true;
                    controller.EndJob();
                }
                else
                {
                    Vector3 movePos = job.building.transform.position;
                    movePos.y = 0;
                    movement.MoveToPosition(movePos, 0.6f);
                    atJob = true;
                }
            }
        }
        else if (atStone)
        {
            if (stoneTracker == null || stoneTracker.gameObject == null)
            {
                returning = true;
                atJob = false;
            }
            else if (Time.time - workStartTime >= Constants.AI_TIME_RESOURCE_COLLECT_WOOD)
            {
                stoneTracker.MineStone(1);
                Resource stone = ResourceManager.Instance.SpawnResource(ResourceType.stone, movement.Position - new Vector3(0, movement.Position.y, 0));
                controller.AddObjectToHold(stone);
                workStartTime = Time.time;
                if (controller.isInventoryFull() || stoneTracker == null)
                {
                    returning = true;
                    atJob = false;
                }
            }
        }
        else
        {
            if (!movement.ActiveSeek)
            {
                if (atJob)
                {
                    atStone = true;
                    workStartTime = Time.time;
                }
                else
                {
                    Vector3 movePos = job.jobObj.transform.position;
                    movePos.y = 0;
                    movement.MoveToPosition(movePos, 1f);
                    atJob = true;
                }
            }
        }

        return false;
    }

    public void ForceFinishJob(AIController controller, AIMovement movement)
    {
        if (!controller.isInventoryFull())
        {
            while (!controller.isInventoryFull())
            {
                stoneTracker.MineStone(1);
                Resource stone = ResourceManager.Instance.SpawnResource(ResourceType.stone, movement.Position - new Vector3(0, movement.Position.y, 0));
                controller.AddObjectToHold(stone);
            }
        }

        if (!hasCollected)
            controller.PutObjectsIntoStorage(null); //TODO: this should change to the actual storage script or whatever

        movement.TeleportToPos(job.building.transform.position);

        controller.EndJob();
    }
}
