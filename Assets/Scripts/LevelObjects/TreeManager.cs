using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeManager : MonoBehaviour
{
    public List<GameObject> trees;
    public int treeGroupSize;
    public Transform treeParent;

    public Material treeMat;

    List<GameObject> treeGroups;
    List<List<TreeData>> treeData;

    private struct TreeData
    {
        public string treeType;
        public Vector2Int tilePos;
        public Quaternion rotation;

        public TreeData(string treeType, Vector2Int pos, Quaternion rot)
        {
            this.treeType = treeType;
            this.tilePos = pos;
            this.rotation = rot;
        }
    }

    public static TreeManager Instance { get; private set; }
    
    /// <summary>
    /// sets up singleton class
    /// </summary>
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

    public void InitialSetup(List<GameObject> generatedTrees)
    {
        treeData = new List<List<TreeData>>();
        treeGroups = new List<GameObject>();
        

        List<GameObject> treeGrouping = new List<GameObject>();

        GameObject treeGroup = GenerateCombineObject($"TreeGroup{treeGroups.Count + 1}");
        treeData.Add(new List<TreeData>());

        for (int i = 0; i < generatedTrees.Count; i++)
        {
            generatedTrees[i].transform.parent = treeGroup.transform;
            treeData[treeGroups.Count].Add(new TreeData(generatedTrees[i].name, Utility.Instance.GetTileCoordsFromPosition(generatedTrees[i].transform.position), generatedTrees[i].transform.rotation));
            treeGrouping.Add(generatedTrees[i]);

            if (treeGrouping.Count == treeGroupSize || i == generatedTrees.Count-1)
            {
                Utility.Instance.CombineMesh(treeGrouping, treeGroup.transform);
                treeGroup.GetComponent<MeshRenderer>().material = treeMat;
                treeGroups.Add(treeGroup);
                treeGroup = GenerateCombineObject($"TreeGroup{treeGroups.Count + 1}");
                treeGrouping = new List<GameObject>();
                treeData.Add(new List<TreeData>());
            }
        }

    }

    public GameObject SeperateTree(Vector2Int pos)
    {
        GameObject tree = null;
        for (int i = 0; i < treeData.Count; i++)
        {
            for (int j = 0; j < treeData[i].Count; j++)
            {
                if (treeData[i][j].tilePos == pos)
                {
                    tree = GenerateTree(treeData[i][j], treeParent, true);
                    treeData[i].RemoveAt(j);
                    

                    StartCoroutine(RebuildCollider(i));
                }
            }
        }

        return tree;
    }

    public GameObject SeperateTree(List<Vector2Int> pos)
    {
        List<int> collidersToUpdate = new List<int>();
        GameObject tree = null;
        for (int i = 0; i < treeData.Count; i++)
        {
            for (int j = 0; j < treeData[i].Count; j++)
            {
                if (Utility.Instance.ListContainsItem(pos, treeData[i][j].tilePos))
                {
                    tree = GenerateTree(treeData[i][j], treeParent, true);
                    treeData[i].RemoveAt(j);
                    if (!collidersToUpdate.Contains(i))
                        collidersToUpdate.Add(i);
                }
            }
        }

        foreach(int col in collidersToUpdate)
        {
            Debug.Log("Rebuilding collider for " + col);
            RebuildCollider(col);
        }

        return tree;
    }

    private GameObject GetTree(string treeName)
    {
        switch (treeName)
        {
            case "TreesV1(Clone)":
                return trees[0];
            case "TreesV2(Clone)":
                return trees[1];
            case "TreesV3(Clone)":
                return trees[2];
            case "TreesV4(Clone)":
                return trees[3];
        }

        return null;
    }

    private GameObject GenerateTree(TreeData data, Transform parent, bool giveScript = false)
    {
        GameObject obj =  Instantiate
        (
            GetTree(data.treeType),
            new Vector3(
                data.tilePos.x + ((Constants.LEVEL_TILE_WIDTH + 0f) / 2), 
                LevelManager.Instance.GetHeightAtPos(data.tilePos), 
                data.tilePos.y + ((Constants.LEVEL_TILE_HEIGHT + 0f) / 2)),
            data.rotation,
            parent
        );

        if (giveScript)
        {
            obj.AddComponent<TreeAdjuster>();
        }

        return obj;
    }

    private IEnumerator RebuildCollider(int index)
    {
        yield return new WaitForSeconds(2);

        List<GameObject> objs = new List<GameObject>();
        foreach (TreeData data in treeData[index])
        {
            objs.Add(GenerateTree(data, treeGroups[index].transform));
        }

        Utility.Instance.CombineMesh(objs, treeGroups[index].transform);

        yield return null;
    }

    private GameObject GenerateCombineObject(string name)
    {
        GameObject obj = new GameObject(name);

        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshFilter>();

        obj.transform.parent = treeParent;

        return obj;
    }
}
