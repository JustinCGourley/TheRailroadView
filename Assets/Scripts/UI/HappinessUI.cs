using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HappinessUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI happinessText;
    [SerializeField] TextMeshProUGUI totalPopText;
    [SerializeField] TextMeshProUGUI soldierPopText;

    public void SetHappinessText(float happiness, int totalPop, int totalSoldiers)
    {
        happinessText.text = $"Happiness  {Mathf.Floor(happiness)}";
        soldierPopText.text = $"Soldiers     {totalSoldiers}";
    }

    public void SetPopText()
    {
        totalPopText.text = $"Total Pop    {GameManager.Instance.GetTotalWorkers()} / {GameManager.Instance.WorkersNeededForJobs()}";
    }
}
