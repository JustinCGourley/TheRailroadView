using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceUI : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI woodResourceCounter;
    [SerializeField] TextMeshProUGUI stoneResourceCounter;
    [SerializeField] TextMeshProUGUI foodResourceCounter;

    public void SetText(int woodCount, int stoneCount, int foodCount)
    {
        woodResourceCounter.text = $"Wood: {woodCount}";
        stoneResourceCounter.text = $"Stone: {stoneCount}";
        foodResourceCounter.text = $"Food: {foodCount}";
    }
}
