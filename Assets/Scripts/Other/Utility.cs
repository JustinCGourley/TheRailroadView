using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    public static Utility Instance { get; private set; }

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

    /// <summary>
    /// combine two given lists together
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list1"></param>
    /// <param name="list2"></param>
    /// <returns></returns>
    public List<T> CombineLists<T>(List<T> list1, List<T> list2)
    {
        foreach (T obj in list2)
        {
            list1.Add(obj);
        }
        return list1;
    }

    /// <summary>
    /// Returns the index of a given value in a given list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int FindIndexInList<T>(List<T> list, T obj)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Equals(obj))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns the center of a tile based on a given position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector3 GetCenterOfTile(Vector3 pos)
    {
        return new Vector3(
            (int)(pos.x) + (Constants.LEVEL_TILE_WIDTH / 2f), 
            LevelManager.Instance.GetHeightAtPos(new Vector2Int((int)pos.x, (int)pos.z)), 
            (int)(pos.z) + (Constants.LEVEL_TILE_HEIGHT / 2f));
    }


    /// <summary>
    /// Returns a Vector2Int of tile coordinates based off a given Vector3 position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector2Int GetTileCoordsFromPosition(Vector3 pos)
    {
        int tileWidth = Constants.LEVEL_TILE_WIDTH;
        int tileHeight = Constants.LEVEL_TILE_HEIGHT;

        int posX = Mathf.FloorToInt(pos.x) * tileWidth;
        int posZ = Mathf.FloorToInt(pos.z) * tileHeight;

        return new Vector2Int(posX, posZ);
    }

    /// <summary>
    /// Returns the Vector3 world position given a Vector2Int representing tile coordinates
    /// Will optionally return the middle of a tile instead of the origin if 'returnMiddle == true'
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="returnMiddle"></param>
    /// <returns></returns>
    public Vector3 GetPositionFromTileCoords(Vector2Int pos, bool returnMiddle = false)
    {
        int tileWidth = Constants.LEVEL_TILE_WIDTH;
        int tileHeight = Constants.LEVEL_TILE_HEIGHT;
        Vector3 position = new Vector3(
            tileWidth * pos.x,
            0, //TODO: update this for cliffs etc.
            tileHeight * pos.y);
        
        if (returnMiddle)
        {
            position += new Vector3(Constants.LEVEL_TILE_HEIGHT/2f, 0, Constants.LEVEL_TILE_HEIGHT/2f);
        }

        return position;
    }

    /// <summary>
    /// Returns the current tile coords based of the mouse position
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetTileCoordsFromMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))//ray cast from mouse position out of camera
        {
            return Vector2Int.FloorToInt(Utility.Instance.GetTileCoordsFromPosition(hitInfo.point));
        }
        return Vector2Int.zero;
    }

    /// <summary>
    /// Returns a Vector3 of the position of the location of the tile position based on the mouse position (The Y will always be 0)
    /// </summary>
    /// <returns></returns>
    public Vector3 GetTilePositionFromMousePosition(bool center = true)
    {
        Vector2Int curPos = GetTileCoordsFromMousePosition();
        return new Vector3(
            curPos.x + (center == false ? 0 : ((float)(Constants.LEVEL_TILE_WIDTH) / 2)),
            0,
            curPos.y + (center == false ? 0 : ((float)(Constants.LEVEL_TILE_HEIGHT) / 2))
            );
    }

    public bool IsMouseOverUI()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))//ray cast from mouse position out of camera
        {
            return hitInfo.collider.CompareTag(Constants.TAG_UI);
        }
        return false;
    }

    public Vector3 GetWorldPositionFromMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        //int layerMask = LayerMask.GetMask("Selectable");
        if (Physics.Raycast(ray, out hitInfo))//ray cast from mouse position out of camera
        {
            return hitInfo.point;
        }
        return Vector3.zero;
    }

    public Vector3 GetWorldPositionFromMousePosition(Vector3 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hitInfo;
        //int layerMask = LayerMask.GetMask("Selectable");
        if (Physics.Raycast(ray, out hitInfo))//ray cast from mouse position out of camera
        {
            return hitInfo.point;
        }
        return Vector3.zero;
    }

    public Vector2 GetScreenPositionFromWorldPosition(Vector3 worldPos)
    {
        return Camera.main.WorldToScreenPoint(worldPos);
    }

    /// <summary>
    /// Given a list of GameObjects and a parent objects Transform, will combine all the available meshes together into one mesh
    /// There is a upper limit to the number of verticies and triangles a mesh can have, this method does NOT take that into account
    /// </summary>
    /// <param name="objs"></param>
    /// <param name="parentObj"></param>
    public void CombineMesh(List<GameObject> objs, Transform parentObj)
    {
        MeshFilter[] filters = parentObj.GetComponentsInChildren<MeshFilter>();

        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        CombineInstance[] combiner = new CombineInstance[filters.Length-1];

        for (int i = 1; i < filters.Length; i++)
        {
            combiner[i-1].subMeshIndex = 0;
            combiner[i-1].mesh = filters[i].sharedMesh;
            combiner[i-1].transform = filters[i].transform.localToWorldMatrix;
        }

        finalMesh.CombineMeshes(combiner);
        parentObj.GetComponent<MeshFilter>().sharedMesh = finalMesh;

        for (int i = 0; i < objs.Count; i++)
        {
            Destroy(objs[i]);
        }
    }

    public void CombineAllChildrenMeshes(GameObject obj)
    {
        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            children.Add(obj.transform.GetChild(i).gameObject);
        }

        CombineMesh(children, obj.transform);
    }

    /// <summary>
    /// Writes a given 2D array to console pretty-like
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    public void Write2DArrayToConsole<T>(T[,] array)
    {
        Debug.Log("------------------------------------------");
        for (int i = 0; i < array.GetLength(0); i++)
        {
            string row = "";
            for (int j = 0; j < array.GetLength(1); j++)
            {
                row += $"{array[i, j]} ";
            }
            Debug.Log(row);
        }
        Debug.Log("------------------------------------------");
    }

    public void WriteArrayToConsole<T>(T[] array)
    {
        string stuff = "[";
        foreach (T item in array)
        {
            stuff += $"{item}, ";
        }
        Debug.Log(stuff + "]");
    }

    /// <summary>
    /// Sees if an interger is within a given bounds (inclusive)
    /// </summary>
    public bool IsInBounds(int value, int lowerBound, int upperBound)
    {
        return value >= lowerBound && value <= upperBound;
    }


    public bool ListContainsItem<T>(List<T> list, T obj)
    {
        foreach (T item in list)
        {
            if (item.Equals(obj))
            {
                return true;
            }
        }
        return false;
    }

    public float GetPositionCenter(float v1, float v2)
    {
        return v1 < v2 ? v1 + ((v2 - v1) / 2) : v2 + ((v1 - v2) / 2);
    }

    public float GetSizeFromCenter(float center, float otherPoint)
    {
        return center < otherPoint ? otherPoint - center : center - otherPoint;
    }
}
