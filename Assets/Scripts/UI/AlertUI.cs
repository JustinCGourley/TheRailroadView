using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertUI : MonoBehaviour
{
    public static AlertUI Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public GameObject needWorkerUIObj;
}
