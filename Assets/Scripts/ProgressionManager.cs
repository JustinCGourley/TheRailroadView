using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    [SerializeField] List<ObjectInfo> fullBuildingList = new List<ObjectInfo>();
    public List<ObjectInfo> GetAllBuildings() { return fullBuildingList; }

    List<ObjectInfo> unlockedBuildings = new List<ObjectInfo>();
    public List<ObjectInfo> GetUnlockedBuildings() { return unlockedBuildings; }
    List<ObjectInfo> buildingPool = new List<ObjectInfo>();

    [SerializeField] List<ObjectInfo> pipeBuildings = new List<ObjectInfo>();
    public List<ObjectInfo> GetPipeBuildings() { return pipeBuildings; }

    [SerializeField] int currentDay;
    public int CurrentDay { get { return currentDay; } }

    Dictionary<ObjectInfo, BuildingUpgrades> buildingUpgradeDict;


    // Start is called before the first frame update
    void Start()
    {
        buildingUpgradeDict = new Dictionary<ObjectInfo, BuildingUpgrades>();
        int id = 0;
        foreach (ObjectInfo obj in fullBuildingList)
        {
            int buildingIndex = 0;
            if (obj == null || obj.obj == null)
            {
                Debug.LogError($"Cannot find object for {obj.buildingName}");
            }
            obj.obj.GetComponent<Building>().buildingId = id;
            obj.obj.GetComponent<Building>().buildingTier = buildingIndex;
            id++;
            buildingIndex++;

            if (obj.isStartBuilding)
            {
                UnlockBuilding(obj);
            }

            buildingUpgradeDict.Add(obj, new BuildingUpgrades());
        }

        currentDay = 0;
    }

    public void UnlockBuilding(ObjectInfo objInfo)
    {
        unlockedBuildings.Add(objInfo);
        unlockedBuildings[unlockedBuildings.Count - 1].SetUpgrade(0);
    }

    public void BuildPool()
    {
        List<ObjectInfo> pool = new List<ObjectInfo>();

        //adds all the non-unlocked buildings to the pool
        foreach (ObjectInfo obj in fullBuildingList)
        {
            if (!unlockedBuildings.Contains(obj))
            {
                pool.Add(obj);
            }
        }

        buildingPool = pool;
        Debug.Log($"Built pool! [{buildingPool.Count}]");
    }

    public ObjectInfo GetRandomItemFromPool(BuildingType buildingType, List<ObjectInfo> usedBuildings)
    {
        List<ObjectInfo> validItems = new List<ObjectInfo>();
        foreach (ObjectInfo obj in buildingPool)
        {
            if (obj.buildingType == buildingType && !usedBuildings.Contains(obj))
            {
                validItems.Add(obj);
            }
        }

        if (validItems.Count == 0) 
        {
            return null;
        }
        return validItems[Random.Range(0, validItems.Count-1)];
    }

    public List<ObjectInfo> GetPool()
    {
        return buildingPool;
    }

    public void UpgradeBuilding(ObjectInfo info, int level)
    {
        unlockedBuildings.Find(x => x == info).SetUpgrade(level);
    }

    public void UpgradeBuilding(ObjectInfo building, BuildingUpgrades upgrades)
    {
        if (!buildingUpgradeDict.ContainsKey(building))
        {
            Debug.LogError($"Unable to find building {building.name} in the upgrade dictionary");
            return;
        }
        BuildingUpgrades buildingUpgrade = buildingUpgradeDict[building];

        buildingUpgrade.Add(upgrades);
    }

    public ObjectInfo GetObjectInfoFromBuilding(Building obj)
    {
        //pipe building!
        if (obj.buildingId == 9999)
        {
            return GameManager.Instance.GetPipeObjectInfo();
        }
        if (obj.buildingId == -1)
        {
            return GameManager.Instance.GetNexusInfo();
        }

        foreach (ObjectInfo info in unlockedBuildings)
        {
            if (info.obj.GetComponent<Building>().buildingId == obj.buildingId)
            {
                return info;
            }
        }


        Debug.LogError("Unable to find clicked building?");
        return null;
    }

    public void ProgressDay()
    {
        currentDay++;
    }
}
