using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{

    //how much real time passes before a minute in the game passes
    [SerializeField] private float timeToPassPerHour;
    [SerializeField] private Text timeText;
    [SerializeField] private GameObject theSun;
    [SerializeField] private GameObject dayRadial;

    [SerializeField] private int nightStartHour;
    [SerializeField] private int newDayStartHour;

    private float curTime;

    private int hour;

    // Start is called before the first frame update
    void Start()
    {
        curTime = Time.time;
        hour = newDayStartHour;
    }

    private void Update()
    {
        if (Time.time - curTime >= timeToPassPerHour)
        {
            curTime = Time.time;
            hour++;

            if (hour >= 23)
            {
                hour = 0;
            }

            if (hour == nightStartHour)
            {
                GameManager.Instance.EndDay();
            }
            else if (hour == newDayStartHour)
            {
                GameManager.Instance.StartNewDay();
            }
            UpdateTimeTextAndSun();
        }
    }

    private void UpdateTimeTextAndSun()
    {
        timeText.text = $"{hour}";

        //float currentDecimalTime = hour;
        //float rotateAmount = 360f * (currentDecimalTime / 24f);
        //theSun.transform.eulerAngles = new Vector3(rotateAmount, 0, 0);

    }

    private void AddTime(int minutes)
    {
        hour += minutes / 60;
    }

    public int GetCurrentTime()
    {
        return hour;
    }
}
