using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.UI.CanvasScaler;
using Random = UnityEngine.Random;


public class AIManager : MonoBehaviour
{
    public Transform aiTransform;
    public GameObject aiObj;

    //should be as follows:
    // 0 - basic unit
    // 1 - sword unit
    // 2 - ogre unit
    // 3 - mage unit
    // 4 - ranged unit
    public List<GameObject> aiUnitObjs;
    public GameObject aiResearcherObj;

    [SerializeField] GameObject flockObj;

    private List<AIController> workers;

    private List<AIUnitController> units;
    private List<AIResearcherController> researchers;

    // Start is called before the first frame update
    void Start()
    {
        workers = new List<AIController>();
        units = new List<AIUnitController>();
        researchers = new List<AIResearcherController>();
    }

    //Unit AI Stuff
    // =================================================================================

    public void GenerateUnitAI(Building_UnitTower tower, AIUnitType type)
    {
        AIInfo aiInfo = new AIInfo();
        aiInfo.speed = 1.0f;
        aiInfo.unitBuilding = tower;
        aiInfo.unitNumber = units.Count + 1;

        AIUnitController controller = Instantiate(aiUnitObjs[(int)type], tower.transform.position, Quaternion.identity, aiTransform).GetComponent<AIUnitController>();
        controller.info = aiInfo;

        units.Add(controller);
    }

    public AIUnitController GenerateUnitAIGroup(Building_UnitTower tower, AIUnitType type, int count)
    {
        List<AIUnitController> newUnits = new List<AIUnitController>();
        AIUnitController leader = null;
        for (int i = 0; i < count; i++)
        {
            GenerateUnitAI(tower, type);
            if (i == 0)
            {
                leader = units[units.Count - 1];
            }
            else
            {
                units[units.Count - 1].SetLeader(leader);
                newUnits.Add(units[units.Count - 1]);
            }
        }
        FlockGroup flock = Instantiate(flockObj, leader.transform).GetComponent<FlockGroup>();
        leader.SetLeader(flock, newUnits, tower);
        flock.SetupGroup(newUnits.Count);

        return leader;
    }

    public List<T> GetActiveControllers<T>(bool isUnits)
    {
        List<T> list = new List<T>();

        if (isUnits)
        {
            list.AddRange((IEnumerable<T>)units);
        }
        else
        {
            list.AddRange((IEnumerable<T>)researchers);
        }

        return list;
    }

    public List<AIUnitController> GetUnitsInRange(Vector3 pos, float radius)
    {
        List<AIUnitController> unitsInRange = new List<AIUnitController>();

        for (int i = 0; i < units.Count; i++)
        {
            if ((pos - units[i].transform.position).magnitude <= radius)
            {
                unitsInRange.Add(units[i]);
            }
        }

        return unitsInRange;
    }

    public void KillUnit(AIUnitController unit)
    {
        units.Remove(unit);

        if (unit.IsLeader && unit.units.Count > 0)
        {
            Debug.Log("Leader died... setting new leader");
            AIUnitController newLeader = unit.units[0];
            newLeader.SetLeader(unit.flock, unit.units, unit.tower);
            unit.units.Remove(unit.units[0]);
        }

        Destroy(unit.gameObject);
    }

    // researcher stuff
    // =================================================================================

    public AIResearcherController GenerateResearchAI(Vector3 pos, bool hasSearch, bool hasAuto)
    {
        //TODO: change this to actual make groups or something idk
        AIInfo aiInfo = new AIInfo();
        aiInfo.speed = 1.0f;

        AIResearcherController controller = Instantiate(aiResearcherObj, pos, Quaternion.identity, aiTransform).GetComponent<AIResearcherController>();
        controller.info = aiInfo;
        controller.SetHasSearch(hasSearch);
        controller.SetHasAuto(hasAuto);
        researchers.Add(controller);

        return controller;
    }

    public void RemoveResearchers()
    {
        for (int i = 0; i < researchers.Count; i++)
        {
            Destroy(researchers[i].gameObject);
        }

        researchers = new List<AIResearcherController>();
    }

    // =================================================================================
    //                              Worker AI Stuff Here bucko :)
    // =================================================================================

    public void GenerateAI(Vector3 position, float rngOverride = -1)
    {
        AIInfo aiInfo = new AIInfo();
        aiInfo.speed = Random.Range(0.8f, 1.2f);

        AIController controller = Instantiate(aiObj, position, Quaternion.identity, aiTransform).GetComponent<AIController>();
        controller.info = aiInfo;
        controller.info.unitNumber = 1000 + workers.Count;
        workers.Add(controller);
    }

    public void RemoveAI(AIController person)
    {
        if (workers.Contains(person))
        {
            workers.Remove(person);
        }

        foreach (Building buildings in GameManager.Instance.GetAllBuildings())
        {
            if (buildings.workerList.Contains(person))
            {
                buildings.RemoveWorker(person);
            }
        }

        Destroy(person.gameObject);
    }

    public void KillRandomPerson()
    {
        RemoveAI(workers[Random.Range(0, workers.Count - 1)]);
    }

    public AIController AssignWorkerToJob(Job job)
    {
        AIController worker = GetFreeController();
        worker.StartJob(job);

        return worker;
    }

    public List<AIController> GetListOfAvailableWorkers()
    {
        List<AIController> availableWorkers = new List<AIController>(workers);
        foreach (Building building in GameManager.Instance.GetAllBuildings())
        {
            foreach (AIController controller in building.workerList)
            {
                try
                {
                    availableWorkers.Remove(controller);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Unable to remove 'available' worker {controller.name}");
                }
            }
        }

        return availableWorkers;
    }

    public AIController GetFreeController()
    {
        List<AIController> availableWorkers = GetListOfAvailableWorkers();

        if (availableWorkers.Count > 0)
        {
            return availableWorkers[UnityEngine.Random.Range(0, availableWorkers.Count)];
        }

        Debug.LogWarning("Unable to get a free controller...");
        return null;
    }

    public int GetAvailableWorkerCount()
    {
        return GetListOfAvailableWorkers().Count;
    }

    private void Update()
    {
        foreach (AIUnitController controller in units)
        {
            controller.UpdateController();
        }
        for (int i = 0; i < workers.Count; i++)
        {
            workers[i].UpdateController();
        }
        foreach (AIResearcherController researcher in researchers)
        {
            researcher.UpdateController();
        }
    }

    public bool AreAllWorkersDone()
    {
        return (workers.Count == 0);
    }

    public void FinishWorkerJobs()
    {
        Debug.Log("Force finishing jobs");

        Debug.Log($"Worker count: {workers.Count}");

        foreach (AIController worker in workers)
        {
            Debug.Log("Ending job");
            worker.ForceFinishJob();
        }
    }

    public int GetTotalWorkerCount()
    {
        return workers.Count;
    }

    public void EndDay()
    {
        int workerCount = workers.Count;
        for (int i = 0; i < workerCount; i++)
        {
            workers[0].EndDay();
        }

    }

    private List<AIController> GetAllPeople()
    {
        List<AIController> aiControllers = new List<AIController>();
        aiControllers.AddRange(workers);
        return aiControllers;
    }

    /// <summary>
    /// called by AIController to get a home for AI's who are new and dont have one yet
    /// </summary>
    /// <returns></returns>
    public Home FindHome(AIController controller)
    {
        List<Building> houses = GameManager.Instance.GetBuildingsOfType(TowerType.house);

        foreach (Building house in houses)
        {
            Home home = house.GetComponent<Home>();

            if (home.people.Count < home.maxPeople)
            {
                return home;
            }
        }

        return null;
    }

    public void TimeToEat()
    {
        foreach (AIController controller in GetAllPeople())
        {
            controller.Eat();
        }
    }

    public float GetAverageHappiness()
    {
        //hapiness ranges from 1 - 100
        //100 happiness means a person:
        // has a home
        // has not had any recent destruction of buildings
        // has excess for for at least 2 extra days 
        // has ammenities?

        List<AIController> people = GetAllPeople();
        float totalHappiness = 0;
        foreach (AIController person in people)
        {
            totalHappiness += person.CalculateHappiness();
        }
        
        totalHappiness /= people.Count;

        if (people.Count == 0)
        {
            totalHappiness = 100;
        }

        return totalHappiness;
    }

    // every cycle we will add or remove people based on the average happiness
    // if we are bellow a threshold (50 for now) of total average happiness, we will remove some people
    // if we are above the treshold, we will add people
    public void CyclePeople()
    {
        int peopleToAdd = 0;

        float averageHappiness = GetAverageHappiness();


        Debug.Log($"Cycling people |happiness: {averageHappiness}");
        //add people
        if (averageHappiness >= 50)
        {
            if (averageHappiness <= 60)
            {
                peopleToAdd = Random.Range(1, 2);
            }
            else if (averageHappiness <= 80)
            {
                peopleToAdd = Random.Range(2, 4);
            }
            else if (averageHappiness <= 90)
            {
                peopleToAdd = Random.Range(3, 5);
            }
            else if (averageHappiness > 90)
            {
                peopleToAdd = Random.Range(5, 7);
            }

            Debug.Log($"adding {peopleToAdd} more people to the town!");
            UIManager.Instance.AddInfoMessage($"{peopleToAdd} people have joined the town!");
            for (int i = 0; i < peopleToAdd; i++)
            {
                GenerateAI(GameManager.Instance.GetNexusPosition());
            }
        }
        //remove people
        else
        {
            foreach (AIController person in GetAllPeople())
            {
                if (person.CalculateHappiness() <= 30)
                {
                    // if person is too low happiness, they have a 10% chance to just leave
                    if (Random.Range(1, 10) == 1)
                    {
                        Debug.Log("A guy has left the town rip");
                        UIManager.Instance.AddInfoMessage($"A townsmember has left the city");
                        peopleToAdd--;
                        RemoveAI(person);
                    }
                }
            }
        }

        int totalSoldiers = 0;
        foreach (Building_UnitTower unit in GameManager.Instance.GetUnitBuildings())
        {
            totalSoldiers += unit.currentUnits;
        }
        // update UI
        UIManager.Instance.UpdateHappinessUI(averageHappiness, GetTotalWorkerCount(), totalSoldiers);
    }
}
