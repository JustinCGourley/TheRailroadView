using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OrbManager : MonoBehaviour
{
    public static OrbManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    [SerializeField] 
    GameObject orbPrefab;
    List<GameObject> orbs;


    // Start is called before the first frame update
    void Start()
    {
        orbs = new List<GameObject>();
    }

    public void CreateResearchOrbs(int numToSpawn)
    {
        if (orbs.Count > 0)
        {
            RemoveOrbs();
        }

        for (int i = 0; i < numToSpawn; i++)
        {
            GameObject orb = GameObject.Instantiate(orbPrefab);

            orb.transform.position = GetRandomPointNearNexus(10f, 30f);
            orb.SetActive(false);
            orbs.Add(orb);
        }
    }

    public void RemoveOrbs()
    {
        for (int i = 0; i < orbs.Count; i++)
        {
            if (orbs[i] != null)
            {
                Destroy(orbs[i]);
            }
        }

        orbs = new List<GameObject>();
    }

    public void RemoveOrb(GameObject orb)
    {
        orbs.Remove(orb);
        Destroy(orb);
    }

    public List<GameObject> RevealOrbFromPosition(Vector3 pos, float radius)
    {
        List<GameObject> revealedOrbs = new List<GameObject>();
        foreach (GameObject orb in orbs)
        {
            if ((pos - orb.transform.position).magnitude <= radius && !orb.activeSelf)
            {
                orb.SetActive(true);
                revealedOrbs.Add(orb);
            }
        }

        return revealedOrbs;
    }

    public Vector3 GetClosestOrbFromPosition(Vector3 pos)
    {
        Vector3 closest = Vector3.zero;
        float closestDistance = -1;
        foreach (GameObject orb in orbs)
        {
            if ((pos - orb.transform.position).magnitude <= closestDistance || closestDistance == -1)
            {
                closestDistance = (pos - orb.transform.position).magnitude;
                closest = orb.transform.position;
            }
        }

        return closest;
    }

    //https://www.loekvandenouweland.com/content/calculate-random-point-on-circle-edge-in-unity3d.html
    private Vector3 GetRandomPointNearNexus(float minRadius, float maxRadius)
    {
        bool done = false;
        while(!done)
        {
            var vector2 = Random.insideUnitCircle.normalized * (Random.Range(minRadius, maxRadius));
            Vector3 pos = GameManager.Instance.GetNexusPosition() + new Vector3(vector2.x, 0, vector2.y);
            if (pos.x >= 0 && pos.x < Constants.LEVEL_SIZE && pos.z >= 0 && pos.z < Constants.LEVEL_SIZE)
            {
                done = true;
                return Utility.Instance.GetCenterOfTile(pos);
            }
        }

        return Vector3.zero; //we should never reach this
    }
}
