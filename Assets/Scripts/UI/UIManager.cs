using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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

    [SerializeField] GameObject buildingMenu;
    [SerializeField] GameObject workMenuObj;

    [SerializeField] GameObject startDayButtonObj;
    [SerializeField] GameObject endDayButtonObj;
    [SerializeField] TextMeshProUGUI cursorText;

    [SerializeField] GameObject bottomLeftMenu;

    WorkMenu workMenu;

    [SerializeField] RectTransform healthUI;
    [SerializeField] TextMeshProUGUI healthUIText;
    Vector3 healthStartPos;
    float healthStartWidth;

    [SerializeField] RectTransform selector;

    [SerializeField] GameObject researchUI;
    [SerializeField] GameObject useTowerUIObj;

    ResourceUI resourceUI;
    HappinessUI happinessUI;
    InfoUI infoUI;
    UseTowerUI useTowerUI;
    ManaUI manaUI;
    [SerializeField] BuildingUI buildingUI;
    [SerializeField] BuildingUI buildingPillarUI;
    [SerializeField] TextMeshProUGUI dayText;

    // Start is called before the first frame update
    void Start()
    {
        workMenu = this.GetComponent<WorkMenu>();

        resourceUI = this.GetComponent<ResourceUI>();
        happinessUI = this.GetComponent<HappinessUI>();

        infoUI = this.GetComponent<InfoUI>();

        useTowerUI = this.GetComponent<UseTowerUI>();
        
        manaUI = this.GetComponent<ManaUI>();

        healthStartWidth = healthUI.rect.width;
    }

    public void CloseAllMenus()
    {
        buildingMenu.SetActive(false);
        workMenuObj.SetActive(false);
        researchUI.SetActive(false);
    }

    public void CycleBuildingMenu()
    {
        if (buildingMenu.activeSelf)
        {
            buildingMenu.SetActive(false);
            return;
        }
        CloseAllMenus();
        SetupBuildingMenu();
        buildingMenu.SetActive(!buildingMenu.activeSelf);
    }

    public void OpenWorkMenu()
    {
        if (workMenuObj.activeSelf)
        {
            workMenuObj.SetActive(false);
            return;
        }
        CloseAllMenus();
        workMenu.UpdateMenu();
        workMenuObj.SetActive(!workMenuObj.activeSelf);
        if (researchUI.activeSelf)
        {
            researchUI.SetActive(false);
        }
    }

    public void UpdateWorkMenu()
    {
        workMenu.UpdateMenu();
    }

    public void CloseWorkMenu()
    {
        workMenuObj.SetActive(false);
    }

    public void OpenResearchMenu(bool active = true)
    {
        researchUI.SetActive(active);
        if (active && workMenuObj.activeSelf)
        {
            CloseWorkMenu();
        }
    }

    public void FlipResearchMenu()
    {
        if (researchUI.activeSelf)
        {
            researchUI.SetActive(false);
            return;
        }
        CloseAllMenus();
        OpenResearchMenu(!researchUI.activeSelf);
    }

    public void SetStartButton(bool state)
    {
        startDayButtonObj.SetActive(state);
    }

    public void SetEndButtonActive(bool state)
    {
        endDayButtonObj.SetActive(state);
    }

    public int GetSliderValue(string sliderType)
    {
        return workMenu.GetSliderValue(sliderType);
    }

    public int GetTotalGatherSpots()
    {
        return workMenu.GetTotalGatherSpots;
    }

    public List<int> GetTrainingSliderValues()
    {
        return workMenu.GetTrainingSliderValues();
    }

    public bool IsOverProvisioned()
    {
        return workMenu.IsOverProvisioned();
    }

    public void SetCursorTextAndPosition(string text, Vector3 position)
    {
        cursorText.text = text;
        cursorText.rectTransform.localPosition = position;
    }

    public void SetupBuildingMenu()
    {
        buildingMenu.GetComponent<BuildingMenu>().SetupMenu();
    }

    public void OpenBuildingMenu(bool open = true)
    {
        SetupBuildingMenu();
        buildingMenu.SetActive(open);
    }

    public void SetBottomMenuActive(bool active)
    {
        bottomLeftMenu.SetActive(active);
        workMenu.CloseMenu();
    }

    public void UpdateHealthUI(float health, float maxHealth)
    {
        float healthScale = health / maxHealth;
        healthUI.sizeDelta = new Vector2(healthStartWidth * healthScale, 31);

        healthUIText.text = $"{health} / {maxHealth}";
    }

    public void UpdateSelectorUI(Vector2 start, Vector2 end, bool active = true)
    {
        if (selector.gameObject.activeSelf != active)
        {
            selector.gameObject.SetActive(active);
        }

        //Vector2 size = new Vector2(end.x - start.x, end.y - start.y);
        //Vector2 centerPoint = start + (end - start) - (size/2);


        float cX = Utility.Instance.GetPositionCenter(start.x, end.x);
        float cY = Utility.Instance.GetPositionCenter(start.y, end.y);

        float sizeX = Utility.Instance.GetSizeFromCenter(cX, start.x);
        float sizeY = Utility.Instance.GetSizeFromCenter(cY, start.y);


        Vector2 center = new Vector2(cX, cY);
        Vector2 size = new Vector2(sizeX, sizeY) * 2;

        selector.anchoredPosition = center;
        selector.sizeDelta = size;
    }

    public void UpdateResourceUI(int wood, int stone, int food)
    {
        if (resourceUI == null) resourceUI = this.GetComponent<ResourceUI>();
        resourceUI.SetText(wood, stone, food);
    }

    public void UpdateHappinessUI(float happiness, int totalPop, int totalSoldiers)
    {
        happinessUI.SetHappinessText(happiness, totalPop, totalSoldiers);
    }

    public void UpdatePopulationUI()
    {
        happinessUI.SetPopText();
    }

    public void AddInfoMessage(string message)
    {
        infoUI.AddMessage(message);
    }

    public void SetUpUseTowerUI(List<Building_ControlledTower> controlled)
    {
        useTowerUI.CreateTowerUIButtons(controlled);
        useTowerUIObj.SetActive(true);

    }

    public void UpdateUseTowerUI(UseButtonUI selected)
    {
        useTowerUI.UpdateButtons(selected);
    }

    public void CloseUseTowerUI()
    {
        useTowerUI.ClearButtons();
        useTowerUIObj.SetActive(false);
    }

    public void UpdateManaUI(int amount, int max)
    {
        manaUI.UpdateManaAmount(amount, max);
    }

    public void OpenBuildingInfoUI(ObjectInfo objInfo, GameObject obj)
    {
        CloseBuildingInfoUI();
        TowerType towerType = obj.GetComponent<Building>().towerType;
        if (towerType == TowerType.pillar)
        {
            OpenBuildingPillarInfoUI(objInfo, obj);
        }
        else
        {
            buildingUI.SetupText(objInfo, obj);
            buildingUI.gameObject.SetActive(true);
        }
    }

    public void CloseBuildingInfoUI()
    {
        buildingUI.gameObject.SetActive(false);
        CloseBuildingPillarInfoUI();
    }

    void OpenBuildingPillarInfoUI(ObjectInfo objInfo, GameObject obj)
    {
        Debug.Log("Opening buildingPillar UI");
        buildingPillarUI.SetupText(objInfo, obj);
        buildingPillarUI.gameObject.SetActive(true);
    }

    public void CloseBuildingPillarInfoUI()
    {
        buildingPillarUI.gameObject.SetActive(false);
    }

    public void UpdateDayText()
    {
        dayText.text = $"Day {ProgressionManager.Instance.CurrentDay}";
    }

    public void UpdateResearchOrbText()
    {
        researchUI.GetComponent<ResearchManager>().UpdateResearchOrbText();
    }
}
