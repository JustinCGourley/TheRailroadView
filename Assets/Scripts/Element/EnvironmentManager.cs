using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }

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

    public GameObject firePool;
    public GameObject icePool;
    public GameObject mudPool;
    public GameObject lavaPool;
    public GameObject waterPool;
    public GameObject steamPool;

    List<EnvironmentHazard> envHazards;

    private void Start()
    {
        envHazards = new List<EnvironmentHazard>();
    }

    private void FixedUpdate()
    {
        foreach (EnvironmentHazard envHazard in envHazards)
        {
            envHazard.UpdateHazard();
        }
    }

    public void DestroyAllHazards()
    {
        for (int i = 0; i < envHazards.Count; i++)
        {
            try
            {
                Destroy(envHazards[i].gameObject);
            }
            catch
            {
                Debug.LogWarning("Attmpted to destroy environment hazard, but was unable to do so");
            }
        }
    }

    public void SpawnPool(Vector3 pos, Element element)
    {
        EnvironmentHazard newPool;
        switch (element)
        {
            case Element.fire:
                newPool = Instantiate(firePool, pos, Quaternion.identity, this.transform).GetComponent<EnvironmentHazard>();
                break;
            case Element.ice:
                newPool = Instantiate(icePool, pos, Quaternion.identity, this.transform).GetComponent<EnvironmentHazard>();
                break;
            case Element.water:
                newPool = Instantiate(waterPool, pos, Quaternion.identity, this.transform).GetComponent<EnvironmentHazard>();
                break;
            case Element.mud:
                newPool = Instantiate(mudPool, pos, Quaternion.identity, this.transform).GetComponent<EnvironmentHazard>();
                break;
            case Element.lava:
                newPool = Instantiate(lavaPool, pos, Quaternion.identity, this.transform).GetComponent<EnvironmentHazard>();
                break;
            case Element.steam:
                newPool = Instantiate(steamPool, pos, Quaternion.identity, this.transform).GetComponent<EnvironmentHazard>();
                break;
            default:
                Debug.LogWarning($"Element type does not have an availble pool to spawn  | for element {element}");
                return;
        }

        envHazards.Add( newPool );
    }
}
