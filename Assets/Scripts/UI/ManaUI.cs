using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManaUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI manaText;
    [SerializeField] Image manaImage;

    Vector2 startPos;

    private void Start()
    {
        startPos = manaText.transform.localPosition;
    }

    public void UpdateManaAmount(int amount, int max)
    {
        manaText.text = $"{amount}\n―――\n{max}";
        float percentDone = 1f - (float)amount / max;


        float size = manaImage.rectTransform.sizeDelta.y;
        Vector2 newPos = startPos + new Vector2(0, -size * percentDone);
        manaImage.transform.localPosition = newPos;
    }
}
