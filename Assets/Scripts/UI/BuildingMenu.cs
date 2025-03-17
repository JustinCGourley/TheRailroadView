using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingMenuButton
{
    public Button button;
    public TowerType towerType;
    public BuildingMenuButton(Button button, TowerType towerType)
    {
        this.button = button;
        this.towerType = towerType;
    }
}

public class BuildingMenu : MonoBehaviour
{
    [SerializeField] GameObject uiObj;
    [SerializeField] Transform buildingMenuTransform;
    [SerializeField] Transform buildingPipeMenuTransform;
    [SerializeField] float uiSpacing;

    [SerializeField] List<Button> filterButtons;
    [SerializeField] List<TowerType> filterTypes; // this should be the same size as filterButtons
    Button lastSelectedButton = null;

    List<BuildingMenuButton> buttons = new List<BuildingMenuButton>();

    private void Start()
    {
        for (int i = 0; i < filterButtons.Count; i++)
        {
            int index = i;
            filterButtons[i].onClick.AddListener(() => { FilterBuildings(filterButtons[index], filterTypes[index]); });
        }

        filterButtons[0].interactable = false;
        lastSelectedButton = filterButtons[0];
    }

    private void ClearButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Destroy(buttons[i].button.gameObject);
        }
        buttons.Clear();
    }


    public void SetupMenu()
    {
        ClearButtons();
        List<ObjectInfo> curBuildings = ProgressionManager.Instance.GetUnlockedBuildings();

        foreach (ObjectInfo building in curBuildings)
        {
            GameObject buildingUI = Instantiate(uiObj, buildingMenuTransform);

            buildingUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{building.buildingName}";
            buildingUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Wood: ${building.woodCost}";
            buildingUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"Stone: ${building.stoneCost}";
            buildingUI.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"Mana: ${building.manaCost}";
            buildingUI.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = $"Workers: ${building.workerCost}";

            buildingUI.GetComponent<Button>().onClick.AddListener(() => { GameManager.Instance.BuildingButtonPressed(building); });

            buttons.Add(new BuildingMenuButton(buildingUI.GetComponent<Button>(), building.obj.GetComponent<Building>().towerType));
        }
    }

    public void FilterBuildings(Button clickedButton, TowerType towerType)
    {
        foreach (BuildingMenuButton button in buttons)
        {
            if (isValidTowerType(button.towerType, towerType))
            {
                button.button.gameObject.SetActive(true);
            }
            else
            {
                button.button.gameObject.SetActive(false);
            }
        }

        if (lastSelectedButton != null)
        {
            lastSelectedButton.interactable = true;
        }
        clickedButton.interactable = false;
        lastSelectedButton = clickedButton;
    }

    private bool isValidTowerType(TowerType type, TowerType filter)
    {
        if (filter == TowerType.attack)
        {
            return type == TowerType.auto || type == TowerType.controlled || type == TowerType.unit;
        }

        if (filter == TowerType.support)
        {
            return type == TowerType.pipe || type == TowerType.pillar;
        }

        if (filter == TowerType.city)
        {
            return type == TowerType.farm || type == TowerType.storage || type == TowerType.gather || type == TowerType.house || type == TowerType.research;
        }

        return type == filter || filter == TowerType.none;
    }

}
