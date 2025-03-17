using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AIType
{
    Worker,
    Unit,
    Researcher
}

public class AIController : MonoBehaviour
{
    public AIType aiType;
    public AIInfo info;
    public List<Resource> HoldingObjs { get { return holdingObjs; } }
    List<Resource> holdingObjs;
    int maxHoldCount;

    public AIMovement aiMovement;

    private bool wanderStart = false;
    private bool movingToJob = false;
    private bool endingJob = false;

    public AIManager aiManager;
    AIWander aiWander;
    AIJobBase curJob;
    AIMoveTo aiMoveTo;

    bool endingDay = false;
    Home home = null;

    bool forceFinishedJob = false;

    int daysSinceLastAte;

    // Start is called before the first frame update
    void Start()
    {
        aiMovement = this.GetComponent<AIMovement>();
        aiManager = GameObject.Find(Constants.GAMEOBJECT_AIMANAGER).GetComponent<AIManager>();
        aiMoveTo = new AIMoveTo();

        aiWander = new AIWander();
        wanderStart = true;

        holdingObjs = new List<Resource>();
        maxHoldCount = Constants.AI_MAX_HOLD_COUNT;

    }

    public virtual void UpdateController()
    {
        if (endingDay)
        {
            aiMoveTo.WorkJob(this, this.aiMovement);
            //once home AI goes back to normal (wont be activated until next day though)
            if (AtHome())
            {
                endingDay = false;
                wanderStart = true;
            }
        }
        else if (info.job != null) //do job
        {
            if (movingToJob) 
            {
                aiMoveTo.WorkJob(this, this.aiMovement); //pathfinding
            }
            else
            {
                curJob.WorkJob(this, this.aiMovement); //just seeking
            }
        }
        else 
        {
            if (wanderStart)
            {
                aiWander.StartWander(Utility.Instance.GetCenterOfTile(this.transform.position));
                aiMovement.movementType = AIMovement.MovementType.walking;
                wanderStart = false;
            }
            else
            {
                aiWander.Wander(aiMovement);
            }
        }

        
        aiMovement.UpdateMovement();
        if (holdingObjs.Count != 0)
        {
            HoldObjects();
        }
    }

    public void StartJob(Job job)
    {
        info.job = job;
        forceFinishedJob = false;

        aiMovement.movementType = AIMovement.MovementType.running;

        movingToJob = true;
        if (job.jobType == JobType.harvest_material_stone || job.jobType == JobType.harvest_material_wood)
        {
            aiMoveTo.StartJob(this, this.aiMovement, job.building.transform.position);
        }
        else
        {
            aiMoveTo.StartJob(this, this.aiMovement);
        }

        if (info.job == null) { return; } //check if aiMoveTo failed to start job
        switch (info.job.jobType)
        {
            case JobType.harvest_material_wood:
                curJob = new AICollectTree();
                curJob.StartJob(this, this.aiMovement);
                break;
            case JobType.harvest_material_stone:
                curJob = new AICollectStone();
                curJob.StartJob(this, this.aiMovement);
                break;
            case JobType.work_building:
                Debug.Log("Start work building");
                break;
            case JobType.work_building_farm:
                curJob = new AIWorkFarm();
                curJob.StartJob(this, this.aiMovement);
                break;
        }
    }

    public void EndJob(int code = -1)
    {
        switch (code)
        {
            case 1: //AiMoveTo received a null value for path to follow
                movingToJob = false;
                return;
            default:
                break;
        }

        //AIMoveTo will call endJob, catch this before ending the real job
        if (movingToJob)
        {
            movingToJob = false;
            return;
        }

        if (info.unitNumber >= 1000)
        {
            JobManager.Instance.CompleteJob(this.info.job);

            this.info.job = null;
            curJob = null;
            wanderStart = true;
        }
    }

    public bool HasJob()
    {
        return curJob != null;
    }

    private void FindHome()
    {
        if (home == null)
        {
            home = aiManager.FindHome(this);
            if (home != null)
            {
                home.AddPerson(this);
            }
        }
    }

    public void AddObjectToHold(Resource obj)
    {
        //if (isInventoryFull()) { Debug.LogError("Somehow trying to hold more objects than able to?"); }
        holdingObjs.Add(obj);
    }

    public bool isInventoryFull()
    {
        return holdingObjs.Count >= maxHoldCount;
    }

    public void PutObjectsIntoStorage(GameObject storage)
    {
        for (int i = 0; i < holdingObjs.Count; i++)
        {
            ResourceManager.Instance.AddResource(holdingObjs[i].type, 1);
            ResourceManager.Instance.RemoveResource(holdingObjs[i]);
        }
        holdingObjs = new List<Resource>();
    }

    private void HoldObjects()
    {
        foreach (Resource resource in holdingObjs)
        {
            resource.obj.transform.position = new Vector3(aiMovement.Position.x, 0.1f, aiMovement.Position.z);
        }
    }

    public void ForceFinishJob()
    {
        if (forceFinishedJob) return;

        forceFinishedJob = true;
        movingToJob = false;
        curJob.ForceFinishJob(this, aiMovement);
        EndDay();
    }

    /// <summary>
    /// For when the day ends, we force the ai to stop whatever they are doing and return home
    /// </summary>
    public void EndDay()
    {
        endingDay = true;
        FindHome(); //find a home if we dont have one

        if (home != null)
        {
            GoHome();
        }
        if (this.info.job != null)
        {
            ForceFinishJob();
            this.info.job = null;
        }
    }

    private void GoHome()
    {
        aiMoveTo.StartJobWithPosition(this, aiMovement, home.transform.position);
        aiMovement.movementType = AIMovement.MovementType.sprinting;
    }

    private bool AtHome()
    {
        if (home == null) return true;
        return (this.aiMovement.Position - home.transform.position).magnitude <= Constants.AI_REACHED_HOME_DISTANCE;
    }

    public void Eat()
    {
        bool canEat = ResourceManager.Instance.SpendResource(ResourceType.food, 1);
        if (canEat)
        {
            daysSinceLastAte = 0;
        }
        else
        {
            daysSinceLastAte++;
        }
    }

    public float CalculateHappiness()
    {
        // has a job? (no = +0 | yes = +20)
        // has a home? (no = +0 | yes = +40)
        // has eaten today (no = -10*num of days not eaten | yes = +40)

        float happiness = 0;

        if (info.job != null)
        {
            happiness += 20;
        }

        if (home != null)
        {
            happiness += 40;
        }

        if (daysSinceLastAte == 0)
        {
            happiness += 40;
        }
        else
        {
            happiness -= (daysSinceLastAte * 10);
        }

        return happiness;
    }
}
