using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UseTowerUI : MonoBehaviour
{

    [SerializeField] GameObject uiButtonObj;
    [SerializeField] Transform useTowerUITransform;

    [SerializeField] float spacing;

    List<UseButtonUI> buttons = new List<UseButtonUI>();

    public void CreateTowerUIButtons(List<Building_ControlledTower> controlled)
    {
        ClearButtons();
        for (int i = 0; i < controlled.Count; i++)
        {
            Building_ControlledTower building = controlled[i];

            AddBuildingButton(building);
        }
    }

    private void AddBuildingButton(Building_ControlledTower building)
    {
        ObjectInfo objInfo = ProgressionManager.Instance.GetObjectInfoFromBuilding(building);
        foreach (UseButtonUI uB in buttons)
        {
            if (objInfo == uB.objInfo)
            {
                uB.objs.Add(building);
                return;
            }
        }

        //no buttons found so we should create a new one
        GameObject newButton = Instantiate(uiButtonObj, useTowerUITransform);

        UseButtonUI useButton = newButton.GetComponent<UseButtonUI>();
        useButton.SetupButton(building);

        newButton.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.SelectControlledTower(useButton));

        buttons.Add(useButton);
    }


    public void UpdateButtons(UseButtonUI curSelectedButton)
    {
        foreach (UseButtonUI uB in buttons)
        {
            uB.SetSelected(false);
            int active = 0;
            Building_ControlledTower closestToFinishing = null;
            foreach (Building_ControlledTower tower in uB.objs)
            {
                if (tower.timeLeftUntilFire() == 0)
                {
                    active++;
                }
                else if (closestToFinishing == null || tower.timeLeftUntilFire() < closestToFinishing.timeLeftUntilFire())
                {
                    closestToFinishing = tower;
                }
            }

            uB.SetCounterText(active);

            if (closestToFinishing != null)
            {
                float posY = Mathf.Lerp(0, 100, closestToFinishing.percentLeftUntilFire());
                uB.SetFillHeight(posY);
            }
        }

        if (curSelectedButton != null)
        {
            curSelectedButton.SetSelected(true);
        }
    }

    public void ClearButtons()
    {
        foreach (UseButtonUI button in buttons)
        {
            Destroy(button.button.gameObject);
        }
        buttons.Clear();
    }
}
