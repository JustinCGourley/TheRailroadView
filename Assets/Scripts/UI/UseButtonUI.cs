using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UseButtonUI : MonoBehaviour
{
    [HideInInspector] public List<Building_ControlledTower> objs;
    [HideInInspector] public Button button;
    [HideInInspector] public ObjectInfo objInfo;

    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI counterText;
    [SerializeField] GameObject counterObj;
    [SerializeField] RectTransform fillTransform;
    [SerializeField] GameObject disabledObj;
    [SerializeField] GameObject selectedObj;

    public void SetupButton(Building_ControlledTower firstTower)
    {
        objs = new List<Building_ControlledTower>();
        this.button = this.GetComponent<Button>();
        objs.Add(firstTower);
        objInfo = ProgressionManager.Instance.GetObjectInfoFromBuilding(firstTower);
        counterText.text = objInfo.buildingName;
    }

    public void SetCounterText(int active)
    {
        counterText.text = $"{active}";
        counterObj.SetActive(active > 0);
    }

    public void SetFillHeight(float height)
    {
        Vector2 pos = new Vector2(0, height);
        fillTransform.localPosition = pos;
    }

    public void SetSelected(bool selected = false)
    {
        selectedObj.SetActive(selected);
        button.interactable = !selected;
    }
}
