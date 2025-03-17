using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

struct PanelItem
{
    public GameObject building;
    public ObjectInfo buildingInfo;
    public int cost;
    public bool bought;
}
struct Panel
{
    public GameObject panelObj;
    public List<PanelItem> panelItems;
}
public class ResearchManager : MonoBehaviour
{
    [SerializeField] GameObject panelObj;
    [SerializeField] GameObject upgradeContentUI;
    [SerializeField] GameObject upgradeButtonUI;
    [SerializeField] GameObject upgradeMenu;
    [SerializeField] GameObject unlockMenu;
    [SerializeField] Transform contentTransform;
    [SerializeField] Transform contentUpgradeTransform;

    [SerializeField] Button upgradeButton;
    [SerializeField] Button unlockButton;

    [SerializeField] TextMeshProUGUI researchOrbText;

    List<Panel> panels;

    List<ObjectInfo> usedBuildings;

    private void Start()
    {
        usedBuildings = new List<ObjectInfo>();
        panels = new List<Panel>();
        AddPanel();
        SetupUpgradeMenu();
        OpenMenu(0);
    }

    public void AddPanel()
    {
        //get building infos from progression manager
        ProgressionManager.Instance.BuildPool();

        ObjectInfo attackObj = ProgressionManager.Instance.GetRandomItemFromPool(BuildingType.attack, usedBuildings);
        if (attackObj != null) usedBuildings.Add(attackObj);
        ObjectInfo supportObj = ProgressionManager.Instance.GetRandomItemFromPool(BuildingType.support, usedBuildings);
        if (supportObj != null) usedBuildings.Add(supportObj);
        ObjectInfo cityObj = ProgressionManager.Instance.GetRandomItemFromPool(BuildingType.city, usedBuildings);
        if (cityObj != null) usedBuildings.Add(cityObj);

        Panel panel = new Panel();
        panel.panelObj = CreatePanelObj();
        panel.panelItems = CreatePanelItems(attackObj, supportObj, cityObj);
        UpdatePanelUI(panel, attackObj, supportObj, cityObj);

        panels.Add(panel);
    }

    private void UpdatePanelUI(Panel panel, ObjectInfo attackObj, ObjectInfo supportObj, ObjectInfo cityObj)
    {
        /*
         * Panel Structure:
         * 
         * Attack Item Picture -> text
         * Support Item Picture -> text
         * City Item Picture -> text
         * 
         */
        if (attackObj != null)
        {
            TextMeshProUGUI attackItemText = panel.panelObj.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            attackItemText.text = $"{attackObj.buildingName}\nCost: ${attackObj.researchCost}";
        }
        else
        {
            Destroy(panel.panelObj.transform.GetChild(0).gameObject);
        }

        if (supportObj != null)
        {
            TextMeshProUGUI supportItemText = panel.panelObj.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            supportItemText.text = $"{supportObj.buildingName}\nCost: ${supportObj.researchCost}";
        }
        else
        {
            Destroy(panel.panelObj.transform.GetChild(1).gameObject);
        }

        if (cityObj != null)
        {
            TextMeshProUGUI cityItemText = panel.panelObj.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
            cityItemText.text = $"{cityObj.buildingName}\nCost: ${cityObj.researchCost}";
        }
        else
        {
            Destroy(panel.panelObj.transform.GetChild(2).gameObject);
        }

    }

    private List<PanelItem> CreatePanelItems(ObjectInfo attackObj, ObjectInfo supportObj, ObjectInfo cityObj)
    {
        PanelItem attackItem = new PanelItem();
        if (attackObj != null)
        { 
            attackItem.building = attackObj.obj;
            attackItem.buildingInfo = attackObj;
            attackItem.cost = attackObj.researchCost;
            attackItem.bought = false;
        }
        else
        {
            attackItem.building = null;
        }

        PanelItem supportItem = new PanelItem();
        if (supportObj != null)
        {
            supportItem.building = supportObj.obj;
            supportItem.buildingInfo = supportObj;
            supportItem.cost = supportObj.researchCost;
            supportItem.bought = false;
        }
        else
        {
            supportItem.building = null;
        }

        PanelItem cityItem = new PanelItem();
        if (cityObj != null)
        {
            cityItem.building = cityObj.obj;
            cityItem.buildingInfo = cityObj;
            cityItem.cost = cityObj.researchCost;
            cityItem.bought = false;
        }
        else
        {
            cityItem.building = null;
        }


        return new List<PanelItem> { attackItem, supportItem, cityItem };
    }

    private GameObject CreatePanelObj()
    {
        return Instantiate(panelObj, contentTransform);
    }

    public void ClickedPanel(GameObject panel, int panelNum)
    {
        Panel foundPanel = FindPanel(panel);

        if (foundPanel.panelObj == null)
        {
            Debug.LogError("Panel wasn't found");
            return;
        }
        

        if (ResourceManager.Instance.SpendResource(ResourceType.researchOrb, foundPanel.panelItems[panelNum].cost))
        {
            PanelItem item = foundPanel.panelItems[panelNum];
            item.bought = true;
            foundPanel.panelItems[panelNum] = item;
            ProgressionManager.Instance.UnlockBuilding(item.buildingInfo);
            Debug.Log("Trying to delete " + foundPanel.panelObj.transform.GetChild(panelNum).GetChild(1));
            foundPanel.panelObj.transform.GetChild(panelNum).GetChild(1).GetComponent<Button>().interactable = false;

            //unlock new panel set
            if (foundPanel.Equals(panels[panels.Count-1]))
            {
                AddPanel();
            }
        }
    }

    Panel FindPanel(GameObject panelToFind)
    {
        foreach (Panel panel in panels)
        {
            if (panel.panelObj == panelToFind)
            {
                return panel;
            }
        }

        return new Panel();
    }

    public void UpdateResearchOrbText()
    {
        researchOrbText.text = $"Research Orbs: {ResourceManager.Instance.ResearchOrbCount}";
    }


    // ================================================ below is for upgrade UI =====================================================

    // TODO -> REDO THIS SHIT
    public void SetupUpgradeMenu()
    {
        ClearUpgradeMenu();

        List<ObjectInfo> unlockedBuildings = ProgressionManager.Instance.GetUnlockedBuildings();

        foreach (ObjectInfo unlockedBuilding in unlockedBuildings)
        {
            if (unlockedBuilding.isUpgradable)
            {
                GameObject menu = CreateMenuObj();
                Transform menuContent = menu.transform.Find("UpgradeButtons");

                SetupButtonsForBuilding(unlockedBuilding);
                
            }
        }
    }

    void SetupButtonsForBuilding(ObjectInfo building)
    {
        building.obj
    }

    private void ClearUpgradeMenu()
    {
        foreach (Transform t in contentUpgradeTransform)
        {
            Destroy(t.gameObject);
        }
    }

    private GameObject CreateMenuObj()
    {
        return Instantiate(upgradeContentUI, contentUpgradeTransform);
    }

    private GameObject CreateMenuButtonObj(Transform menuTransform)
    {
        return Instantiate(upgradeButtonUI, menuTransform);
    }


    // ================================================ below is for other stuff =====================================================


    public void OpenMenu(int menuIndex)
    {
        if (menuIndex == 0)
        {
            unlockMenu.SetActive(true);
            upgradeMenu.SetActive(false);

            upgradeButton.interactable = true;
            unlockButton.interactable = false;
        }
        else if (menuIndex == 1)
        {
            unlockMenu.SetActive(false);
            upgradeMenu.SetActive(true);

            upgradeButton.interactable = false;
            unlockButton.interactable = true;
        }
    }



}
