using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JobType : int
{
    harvest_material_wood  = 0,
    harvest_material_stone = 1,

    work_building          = 2,
    work_building_farm     = 3,
}

public class Job
{
    public Vector3 jobPos;
    public JobType jobType;
    public GameObject jobObj;
    public List<GameObject> jobObjs;
    public int maxWorkerCount;
    public List<AIController> workers;
    public Building building;

    public Job(Vector3 jobPos, JobType jobType, GameObject jobObj, int maxWorkerCount)
    {
        this.jobPos = jobPos;
        this.jobType = jobType;
        this.jobObj = jobObj;
        this.maxWorkerCount = maxWorkerCount;
        workers = new List<AIController>();
    }
}
