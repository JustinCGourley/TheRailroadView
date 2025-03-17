using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//singleton class
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    LevelData[,] levelMap;
    int[,] claimedMap;
    float planeWidth, planeHeight;
    public float PlaneWidth { get { return planeWidth; } }
    public float PlaneHeight { get { return planeHeight; } }

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

    // Start is called before the first frame update
    void Start()
    {
        claimedMap = new int[Constants.LEVEL_SIZE, Constants.LEVEL_SIZE];
    }

    public int GetLevelSize()
    {
        return levelMap.GetLength(0);
    }

    public void SetLevel(LevelData[,] levelMap, float planeWidth, float planeHeight)
    {
        this.levelMap = levelMap;
        this.planeWidth = planeWidth;
        this.planeHeight = planeHeight;

        PathFinding.Instance.SetupPathNodeGrid();
    }

    public LevelData GetTileData(Vector2Int pos)
    {
        return levelMap[pos.x, pos.y];
    }

    public LevelData GetTileData(int x, int y)
    {
        return levelMap[x, y];
    }

    public void UpdateLevelWithBuilding(Vector2Int pos, GameObject obj, ObjectType objType)
    {
        levelMap[pos.x, pos.y].obj = obj;
        levelMap[pos.x, pos.y].objType = objType;

        PathFinding.Instance.UpdatePathNodeGrid(pos.x, pos.y, levelMap[pos.x, pos.y]);
    }

    public float GetHeightAtPos(Vector2Int pos)
    {
        return levelMap[pos.x, pos.y].height;
    }

    public bool TileIsOccupied(Vector2Int pos)
    {
        return (levelMap[pos.x, pos.y].objType != ObjectType.none);
    }

    public bool TileIsOccupiedBuilding(Vector2Int pos)
    {
        return (levelMap[pos.x, pos.y].objType == ObjectType.building || levelMap[pos.x, pos.y].objType == ObjectType.nexus || levelMap[pos.x, pos.y].objType == ObjectType.pipe);
    }

    public void DestroyObjectOnTile(Vector2Int pos)
    {
        Destroy(levelMap[pos.x, pos.y].obj);
        levelMap[pos.x, pos.y].obj = null;
        levelMap[pos.x, pos.y].objType = ObjectType.none;

        PathFinding.Instance.UpdatePathNodeGrid(pos.x, pos.y, levelMap[pos.x, pos.y]);
    }

    public void RemoveBuildingOnTile(Vector2Int pos)
    {
        levelMap[pos.x, pos.y].obj = null;
        levelMap[pos.x, pos.y].objType = ObjectType.none;

        PathFinding.Instance.UpdatePathNodeGrid(pos.x, pos.y, levelMap[pos.x, pos.y]);
    }


    public void UpdateLevelData(LevelData data, Vector2Int position)
    {
        levelMap[position.x, position.y] = data;
    }

    /// <summary>
    /// Returns the closest data on tiles that are closest to a given position, and match the searching for type
    /// So far this is used to find the nearest tree from a given position (always the nexus)
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="objType"></param>
    /// <returns></returns>
    public LevelData GetClosestObjectFromPosition(Vector2Int pos, ObjectType objType)
    {
        bool finished = false;
        int searchRadius = 1;
        int maxLoops = 100;

        while (!finished && maxLoops != 0)
        {
            maxLoops--;
            int minX = pos.x - searchRadius, maxX = pos.x + searchRadius;
            if (minX < 0)
                minX = 0;
            if (maxX >= Constants.LEVEL_SIZE)
                maxX = Constants.LEVEL_SIZE-1;
            int minY = pos.y - searchRadius, maxY = pos.y + searchRadius;
            if (minY < 0)
                minY = 0;
            if (maxY >= Constants.LEVEL_SIZE)
                maxY = Constants.LEVEL_SIZE-1;
            List<Vector2Int> possibleClosestObjs = new List<Vector2Int>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (!((x == minX || x == maxX) || (y == minY || y == maxY))) continue;

                    if (levelMap[x,y].objType == objType && claimedMap[x,y] == 0)
                    {
                        possibleClosestObjs.Add(levelMap[x, y].position);
                    }
                }
            }
            if (possibleClosestObjs.Count == 0)
            {
                searchRadius += 1;
            }
            else
            {
                Vector2Int closest = Vector2Int.zero;
                float closestValue = 0;
                foreach (Vector2Int closeObj in possibleClosestObjs)
                {
                    if (closest == Vector2Int.zero)
                    {
                        closest = closeObj;
                        closestValue = (pos - closest).magnitude;
                    }
                    else
                    {
                        if ((pos - closeObj).magnitude < closestValue)
                        {
                            closestValue = (pos - closeObj).magnitude;
                            closest = closeObj;
                        }
                    }
                }
                finished = true;
                return levelMap[closest.x, closest.y];
            }
        }

        Debug.LogError("I dont think we were supposed to be able to reach here? LevelManager GetCLosestObjectFromPosition");
        return levelMap[0,0];
    }

    public void ClaimLocation(Vector2Int pos)
    {
        claimedMap[pos.x, pos.y] = 1;
    }

    public void UnclaimLocation(Vector2Int pos)
    {
        claimedMap[pos.x, pos.y] = 0;
    }
}
