using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathNode
{
    public Vector2Int pos;
    public LevelData levelData;

    public int gCost; //distance to end
    public int hCost; //distance from start
    public int fCost; //g + h
    public int weight;

    public bool isWalkable = true;

    public PathNode cameFromNode;
    public PathNode(Vector2Int pos, LevelData levelData)
    {
        this.pos = pos;
        this.levelData = levelData;
    }
    // noWeight = used for start and end node to not be considered for weight costs
    public void CalculateFCost(bool noWeight = false)
    {
        weight = GetWeight();
        if (noWeight) weight = 0;
        fCost = gCost + hCost + weight;
    }

    public PathNode(int x, int y, LevelData levelData)
    {
        pos.x = x;
        pos.y = y;
        this.levelData = levelData;
    }

    private int GetWeight()
    {
        int w = 0;
        switch (levelData.terrainType)
        {
            case TerrainType.water:
                w = 99999;
                break;
            case TerrainType.sand:
                w = 0;
                break;
            case TerrainType.grass:
                w = 0;
                break;
            case TerrainType.dead_grass:
                w = 0;
                break;
            case TerrainType.rock:
                w = 5;
                break;
        }

        switch (levelData.objType)
        {
            case ObjectType.building:
                w += 99999;
                break;
            case ObjectType.tree:
            case ObjectType.metal:
            case ObjectType.rock:
            case ObjectType.pipe:
                w += 50;
                break;
        }

        return w;
    }

    public override string ToString()
    {
        return $"CurrentNode - pos: {pos} | g: {gCost} | h: {hCost} | f: {fCost}";
    }
}
public class PathFinding : MonoBehaviour
{
    public static PathFinding Instance { get; private set; }
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

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    private List<PathNode> openList;
    private List<PathNode> closedList;
    PathNode[,] pathNodeGrid;


    LevelManager levelManager;
    // Start is called before the first frame update
    void Start()
    {
        levelManager = GameObject.Find(Constants.GAMEOBJECT_LEVELMANAGER).GetComponent<LevelManager>();
    }

    public void SetupPathNodeGrid()
    {
        if (levelManager == null) { levelManager = GameObject.Find(Constants.GAMEOBJECT_LEVELMANAGER).GetComponent<LevelManager>(); }
        pathNodeGrid = new PathNode[Constants.LEVEL_SIZE, Constants.LEVEL_SIZE];

        DateTime timeStart = DateTime.Now;
        Debug.Log("PATHFINDING - RESETTING PATHNODEGRID!!! [This hurts]");

        for (int x = 0; x < Constants.LEVEL_SIZE; x++)
        {
            for (int y = 0; y < Constants.LEVEL_SIZE; y++)
            {
                pathNodeGrid[x, y] = new PathNode(x, y, levelManager.GetTileData(x, y));
            }
        }

        Debug.Log($"Done setting up grid [{(DateTime.Now - timeStart).TotalSeconds}]");
    }

    public void UpdatePathNodeGrid(int x, int y, LevelData tileData)
    {
        pathNodeGrid[x, y].levelData = tileData;
    }

    public List<Vector3> FindPathWithPositions(Vector3 startPos, Vector3 endPos)
    {
        return FindPathWithPositions(
            Utility.Instance.GetTileCoordsFromPosition(startPos),
            Utility.Instance.GetTileCoordsFromPosition(endPos)
        );
    }

    public List<Vector3> FindPathWithPositions(Vector2Int startPos, Vector2Int endPos)
    {
        List<PathNode> pathNodes = FindPath(startPos, endPos);
        List<Vector3> path = new List<Vector3>();
        foreach (PathNode node in pathNodes)
        {
            path.Add(Utility.Instance.GetPositionFromTileCoords(node.pos, true));
        }
        return path;
    }

    // given 2 Vector3 return a list of Vector3 list
    public void FindPathAsync(Vector3 startPos, Vector3 endPos, System.Action<List<Vector3>> callback)
    {
        StartCoroutine(GetPathAsync(startPos, endPos, callback));
    }
    IEnumerator GetPathAsync(Vector3 startPos, Vector3 endPos, System.Action<List<Vector3>> callback)
    {
        List<Vector3> path = FindPathWithPositions(startPos, endPos);

        yield return null;

        callback(path);
    }

    //given 2 Vector3 return a list of PathNode list
    public void FindPathAsync(Vector3 startPos, Vector3 endPos, System.Action<List<PathNode>> callback)
    {
        StartCoroutine(GetPathAsync(startPos, endPos, callback));
    }

    IEnumerator GetPathAsync(Vector3 startPos, Vector3 endPos, System.Action<List<PathNode>> callback)
    {
        List<PathNode> path = FindPath(Utility.Instance.GetTileCoordsFromPosition(startPos), Utility.Instance.GetTileCoordsFromPosition(endPos));

        yield return null;

        callback(path);
    }

    //given 2 Vector2Int return a Vector3 list
    public void FindPathAsync(Vector2Int startPos, Vector2Int endPos, System.Action<List<Vector3>> callback)
    {
        StartCoroutine(GetPathAsync(startPos, endPos, callback));
    }

    IEnumerator GetPathAsync(Vector2Int startPos, Vector2Int endPos, System.Action<List<Vector3>> callback)
    {
        List<Vector3> nodes = FindPathWithPositions(startPos, endPos);
        yield return null;

        callback(nodes);
    }



    public List<PathNode> FindPath(Vector2Int startPos, Vector2Int endPos, bool buildingsNotWalkable = false)
    {
        if (pathNodeGrid == null)
        {
            Debug.LogError("PathNodeGrid was null but it should not have been...\nReseting but this may cause lag");
            SetupPathNodeGrid();
        }

        DateTime startTime = DateTime.Now;

        PathNode startNode = GetNode(startPos);
        PathNode endNode = GetNode(endPos);
        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        for (int x = 0; x < Constants.LEVEL_SIZE; x++)
        {
            for (int y = 0; y < Constants.LEVEL_SIZE; y++)
            {
                PathNode pathNode = GetNode(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;

            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost(true);

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                return CalculatePath(endNode, startTime);
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {

                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.isWalkable || (neighbourNode.levelData.objType == ObjectType.building && buildingsNotWalkable))
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost((neighbourNode.hCost == 0));

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        Debug.LogError($"Unable to find path from {startNode.pos} to {endNode.pos}");
        //out of nodes
        return null;
    }

    void printList(List<PathNode> nodes)
    {
        Debug.Log("Start ->");
        foreach (PathNode node in nodes)
        {
            Debug.Log(node + " ->");
        }
    }

    private PathNode GetNode(Vector2Int pos) { return this.pathNodeGrid[pos.x, pos.y]; }
    private PathNode GetNode(int x, int y) { return pathNodeGrid[x, y]; }

    private List<PathNode> GetNeighbourList(PathNode curNode)
    {
        List<PathNode> neighbours = new List<PathNode>();
        if (curNode.pos.x - 1 >= 0)
        {
            //left
            neighbours.Add(GetNode(curNode.pos.x - 1, curNode.pos.y));
            //left-top
            if (curNode.pos.y + 1 < Constants.LEVEL_SIZE) neighbours.Add(GetNode(curNode.pos.x - 1, curNode.pos.y + 1));
            //left-bottom
            if (curNode.pos.y - 1 >= 0) neighbours.Add(GetNode(curNode.pos.x - 1, curNode.pos.y - 1));
        }
        if (curNode.pos.x + 1 < Constants.LEVEL_SIZE)
        {
            //right
            neighbours.Add(GetNode(curNode.pos.x + 1, curNode.pos.y));
            //right-top
            if (curNode.pos.y + 1 < Constants.LEVEL_SIZE) neighbours.Add(GetNode(curNode.pos.x + 1, curNode.pos.y + 1));
            //right-bottom
            if (curNode.pos.y - 1 >= 0) neighbours.Add(GetNode(curNode.pos.x + 1, curNode.pos.y - 1));
        }
        //bottom
        if (curNode.pos.y - 1 >= 0) neighbours.Add(GetNode(curNode.pos.x, curNode.pos.y - 1));
        //top
        if (curNode.pos.y + 1 < Constants.LEVEL_SIZE) neighbours.Add(GetNode(curNode.pos.x, curNode.pos.y + 1));

        return neighbours;
    }

    private List<PathNode> CalculatePath(PathNode endNode, DateTime startTime)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode curNode = endNode;
        while (curNode.cameFromNode != null)
        {
            path.Add(curNode.cameFromNode);
            curNode = curNode.cameFromNode;
        }
        path.Reverse();

        //Debug.Log($"Finished finding a path in {(DateTime.Now - startTime).TotalSeconds}");
        return path;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodes)
    {
        PathNode lowestFCostNode = pathNodes[0];
        for (int i = 1; i < pathNodes.Count; i++)
        {
            if (pathNodes[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodes[i];
            }
        }
        return lowestFCostNode;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.pos.x - b.pos.x);
        int yDistance = Mathf.Abs(a.pos.y - b.pos.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }
}
