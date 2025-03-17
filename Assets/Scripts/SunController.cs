using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SunController : MonoBehaviour
{
    //There needs to be a directional light in the scene named 'THE SUN' for this to work :)
    GameObject sun = null;
    [SerializeField] Image timeRadial;
    float animationTimer = 0f;
    float rotationAngle;
    float newAngle;
    float maxTime = 0f;

    float radialTimeToWait;
    float startRadialTime;

    // Start is called before the first frame update
    void Start()
    {
        sun = GameObject.Find(Constants.GAMEOBJECT_SUN);
        newAngle = sun.transform.eulerAngles.x;

        timeRadial.fillAmount = 0;
    }

    public void MoveSunToRotation(float newRotation, float time)
    {
        if (sun == null) { return; }
        animationTimer = Time.time;
        rotationAngle = newAngle;
        newAngle = newRotation;
        maxTime = Time.time + time;

        if (time == 0)
        {
            sun.transform.eulerAngles = new Vector3(newAngle, 330, 0);
            maxTime = 0;
        }
        Debug.Log($"SUNCONTROLLER DEBUG - trying to go to a new place! - newRot: {newRotation} | time: {time}");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if ((Time.time - animationTimer) / maxTime <= 1 && maxTime != 0)
        {
            Vector3 newRotation;
            newRotation = new Vector3(Mathf.Lerp(rotationAngle, newAngle, (Time.time - animationTimer) / maxTime), 330, 0);

            sun.transform.eulerAngles = newRotation;
        }

        if (Time.time - startRadialTime <= radialTimeToWait)
        {
            float r = 1 - ((Time.time - startRadialTime) / radialTimeToWait);
            timeRadial.fillAmount = r;
        }
    }

    public void StartRadial(float time)
    {
        radialTimeToWait = time;
        startRadialTime = Time.time;
    }

    public void SetRadial(float amount)
    {
        timeRadial.fillAmount = amount;
    }
}
