using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ResourceType
{
    wood,
    stone,
    food,
    researchOrb,
}
public struct Resource
{
    public ResourceType type;
    public GameObject obj;
}

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

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

    public List<GameObject> resourceObjs;
    public Transform resourceListTransform;


    List<Resource> resources;
    List<int> resourceCount;

    private int woodCount;
    public int WoodCount { get { return woodCount; } }
    private int stoneCount;
    public int StoneCount { get { return stoneCount; } }
    private int researchOrbCount;
    public int ResearchOrbCount { get { return researchOrbCount; } }

    private int foodCount;
    public int FoodCount { get { return foodCount; } }

    private void Start()
    {
        resources = new List<Resource>();

        woodCount = 40;
        stoneCount = 20;
        researchOrbCount = 0;
        foodCount = 20;

        UpdateResourceText();
    }

    public Resource SpawnResource(ResourceType resourceType, Vector3 position)
    {
        GameObject obj = Instantiate(resourceObjs[(int)resourceType], position, Quaternion.Euler(0, Random.Range(0, 360), 0), resourceListTransform);
        Resource resource = new Resource();
        resource.type = resourceType;
        resource.obj = obj;
        resources.Add(resource);
        return resource;
    }

    public void RemoveResource(Resource resource)
    {
        Destroy(resource.obj);
        resources.Remove(resource);
    }

    public bool SpendResources(int wood, int stone)
    {
        if (woodCount >= wood && stoneCount >= stone)
        {
            woodCount -= wood;
            stoneCount -= stone;

            UpdateResourceText();
            return true;
        }
        else
        {
            UIManager.Instance.AddInfoMessage("You dont have the resources for this!");
            Debug.LogWarning("Unable to spend resources *This shoudlnt happen*");
            return false;
        }
    }

    public bool SpendResources(ObjectInfo info)
    {
        return SpendResources(info.woodCost, info.stoneCost);
    }

    public bool SpendResource(ResourceType resourceType, int amount)
    {
        switch (resourceType)
        {
            case ResourceType.wood:
                if (!(woodCount - amount >= 0))
                {
                    Debug.Log("Unable to spend these resources");
                    UIManager.Instance.AddInfoMessage("You dont have enough wood");
                    return false;
                }
                woodCount -= amount;
                break;
            case ResourceType.stone:
                if (!(stoneCount - amount >= 0))
                {
                    Debug.Log("Unable to spend these resources");
                    UIManager.Instance.AddInfoMessage("You dont have enough stone");
                    return false;
                }
                stoneCount -= amount;
                break;
            case ResourceType.researchOrb:
                if (!(researchOrbCount - amount >= 0))
                {
                    Debug.Log("Unable to spend these resources");
                    UIManager.Instance.AddInfoMessage("You dont have enough research orbs");
                    return false;
                }
                researchOrbCount -= amount;
                UIManager.Instance.UpdateResearchOrbText();
                break;
            case ResourceType.food:
                if (!(foodCount - amount >= 0))
                {
                    Debug.Log("Unable to spend these resources");
                    UIManager.Instance.AddInfoMessage("You dont have enough food");
                    return false;
                }
                foodCount -= amount;
                break;
            default:
                Debug.LogError("Something went wrong with spending resource?");
                return false;
        }

        UpdateResourceText();
        return true;
    }

    public void AddResource(ResourceType resourceType, int amount)
    {
        switch (resourceType)
        {
            case ResourceType.wood:
                woodCount += amount;
                break;
            case ResourceType.stone:
                stoneCount += amount;
                break;
            case ResourceType.researchOrb:
                researchOrbCount += amount;
                Debug.Log("Collected research orb! CurCount: " + researchOrbCount);
                UIManager.Instance.UpdateResearchOrbText();
                break;
            case ResourceType.food:
                foodCount += amount;
                break;
            default:
                Debug.LogWarning("Invalid resource given???");
                break;
        }

        UpdateResourceText();
    }

    public bool CheckResourceCost(int woodAmount, int stoneAmount, int personAmount)
    {
        if (woodCount < woodAmount || stoneCount < stoneAmount || (GameManager.Instance.GetAvailableWorkers() - personAmount < 0 && personAmount != 0))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool CheckResearchCost(int researchCost)
    {
        return researchOrbCount >= researchCost;
    }

    private void UpdateResourceText()
    {
        UIManager.Instance.UpdateResourceUI(woodCount, stoneCount, foodCount);
    }
}
