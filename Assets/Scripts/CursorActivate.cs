using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// THIS IS A TEMP SCRIPT
/// This is purely to show and hide the temp cursor
/// once an actual cursor is made this should be redone within [PlacementController]
/// </summary>
public class CursorActivate : MonoBehaviour
{

    public List<GameObject> objs;


    public void SetActive(bool active)
    {
        foreach (GameObject obj in objs)
        {
            obj.SetActive(active);
        }
    }

}
