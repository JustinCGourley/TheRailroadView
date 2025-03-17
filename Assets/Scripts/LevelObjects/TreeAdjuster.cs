using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAdjuster : MonoBehaviour
{
    private bool isActive;
    public bool IsActive
    {
        get { return isActive; }
    }

    private GameObject selectionCube;

    // Start is called before the first frame update
    void Start()
    {
        //selectionCube = Instantiate(Resources.Load<GameObject>(Constants.RESOURCES_PREFAB_SELECTION_CUBE), this.transform.position, Quaternion.identity);
    }
    
    public void CutTree(int childIndex = 0)
    {
        this.transform.GetChild(childIndex).gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Destroy(selectionCube);
    }

}
