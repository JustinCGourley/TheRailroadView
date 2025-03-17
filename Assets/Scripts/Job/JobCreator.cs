using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobCreator : MonoBehaviour
{
    public static JobCreator Instance { get; private set; }

    /// <summary>
    /// sets up singleton class
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    JobManager jobManager;

    // Start is called before the first frame update
    void Start()
    {
        jobManager = JobManager.Instance;
    }

    public void CreateTreeJob(LevelData tileData, Building building, AIController worker)
    {
        Job job = new Job(
            tileData.obj.transform.position, 
            JobType.harvest_material_wood, 
            tileData.obj,
            1
        );
        job.building = building;
        jobManager.AddJob(job, worker);
    }

    public void CreateStoneJob(LevelData tileData, Building building, AIController worker)
    {
        Job job = new Job(
            tileData.obj.transform.position,
            JobType.harvest_material_stone,
            tileData.obj,
            1
        );
        job.building = building;
        jobManager.AddJob(job, worker);
    }

    public void CreateFarmJob(GameObject farm, AIController worker)
    {
        Job job = new Job(
            farm.transform.position,
            JobType.work_building_farm,
            farm,
            1
        );
        jobManager.AddJob(job, worker);
    }
}
