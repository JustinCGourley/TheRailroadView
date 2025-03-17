using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class JobManager : MonoBehaviour
{

    public static JobManager Instance { get; private set; }
    
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

    List<Job> fullJobQueue;
    List<Job> jobQueue;
    List<Job> recentlyCompleted;
    List<Job> uncompleteJobs;
    AIManager aiManager;

    // Start is called before the first frame update
    void Start()
    {
        jobQueue = new List<Job>();
        fullJobQueue = new List<Job>();
        recentlyCompleted = new List<Job>();
        aiManager = GameObject.Find(Constants.GAMEOBJECT_AIMANAGER).GetComponent<AIManager>();
        uncompleteJobs = new List<Job>();
    }

    public void CreateDayJobs()
    {
        bool researcherSearch = false;
        bool researcherAuto = false;
        List<Building> researchBuildings = new List<Building>();
        List<Building> buildingsLackingWorkers = new List<Building>();

        foreach (Building building in GameManager.Instance.GetAllBuildings())
        {
            for (int i = 0; i < building.workerList.Count; i++)
            {
                // gathering resource type buildings
                if (building.towerType == TowerType.gather)
                {
                    // wood gathering
                    if (building.element == Element.earth)
                    {
                        CreateTreeJob(building, building.workerList[i]);
                    }
                    // stone gathering
                    else if (building.element == Element.mud)
                    {
                        CreateStoneJob(building, building.workerList[i]);
                    }
                }
                // research buildings
                else if (building.towerType == TowerType.research)
                {
                    if (building.element == Element.fire)
                    {
                        researcherSearch = true;
                    }
                    else if (building.element == Element.water)
                    {
                        researcherAuto = true;
                    }
                    else
                    {
                        researchBuildings.Add(building);
                        // only add the building once for research building
                        if (i < building.maxJobs)
                            i = building.maxJobs;
                    }
                }
                // farms
                else if (building.towerType == TowerType.farm)
                {
                    JobCreator.Instance.CreateFarmJob(building.gameObject, building.workerList[i]);
                }
            }
            building.SetWorkerUI();
        }

        //create researchers
        // we need to do this seperatly as we need to know whether we have research upgrade buildings or not
        for (int i = 0; i < researchBuildings.Count; i++)
        {
            for (int j = 0; j < researchBuildings[i].workerList.Count; j++)
            {
                Vector3 spawnPos = researchBuildings[i].transform.position;
                aiManager.GenerateResearchAI(spawnPos, researcherSearch, researcherAuto);
            }
        }

    }

    public void GetWorkersForUnitBuilding(GameObject buildingObj)
    {
        Building building = buildingObj.GetComponent<Building>();
        Building_UnitTower unitTower = buildingObj.GetComponent<Building_UnitTower>();

        int workersAvailable = GameManager.Instance.GetAvailableWorkers();
        int workersToSpend = unitTower.maxUnits;

        if (workersAvailable - unitTower.maxUnits < 0)
        {
            workersToSpend = workersAvailable;
        }

        for (int i = 0; i < workersToSpend; i++)
        {
            // get worker for building
            AIController ai = aiManager.GetFreeController();
            if (ai != null)
            {
                building.AddWorker(ai);
            }
            else
            {
                Debug.LogWarning("You just tried to add a worker that doesnt exist!");
            }
        }

        unitTower.SetUnitCount(workersToSpend);
    }

    public void RefillBuilding(Building building)
    {
        int availableWorkers = GameManager.Instance.GetAvailableWorkers();

        if (availableWorkers <= 0)
        {
            Debug.LogWarning("Cannot refill a building when you have no people to do so with");
            return;
        }

        int workersToSupply = (availableWorkers > (building.maxJobs - building.workerList.Count)) ? (building.maxJobs - building.workerList.Count) : availableWorkers;
        for (int i = 0; i < workersToSupply; i++)
        {
            building.AddWorker(aiManager.GetFreeController());
            Debug.Log("Added worker to " + building.name);
        }
    }

    public void SpendWorkersForBuilding(Building building)
    {
        for (int i = 0; i < building.maxJobs; i++)
        {
            AIController ai = aiManager.GetFreeController();
            if (ai == null)
            {
                Debug.LogWarning("Unable to get any free workers...");
                return;
            }
            else
            {
                building.AddWorker(ai);
            }
        }
        
    }

    private void CreateTreeJob(Building building, AIController worker)
    {
        //find the closest tree from nexus
        LevelData treeData = LevelManager.Instance.GetClosestObjectFromPosition(Utility.Instance.GetTileCoordsFromPosition(building.transform.position), ObjectType.tree);
        treeData.obj = TreeManager.Instance.SeperateTree(treeData.position);
        LevelManager.Instance.UpdateLevelData(treeData, treeData.position);
        JobCreator.Instance.CreateTreeJob(treeData, building, worker);
        LevelManager.Instance.ClaimLocation(treeData.position);

    }

    private void CreateStoneJob(Building building, AIController worker)
    {
        //find the closest tree from nexus

        LevelData stoneData = LevelManager.Instance.GetClosestObjectFromPosition(GameManager.Instance.GetNexusCoords(), ObjectType.rock);
        JobCreator.Instance.CreateStoneJob(stoneData, building, worker);
    }

    private void AddJob(AIController ai, Job job)
    {
        job.workers.Add(ai);

        if (job.workers.Count == job.maxWorkerCount)
        {
            fullJobQueue.Add(job);
            jobQueue.Remove(job);
        }
    }

    // add job for a random worker
    public void AddJob(Job job)
    {
        if (CheckJobDuplicate(job)) { Debug.LogWarning("Attempted to create duplicate job"); return; }
        jobQueue.Add(job);
    }
    // Add job to a spcific worker
    public void AddJob(Job job, AIController worker)
    {
        if (CheckJobDuplicate(job)) { Debug.LogWarning("Attempted to create duplicate job"); return; }
        if (worker == null)
        { 
            Debug.LogWarning("Attempted to give job to null worker..."); 
            jobQueue.Add(job); 
            return; 
        }

        worker.StartJob(job);
    }

    public void AddJobToQueue(Job job, int priority)
    {
        jobQueue.Insert(priority, job);
    }

    public void CompleteJob(Job job)
    {
        if (HasRecentlyCompelted(job)) { return; }
        if (job == null)
        {
            return;
        }

        List<Job> list = (job.workers.Count == job.maxWorkerCount) ? fullJobQueue : jobQueue;
        
        for (int i = 0; i < list.Count; i++)
        {
            if (job.Equals(list[i]))
            {
                list.RemoveAt(i);
                recentlyCompleted.Add(job);
                StartCoroutine(ClearRecentlyCompleted());
                return;
            }
        }
    }

    private bool HasRecentlyCompelted(Job job)
    {
        for (int i = 0; i < recentlyCompleted.Count; i++)
        {
            if (recentlyCompleted[i].Equals(job))
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator ClearRecentlyCompleted()
    {
        yield return new WaitForSeconds(10f);

        this.recentlyCompleted.RemoveAt(0);

        yield return null;
    }

    private bool CheckJobDuplicate(Job job)
    {
        foreach (Job j in fullJobQueue)
        {
            if (j.Equals(job))
            {
                return true;
            }
        }

        foreach (Job j in jobQueue)
        {
            if (j.Equals(job))
            {
                return true;
            }
        }

        return false;
    }

    public int GetPopulationCount()
    {
        return aiManager.GetTotalWorkerCount();
    }

}
