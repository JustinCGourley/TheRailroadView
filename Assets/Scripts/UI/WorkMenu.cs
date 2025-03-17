using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkMenu : MonoBehaviour
{

    [SerializeField] GameObject dragButton;
    [SerializeField] GameObject workMenu;

    [SerializeField] Text popText;
    [SerializeField] Text woodPopText;
    [SerializeField] Text stonePopText;
    [SerializeField] Text workPopText;
    [SerializeField] Text researchPopText;

    [SerializeField] Slider woodSlider;
    [SerializeField] Slider stoneSlider;
    [SerializeField] Slider researchSlider;

    [SerializeField] Slider totalPopSlider;
    
    [SerializeField] GameObject workingMenu;
    [SerializeField] GameObject trainingMenu;
    [SerializeField] Slider usedPopSliderWork;
    [SerializeField] Slider usedPopSliderTrain;
    [SerializeField] GameObject trainingSlider;
    [SerializeField] Transform trainingContent;

    [SerializeField] Text totalTrainingText;
    [SerializeField] Text totalWorkingText;

    [SerializeField] GameObject overprovisionedText;

    List<Slider> trainingSliders = new List<Slider>();
    List<int> trainingStartValue = new List<int>();

    //BOTH WORKSLIDERS AND TEXT NEED TO FOLLOW FORMAT:
    // 0 - farm
    [SerializeField] List<Slider> workSliders = new List<Slider>();
    [SerializeField] List<Text> workSliderText = new List<Text>();

    int totalPop;
    int totalGatherSpots;
    public int GetTotalGatherSpots {  get { return totalGatherSpots; } }
    int totalResearchJobs;
    string curMenu;

    public void UpdateMenu()
    {
        totalPop = JobManager.Instance.GetPopulationCount();
        popText.text = $"Total Population: 0 / {totalPop}";
        totalPopSlider.maxValue = totalPop;


        trainingMenu.SetActive(false);
        workingMenu.SetActive(false);

        totalGatherSpots = 0;
        totalResearchJobs = 0;
        List<Building> buildings = GameManager.Instance.GetAllBuildings();
        foreach (Building building in buildings)
        {
            if (building.towerType == TowerType.gather)
            {
                totalGatherSpots += building.maxJobs;
            }
            if (building.towerType == TowerType.research)
            {
                totalResearchJobs += building.maxJobs;
            }
        }

        totalGatherSpots += Constants.NEXUS_GATHER_JOBS;

        researchSlider.maxValue = totalResearchJobs;
        researchPopText.text = $"0 / {totalResearchJobs}";

        woodSlider.maxValue = totalGatherSpots;
        stoneSlider.maxValue = totalGatherSpots;

        SetupUnitSliders();
        SetupWorkSliders();
        UpdateText();

    }

    public void CloseMenu()
    {
        workMenu.SetActive(false);
        curMenu = null;
    }

    public void OpenMenu(string menu)
    {
        if (menu == curMenu && !(menu == "work" && workingMenu.activeSelf) && !(menu == "training" && trainingMenu.activeSelf))
        {
            trainingMenu.SetActive(false);
            workingMenu.SetActive(false);
            curMenu = "none";
        }
        else
        {
            if (menu == "work")
            {
                workingMenu.SetActive(true);
                trainingMenu.SetActive(false);
            }
            else
            {
                workingMenu.SetActive(false);
                trainingMenu.SetActive(true);
            }
            curMenu = menu;
        }
    }

    public void UpdateSliders(string lastSlide)
    {
        int numAllocated = (int)(stoneSlider.value + woodSlider.value);

        if (numAllocated > totalGatherSpots)
        {
            int remainder = numAllocated - totalGatherSpots;
            if (lastSlide == Constants.RESOURCE_WOOD)
            {
                CorrectSliderValues(new List<Slider> { stoneSlider}, remainder);
            }
            else if (lastSlide == Constants.RESOURCE_STONE)
            {
                CorrectSliderValues(new List<Slider> { woodSlider }, remainder);
            }
        }

        UpdateText();

        UpdatePopText();
    }

    private void UpdateText()
    {
        woodPopText.text = $"{woodSlider.value} / {totalGatherSpots}";
        stonePopText.text = $"{stoneSlider.value} / {totalGatherSpots}";
        researchPopText.text = $"{researchSlider.value} / {totalResearchJobs}";
    }

    private void UpdatePopText()
    {
        int totalAllocated = GetNumAllocated();
        popText.text = $"Total Population: {totalAllocated} / {totalPop}";
        totalPopSlider.value = totalAllocated;

        if (totalAllocated > totalPop)
        {
            overprovisionedText.SetActive(true);
        }
        else
        {
            overprovisionedText.SetActive(false);
        }
    }

    private void CorrectSliderValues(List<Slider> sliders, int remainder)
    {
        int divided = remainder / sliders.Count;
        int leftOver = remainder % sliders.Count;

        for (int i = 0; i < sliders.Count; i++)
        {
            sliders[i].value -= divided;
        }

        for (int i = 0; i < leftOver; i++)
        {
            Slider largest = sliders[0];
            for (int j = 1; j < sliders.Count; j++)
            {
                if (sliders[j].value > largest.value)
                {
                    largest = sliders[j];
                }
            }
            largest.value--;
        }
    }

    public int GetSliderValue(string sliderType)
    {
        switch (sliderType)
        {
            case Constants.RESOURCE_WOOD:
                return (int)woodSlider.value;
            case Constants.RESOURCE_STONE:
                return (int)stoneSlider.value;
            case Constants.RESOURCE_RESEARCH:
                return (int)researchSlider.value;
            case Constants.RESOURCE_FARM:
                return (int)workSliders[0].value;
        }
        return -1;
    }



    public void SetupUnitSliders()
    {
        List<Building_UnitTower> unitTowers = GameManager.Instance.GetUnitBuildings();

        for (int i = 0; i < trainingSliders.Count; i++)
        {
            Destroy(trainingSliders[i].transform.parent.gameObject);
        }

        trainingSliders.Clear();
        trainingStartValue.Clear();

        foreach (Building_UnitTower unitTower in unitTowers)
        {
            GameObject obj = Instantiate(trainingSlider, trainingContent).transform.GetChild(0).gameObject;

            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{GetTowerName(unitTower.towerUnitType)}";
            obj.transform.GetChild(1).GetComponent<Text>().text = $"{unitTower.currentUnits} / {unitTower.maxUnits}";
            Slider slider = obj.GetComponent<Slider>();
            slider.maxValue = unitTower.maxUnits;
            slider.value = unitTower.currentUnits;
            slider.onValueChanged.AddListener((e) => { SliderChangeUnit(slider, unitTower); });

            trainingSliders.Add(slider);
            trainingStartValue.Add((int)slider.value);
            Debug.Log($"Adding slider with initial value: {slider.value}");
        }
    }

    private void SliderChangeUnit(Slider slider, Building_UnitTower tower)
    {
        if (slider.value < tower.currentUnits)
        {
            slider.value = tower.currentUnits;
        }

        slider.transform.GetChild(1).GetComponent<Text>().text = $"{slider.value} / {tower.maxUnits}";


        int numAllocated = GetNumAllocated();

        if (numAllocated > totalPop)
        {
            int remainder = numAllocated - totalPop;
            //CorrectSliderValues(new List<Slider> { woodSlider, stoneSlider, researchSlider }, remainder);
        }


        UpdateUsedPop(usedPopSliderTrain);

        UpdatePopText();
    }

    public void SetupWorkSliders()
    {
        List<Building> buildings = GameManager.Instance.GetAllBuildings();


        int farmJobs = 0;

        foreach (Building building in buildings)
        {
            switch (building.towerType)
            {
                case TowerType.farm:
                    farmJobs += building.maxJobs;
                    break;
            }

        }


        workSliders[0].maxValue = farmJobs;
        workSliderText[0].text = $"0 / {workSliders[0].maxValue}";
        SliderChangeWork(0);
    }

    public void SliderChangeWork(int index)
    {
        UpdateUsedPop(usedPopSliderWork);
        UpdatePopText();
    }

    private void UpdateUsedPop(Slider usedPop)
    {
        int value = 0;
        int max = 0;
        if (usedPop == usedPopSliderTrain)
        {
            foreach (Slider slider in trainingSliders)
            {
                value += (int)slider.value;
                max += (int)slider.maxValue;
            }

            totalTrainingText.text = $"{value} / {max}";
        }
        else if (usedPop == usedPopSliderWork)
        {
            for (int i = 0; i < workSliders.Count; i++)
            {
                value += (int)workSliders[i].value;
                max += (int)workSliders[i].maxValue;
                workSliderText[i].text = $"{(int)workSliders[i].value} / {(int)workSliders[i].maxValue}";
            }

            totalWorkingText.text = $"{value} / {max}";
        }

        usedPop.maxValue = max;
        usedPop.value = value;
    }

    private int GetNumAllocated()
    {
        int count = (int)(woodSlider.value + stoneSlider.value + researchSlider.value);
        
        for (int i = 0; i < trainingSliders.Count; i++)
        {
            count += (int)trainingSliders[i].value - trainingStartValue[i];
        }

        for (int i = 0; i < workSliders.Count; i++)
        {
            count += (int)workSliders[i].value;
        }

        return count;
    }

    private string GetTowerName(AIUnitType type)
    {
        switch (type)
        {
            case AIUnitType.basic:
                return "Guard Tower";
            case AIUnitType.swordsman:
                return "Swordsman";
            case AIUnitType.archer:
                return "Archer Tower";
            case AIUnitType.ogre:
                return "Ogre Tower";
            case AIUnitType.mage:
                return "Mage Tower";
            default:
                return "Idk?";
        }
    }

    public List<int> GetTrainingSliderValues()
    {
        List<int> sliderValues = new List<int>();
        for (int i = 0; i < trainingSliders.Count; i++)
        {
            sliderValues.Add((int)trainingSliders[i].value - trainingStartValue[i]);
        }

        return sliderValues;
    }

    public bool IsOverProvisioned()
    {
        return GetNumAllocated() > totalPop;
    }
}
