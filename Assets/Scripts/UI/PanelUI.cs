using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelUI : MonoBehaviour
{
    public Image picture1;
    public Image picture2;
    public Image picture3;
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;
    public Button button1;
    public Button button2;
    public Button button3;

    private void Start()
    {
        ResearchManager researchManager = GameObject.Find(Constants.GAMEOBJECT_RESEARCHUI).GetComponent<ResearchManager>();
        button1.onClick.AddListener(delegate { researchManager.ClickedPanel(this.gameObject, 0); });
        button2.onClick.AddListener(delegate { researchManager.ClickedPanel(this.gameObject, 1); });
        button3.onClick.AddListener(delegate { researchManager.ClickedPanel(this.gameObject, 2); });
    }
}
