using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICollectTree : AIJobBase
{

    bool atJob = false;
    bool atTree = false;
    bool collectingWood = false;
    Job job;
    float workStartTime = -1;
    GameObject curTree;
    Transform treeObj;
    TreeAdjuster treeAdjuster;
    int curTreeIndex;

    List<Resource> woodToCollect;
    Vector3 endLoc;

    bool forceFinished = false;
    bool hasCollected = false;

    public void StartJob(AIController controller, AIMovement movement)
    {
        job = controller.info.job;
        treeAdjuster = job.jobObj.GetComponent<TreeAdjuster>();

        treeObj = job.jobObj.transform;
        woodToCollect = new List<Resource>();
        curTreeIndex = 0;

        movement.TeleportToPos(job.building.transform.position);
    }

    public bool WorkJob(AIController controller, AIMovement movement)
    {
        if (forceFinished)
        {
            return true;
        }
        if (atTree)
        {
            if (workStartTime == -1)
            {
                workStartTime = Time.time;
            }
            else if (Time.time - workStartTime >= Constants.AI_TIME_RESOURCE_COLLECT_WOOD)
            {
                Vector3 spawnPos = curTree.transform.position;
                spawnPos.y = 0;
                Resource woodObj = ResourceManager.Instance.SpawnResource(ResourceType.wood, spawnPos);
                woodToCollect.Add(woodObj);

                treeAdjuster.CutTree(curTreeIndex);
                curTree = null;
                workStartTime = -1;
                atTree = false;
                curTreeIndex++;

                bool pass = GoToNextTree(movement);
                if (!pass) //trees are gone for this tile if this passes
                {
                    collectingWood = true;
                    atJob = true;
                }
            }
        }
        else if (collectingWood)
        {
            if (!movement.ActiveSeek)
            {
                if (atJob)
                {
                    controller.AddObjectToHold(woodToCollect[0]);
                    woodToCollect.RemoveAt(0);

                    if (woodToCollect.Count > 0)
                    {
                        movement.MoveToPosition(woodToCollect[0].obj.transform.position);
                    }
                    else
                    {
                        endLoc = job.building.transform.position;
                        movement.MoveToPosition(endLoc);
                        atJob = false;
                        LevelManager.Instance.UnclaimLocation(Utility.Instance.GetTileCoordsFromPosition(job.jobPos));
                    }
                }
                else
                {
                    controller.PutObjectsIntoStorage(null); //TODO: this should change to the actual storage script or whatever
                    hasCollected = true;
                    FinishedTreeJob(controller);
                }
            }
        }
        else
        {
            if (!movement.ActiveSeek)
            {
                if (atJob)
                {
                    atTree = true;
                }
                else
                {
                    atJob = true;
                    atTree = false;
                    GoToNextTree(movement);
                }
            }
        }

        if (curTree != null)
            UtilityGizmo.Instance.DrawSphere(curTree.transform.position, 0.3f, new Color(0.0f, 1.0f, 0.0f, 0.2f));
        
        return true;
    }

    private void FinishedTreeJob(AIController controller)
    {
        LevelManager.Instance.DestroyObjectOnTile(Utility.Instance.GetTileCoordsFromPosition(job.jobPos));
        controller.EndJob();
    }

    private bool GoToNextTree(AIMovement movement)
    {
        if (!TreeTileDone(treeObj.transform))
        {
            curTree = treeObj.GetChild(curTreeIndex).gameObject;
            Vector3 movePos = curTree.transform.position;
            movePos.y = 0;
            movement.MoveToPosition(movePos, Constants.AI_TREE_ARRIVED_TREE_DISTANCE);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if tree has been fully cut down
    /// </summary>
    /// <param name="tree"></param>
    /// <returns></returns>
    private bool TreeTileDone(Transform tree)
    {
        return !(tree.childCount > curTreeIndex);
    }

    public void ForceFinishJob(AIController controller, AIMovement movement)
    {
        if (treeObj != null)
        {
            while (controller.HoldingObjs.Count < treeObj.transform.childCount)
            {
                if (woodToCollect.Count > 0)
                {
                    controller.AddObjectToHold(woodToCollect[0]);
                    woodToCollect.RemoveAt(0);
                }
                else
                {
                    Resource woodObj = ResourceManager.Instance.SpawnResource(ResourceType.wood, movement.Position);
                    controller.AddObjectToHold(woodObj);
                }
            }
        }

        Debug.Log("Force finishing job?");

        if (!hasCollected)
            controller.PutObjectsIntoStorage(null);
        
        movement.TeleportToPos(job.building.transform.position);

        FinishedTreeJob(controller);

    }

}
