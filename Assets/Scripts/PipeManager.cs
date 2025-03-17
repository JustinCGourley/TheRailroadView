using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pipe
{
    public Building building = null;
    public PipeAdjuster adjuster;
    public Element element;
}

// IF YOU HAVE MADE IT HERE THIS IS DEPRECATED... FOR NOW?
public class PipeManager : MonoBehaviour
{
    [SerializeField] ObjectInfo basePipeInfo;
    public ObjectInfo GetBasePipeInfo { get { return basePipeInfo; } }
    // Start is called before the first frame update

    List<List<Pipe>> pipelines = new List<List<Pipe>>();
    Element curElement;
    int curPipelineNum;
    public bool isBuildingPipe = false;

    public void StartPlacePipe(Building building)
    {
        //probably show UI or something idk
        pipelines.Add(new List<Pipe>());
        
        Pipe startBuilding = new Pipe();
        startBuilding.building = building;
        startBuilding.element = building.element;
        curPipelineNum = pipelines.Count - 1;
        
        pipelines[curPipelineNum].Add(startBuilding);

        curElement = building.element;
        isBuildingPipe = true;
    }

    public void StartPlacePipe(int pipelineNum)
    {
        curPipelineNum = pipelineNum;
        curElement = pipelines[curPipelineNum][0].element;
    }

    public void EndPlacePipe()
    {
        curElement = Element.none;
        isBuildingPipe = false;
        curPipelineNum = -1;
    }

    public bool CheckIsConnectingPipe(Vector2Int loc)
    {
        if (LevelManager.Instance.GetTileData(loc).obj == null) return false;

        Building building = LevelManager.Instance.GetTileData(loc).obj.GetComponent<Building>();

        Debug.Log($"Checking if we should connect...\n{ProgressionManager.Instance.GetObjectInfoFromBuilding(building).isPipeable}");

        if (ProgressionManager.Instance.GetObjectInfoFromBuilding(building).isPipeable && !building.isPiped)
        {
            building.isPiped = true;

            Pipe endBuilding = new Pipe();
            endBuilding.building = building;
            endBuilding.element = curElement;

            pipelines[curPipelineNum].Add(endBuilding);

            //update last pipe position
            int pos = GetPipePosition(pipelines[curPipelineNum].Count - 2);
            pipelines[curPipelineNum][pipelines[curPipelineNum].Count - 2].adjuster.UpdatePipe(pos);

            building.AddElement(curElement);

            //GameManager.Instance.StopPipeBuild();
            return true;
        }
        return false;
    }

    public bool CanPlacePipe(Vector2Int loc)
    {
        //only have a building
        if (pipelines[curPipelineNum].Count == 1)
        {
            Vector2Int buildingPos = Utility.Instance.GetTileCoordsFromPosition(pipelines[curPipelineNum][0].building.transform.position);
            if ((Mathf.Abs(buildingPos.x - loc.x) == 1 && Mathf.Abs(buildingPos.y - loc.y) == 0) ||
                (Mathf.Abs(buildingPos.x - loc.x) == 0 && Mathf.Abs(buildingPos.y - loc.y) == 1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        Vector2Int lastPos = Utility.Instance.GetTileCoordsFromPosition(pipelines[curPipelineNum][pipelines[curPipelineNum].Count - 1].adjuster.transform.position);
        Vector2Int lastLastPos;
        Pipe lastLastPipe = pipelines[curPipelineNum][pipelines[curPipelineNum].Count - 1];
        if (lastLastPipe.building != null)
        {
            lastLastPos = Utility.Instance.GetTileCoordsFromPosition(lastLastPipe.building.transform.position);
        }
        else
        {
            lastLastPos = Utility.Instance.GetTileCoordsFromPosition(lastLastPipe.adjuster.transform.position);
        }


        if (loc.Equals(lastPos) || loc.Equals(lastLastPos))
        {
            return false;
        }

        //adjecently next to lastPos, and not the same tile as lastLastPos
        if (((Mathf.Abs(lastPos.x - loc.x) == 1 && Mathf.Abs(lastPos.y - loc.y) == 0) ||
            (Mathf.Abs(lastPos.x - loc.x) == 0 && Mathf.Abs(lastPos.y - loc.y) == 1)))
        {
            return true;
        }

        return false;

    }

    public int GetConnectedPipesToPillarCount(Building building)
    {
        int count = 0;
        foreach (List<Pipe> pipeline in pipelines)
        {
            if (pipeline.Count != 0)
            {
                if (pipeline[0].building == building)
                {
                    count++;
                }
            }
        }

        return count;
    }

    public List<Building> GetConnectionsToPillar(Building building)
    {
        List<Building> connections = new List<Building>();
        foreach (List<Pipe> pipeline in pipelines)
        {
            if (pipeline.Count != 0)
            {
                if (pipeline[0].building == building && pipeline[pipeline.Count - 1].building != null)
                {
                    connections.Add(pipeline[pipeline.Count - 1].building);
                }
            }
        }

        return connections;
    }

    public void PlacePipe(GameObject newPipe)
    {
        Pipe pipe = new Pipe();
        pipe.element = curElement;
        pipe.adjuster = newPipe.GetComponent<PipeAdjuster>();

        pipe.adjuster.pipeLineNum = curPipelineNum;

        pipelines[curPipelineNum].Add(pipe);

        //adjust the previous pipe in line
        if (pipelines[curPipelineNum].Count > 2)
        {
            int posL = GetPipePosition(pipelines[curPipelineNum].Count - 2);
            pipelines[curPipelineNum][pipelines[curPipelineNum].Count - 2].adjuster.UpdatePipe(posL);
        }
        int posC = GetPipePosition(pipelines[curPipelineNum].Count - 1);
        pipelines[curPipelineNum][pipelines[curPipelineNum].Count - 1].adjuster.UpdatePipe(posC);
    }

    public void UpdateConnectedBuilding(Building oldBuilding, Building newBuilding)
    {
        int pipeNum = GetPipelineForEndBuilding(oldBuilding);

        if (pipeNum == -1)
        {
            //no building to update
            return;
        }

        pipelines[pipeNum][pipelines[pipeNum].Count-1].building = newBuilding;

        newBuilding.AddElement(pipelines[pipeNum][0].element);
    }

    public int GetPipelineForEndBuilding(Building building)
    {
        for (int i = 0; i < pipelines.Count; i++)
        {
            if (pipelines[i].Count != 0)
            {
                if (pipelines[i][pipelines[i].Count-1].building == building)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public void RemoveBuildingFromPipeline(Building building)
    {
        int pipeNum = GetPipelineForEndBuilding(building);

        if (pipeNum == -1)
        {
            //no connection found
            return;
        }

        //remove last, and update the one before last
        pipelines[pipeNum].RemoveAt(pipelines[pipeNum].Count - 1);

        int pos = GetPipePosition(pipelines[pipeNum].Count - 1);
        pipelines[pipeNum][pipelines[pipeNum].Count - 1].adjuster.UpdatePipe(pos);

    }

    //Key for that is as follows:
    // 0 -> top
    // 1 -> bottom
    // 2 -> left
    // 3 -> right
    // 4 -> bottom + top (vertical)
    // 5 -> left + right (horizontal)
    // 6 -> top + left (corner)
    // 7 -> top + bottom (corner)
    // 8 -> bottom + left (corner)
    // 9 -> bottom + right (corner)
    private int GetPipePosition(int index)
    {
        Vector3 lastPos;

        if (pipelines[curPipelineNum][index-1].building != null)
        {
            lastPos = pipelines[curPipelineNum][index - 1].building.transform.position;
        }
        else
        {
            lastPos = pipelines[curPipelineNum][index - 1].adjuster.transform.position;
        }

        Vector3 curPos = pipelines[curPipelineNum][index].adjuster.transform.position;

        Vector3 nextPos = Vector3.zero;

        if (pipelines[curPipelineNum].Count > index + 1)
        {
            if (pipelines[curPipelineNum][index+1].building != null)
            {
                nextPos = pipelines[curPipelineNum][index+1].building.transform.position;
            }
            else
            {
                nextPos = pipelines[curPipelineNum][index + 1].adjuster.transform.position;
            }
        }


        //no next pos
        if (nextPos.Equals(Vector3.zero))
        {
            // nub going from top/bottom/left/right
            if (lastPos.x < curPos.x)
            {
                return 2;
            }
            else if (lastPos.x > curPos.x)
            {
                return 3;
            }
            else if (lastPos.z < curPos.z)
            {
                return 1;
            }
            else if (lastPos.z > curPos.z)
            {
                return 0;
            }
        }

        //horizontal line
        if ((lastPos.x < curPos.x && curPos.x < nextPos.x) || (nextPos.x < curPos.x && curPos.x < lastPos.x))
        {
            return 5;
        }
        //vertical
        if ((lastPos.z < curPos.z && curPos.z < nextPos.z) || (nextPos.z < curPos.z && curPos.z < lastPos.z))
        {
            return 4;
        }
        //corners
        //top+left
        if ((lastPos.x < curPos.x && nextPos.z > curPos.z) || (lastPos.z > curPos.z && nextPos.x < curPos.x))
        {
            return 6;
        }
        //top+right
        if ((lastPos.x > curPos.x && nextPos.z > curPos.z) || (lastPos.z > curPos.z && nextPos.x > curPos.x))
        {
            return 7;
        }
        //bottom+left
        if ((lastPos.x < curPos.x && nextPos.z < curPos.z) || (lastPos.z < curPos.z && nextPos.x < curPos.x))
        {
            return 8;
        }
        //bottom+right
        if ((lastPos.x > curPos.x && nextPos.z < curPos.z) || (lastPos.z < curPos.z && nextPos.x > curPos.x))
        {
            return 9;
        }


        Debug.LogError($"Unable to get pipe position as no contraints matched...\n{lastPos}\n{curPos}\n{nextPos}");
        return -1;
    }

    public Building GetEndOfPipeBuilding(int pipelineNum)
    {
        int index = pipelines[pipelineNum].Count - 1;
        if (pipelines[pipelineNum][index].building != null)
        {
            return pipelines[pipelineNum][index].building;
        }

        return null;
    }

    public List<Pipe> GetPipeLine(int pipelineNum)
    {
        return pipelines[pipelineNum];
    }

    public void DestroyPipeline(int pipelineNum)
    {
        pipelines[pipelineNum].Clear();
    }
}
