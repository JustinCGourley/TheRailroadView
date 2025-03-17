using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TowerType
{
    none,
    auto,
    controlled,
    unit,
    farm,
    gather,
    storage,
    house,
    pillar,
    pipe,
    research,
    nexus,
    attack, // this is used PURELY for sorting in building menu (towers should NOT have this type)
    support, // this is used PURELY for sorting in building menu (towers should NOT have this type)
    city // this is used PURELY for sorting in building menu (towers should NOT have this type)
}

public abstract class Building : MonoBehaviour
{
    public Element element = Element.none;
    public TowerType towerType = TowerType.none;
    public int maxJobs; // only used for city buildings
    public abstract bool doesBuildingUpdate(); //does building update during night phase
    public abstract bool isTower();
    public abstract bool doesBuildingUpdateInDay();

    public abstract void buildingStart(Vector3 pos, Vector2Int coords);
    public abstract void buildingUpdate();
    public int buildingId;
    public int buildingTier;
    public bool isPiped;
    public List<AIController> workerList = null;
    GameObject alertUI;

    public void AddElement(Element element)
    {
        Debug.Log($"Checking Element baby | curEle: {this.element} - new ele: {element}");
        Element newElement = ElementManager.Instance.CheckElementCombo(new List<Element> { element, this.element});
        if (newElement != Element.none)
        {
            this.element = newElement;
            Debug.Log($"Setting tower {this.gameObject.name} element to {this.element}");
        }
    }
    public void AddWorker(AIController worker)
    {
        if (workerList == null)
        {
            workerList = new List<AIController>();
        }

        workerList.Add(worker);

        SetWorkerUI();
    }

    public void RemoveWorker(AIController worker)
    {
        if (workerList == null)
        {
            Debug.LogWarning($"Unable to remove worker '{worker.name}'");
            return;
        }

        workerList.Remove(worker);

        SetWorkerUI();
    }

    public void SetWorkerUI()
    {
        NeedWorkerUI(workerList.Count < maxJobs);
    }

    public void NeedWorkerUI(bool active)
    {
        if (active)
        {
            // create the sprite
            GameObject obj = Instantiate(AlertUI.Instance.needWorkerUIObj, this.transform);
            alertUI = obj;
        }
    }
}
