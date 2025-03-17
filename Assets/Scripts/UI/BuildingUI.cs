using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

enum BuildingUIType
{
    generic,
    tower,
    unit,
    pipe,
    pillar
}

public class BuildingUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI buildingName;
    [SerializeField] TextMeshProUGUI statsText;

    [SerializeField] Button upgradeButton;
    [SerializeField] TextMeshProUGUI upgradeButtonText;
    [SerializeField] TextMeshProUGUI upgradeButtonCostText;

    [SerializeField] BuildingUIType uiType;

    [SerializeField] GameObject workerPersonObj;
    [SerializeField] Transform workerUIParent;
    [SerializeField] Color workerPersonInactiveColor;
    [SerializeField] GameObject workerUI;

    private Building curBuildingSelected = null;

    public void SetupText(ObjectInfo objInfo, GameObject obj)
    {

        Building building = obj.GetComponent<Building>();
        curBuildingSelected = building;
        string stats = "";
        switch (building.towerType)
        {
            case TowerType.auto:
                uiType = BuildingUIType.tower;
                Building_Tower t = building as Building_Tower;
                stats = $"Element: {t.element}\nDamage: {t.projectileInfo.damage}\nSpeed: {t.towerShotSpeed}\nRange: {t.range}";
                break;
            case TowerType.unit:
                Building_UnitTower ut = building as Building_UnitTower;
                stats = $"Trained Units: {ut.currentUnits}\nMax Units: {ut.maxUnits}";
                break;
            case TowerType.controlled:
                uiType = BuildingUIType.tower;
                Building_ControlledTower ct = building as Building_ControlledTower;
                stats = $"Element: {ct.element}\nDamage: {ct.projectile.damage}\nSpeed: {ct.projectile.speed}\nRange: {ct.towerRange}";
                break;
            case TowerType.house:
                
                break;
            case TowerType.pillar:


                break;
            case TowerType.pipe:
                uiType = BuildingUIType.pipe;

                stats = $"its a pipeline" + (GameManager.Instance.IsSelectedPipelineComplete(obj) ? "\nA completed one!" : "");
                break;
            default:

                break;
        }

        buildingName.text = $"{objInfo.buildingName}" + ((building.towerType == TowerType.auto || building.towerType == TowerType.controlled) ? $" lvl {building.buildingTier}" : "");

        if (uiType == BuildingUIType.pipe || uiType == BuildingUIType.tower)
        {
            upgradeButton.gameObject.SetActive(true);

            if (building.towerType == TowerType.controlled || building.towerType == TowerType.auto)
            {
                upgradeButtonText.text = "Upgrade Tower";
                upgradeButton.interactable = building.buildingTier < objInfo.CurUpgrade;
                if (upgradeButton.interactable)
                {
                    upgradeButtonCostText.text = $"- Cost -\nWood: {objInfo.woodCost}\nStone: {objInfo.stoneCost}";
                }
                else
                {
                    upgradeButtonCostText.text = "";
                }
            }
            else if (building.towerType == TowerType.pipe)
            {
                upgradeButtonText.text = "Edit Pipeline";
                upgradeButtonCostText.text = "";
                buildingName.text = "Pipeline";
                upgradeButton.interactable = !GameManager.Instance.IsSelectedPipelineComplete(obj);
            }
            else
            {
                upgradeButton.gameObject.SetActive(false);
            }
        }
        else if (uiType == BuildingUIType.pillar)
        {
            upgradeButton.interactable = GameManager.Instance.IsSelectedPillarFull(building);
        }
        statsText.text = stats;

        // if there are any jobs, show the job UI stuff
        if (building.maxJobs > 0)
        {
            workerUI.SetActive(true);
            UpdateWorkerUI(building);
        }
        else
        {
            workerUI.SetActive(false);
        }
    }

    void UpdateWorkerUI(Building building)
    {
        // remove previous
        int childCount = workerUIParent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(workerUIParent.GetChild(i).gameObject);
        }

        // add in current
        for (int i = 0; i < building.maxJobs; i++)
        {
            GameObject obj = Instantiate(workerPersonObj, workerUIParent);
            Vector2 pos = obj.transform.localPosition;
            pos.x += Constants.UI_WORKER_UI_SPACING * i;
            obj.transform.localPosition = pos;

            if (i > building.workerList.Count - 1)
            {
                obj.GetComponent<RawImage>().color = workerPersonInactiveColor;
            }
        }
    }

    public void RefillButtonPressed()
    {
        JobManager.Instance.RefillBuilding(curBuildingSelected);

        UpdateWorkerUI(curBuildingSelected);
    }

    public void UpgradeBuilding()
    {
        if (uiType == BuildingUIType.pillar)
        {
            GameManager.Instance.StartPipeBuild();
        }
        else if (uiType == BuildingUIType.tower)
        {
            GameManager.Instance.UpgradeSelectedBuilding();
        }
        else if (uiType == BuildingUIType.pipe)
        {
            GameManager.Instance.EditPipeBuild();
        }
    }

    public void SellSelected()
    {
        GameManager.Instance.SellSelectedBuilding();
    }
}
