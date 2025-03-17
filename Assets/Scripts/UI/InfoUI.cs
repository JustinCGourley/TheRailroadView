using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class InfoUI : MonoBehaviour
{

    [SerializeField] Transform contentTransform;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject messageObj;

    List<GameObject> messageList = new List<GameObject>();

    public void AddMessage(string message)
    {
        GameObject obj = Instantiate(messageObj, contentTransform);

        obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = message;

        messageList.Add(obj);

        //remove from the top if we hit a threshold
        if (messageList.Count > Constants.UI_INFO_MAX_MESSAGES)
        {
            Destroy(messageList[0]);
            messageList.RemoveAt(0);
        }

        //update canvas changes, then set scroll bar to bottom of view
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }

}
