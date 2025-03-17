using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWorkFarm : AIJobBase
{
    Building_Farm farm;
    bool movingToJob = false;
    bool atJob = false;
    bool returning = false;
    Vector3 randomPoint = Vector3.zero;
    AIWander aiWander;
    bool hasCollected = false;
    public void StartJob(AIController controller, AIMovement movement)
    {
        farm = controller.info.job.jobObj.GetComponent<Building_Farm>();
        atJob = false;
        aiWander = new AIWander();
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
                    Vector3 movePos = GameManager.Instance.GetClosestStoragePosition(movement.Position);
                    movePos.y = 0;
                    movement.MoveToPosition(movePos, 0.6f);
                    atJob = true;
                }
            }
        }
        else if (atJob)
        {
            if (farm.IsDone())
            {
                farm.CutFarm();
                for (int i = 0; i < Constants.FARM_RESOURCE_SPAWN; i++)
                {
                    Resource obj = ResourceManager.Instance.SpawnResource(ResourceType.food, controller.transform.position);
                    controller.AddObjectToHold(obj);
                }
                atJob = false;
                returning  = true;
            }
            else
            {
                aiWander.Wander(movement);
            }
        }
        else
        {
            if (!movement.ActiveSeek)
            {
                if (movingToJob)
                {
                    atJob = true;
                    farm.StartFarming();
                    aiWander.StartWander(controller.info.job.jobPos);
                }
                else
                {
                    Vector3 movePos = controller.info.job.jobObj.transform.position;
                    movePos.y = 0;
                    movement.MoveToPosition(movePos, 1f);
                    movingToJob = true;
                }
            }
        }

        return false;
    }

    public void ForceFinishJob(AIController controller, AIMovement movement)
    {
        farm.CutFarm();
        for (int i = 0; i < Constants.FARM_RESOURCE_SPAWN || controller.HoldingObjs.Count == Constants.FARM_RESOURCE_SPAWN; i++)
        {
            Resource obj = ResourceManager.Instance.SpawnResource(ResourceType.food, controller.transform.position);
            controller.AddObjectToHold(obj);
        }

        if (!hasCollected)
            controller.PutObjectsIntoStorage(null); //TODO: this should change to the actual storage script or whatever

        movement.TeleportToPos(controller.info.job.building.transform.position);

        controller.EndJob();
    }
}
