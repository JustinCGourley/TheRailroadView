using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionController : MonoBehaviour
{
    Vector3 startPos = Vector3.down;
    public bool isSelecting { get { return !startPos.Equals(Vector3.down); } }

    List<AIUnitController> selectedUnits;
    List<AIResearcherController> selectedResearchers;

    Dictionary<int, Vector2Int> claimedLocations = new Dictionary<int, Vector2Int>();

    float startDrawTime;

    // Start is called before the first frame update
    void Start()
    {
        selectedUnits = new List<AIUnitController>();
        selectedResearchers = new List<AIResearcherController>();
    }

    // TODO - restructure this so that it will always pick units first, and researchers second
    public void UpdateController(GameManager.PlayState playState)
    {
        if (Time.time - startDrawTime <= Constants.START_DRAW_DELAY_TIME) 
            return;

        if (playState == GameManager.PlayState.night)
            SelectUnits();
        else if (playState == GameManager.PlayState.day)
            SelectResearchers();
    }

    private void SelectUnit(AIUnitController unit)
    {
        if (unit.IsLeader && !selectedUnits.Contains(unit))
        {
            unit.SetSelectUnit(true);
            selectedUnits.Add(unit);
        }
        else if (!selectedUnits.Contains(unit.Leader))
        {
            unit.Leader.SetSelectUnit(true);
            selectedUnits.Add(unit.Leader);
        }
    }

    private void SelectResearcher(AIResearcherController researcher)
    {
        researcher.SetSelectUnit(true);
        selectedResearchers.Add(researcher);
    }

    void SelectUnits()
    {
        Vector3 endPos = Input.mousePosition;

        foreach (AIUnitController unit in selectedUnits)
        {
            unit.SetSelectUnit(false);
        }

        List<AIUnitController> newSelect = GameManager.Instance.GetControllerInBounds<AIUnitController>(startPos, endPos, true);

        selectedUnits.Clear();

        foreach (AIUnitController unit in newSelect)
        {
            SelectUnit(unit);
        }
    }

    void SelectResearchers()
    {
        Vector3 endPos = Input.mousePosition;

        foreach (AIResearcherController unit in selectedResearchers)
        {
            unit.SetSelectUnit(false);
        }

        selectedResearchers = GameManager.Instance.GetControllerInBounds<AIResearcherController>(startPos, endPos, false);

        foreach (AIResearcherController unit in selectedResearchers)
        {
            unit.SetSelectUnit(true);
        }
    }

    public void EndDraw()
    {
        startPos = Vector3.down;
        UIManager.Instance.UpdateSelectorUI(Vector3.zero, Vector3.zero, false);
    }

    public void StartDraw(ClickUnitResult selectedUnit)
    {
        foreach (AIUnitController unit in selectedUnits)
        {
            unit.SetSelectUnit(false);
        }

        foreach (AIResearcherController unit in selectedResearchers)
        {
            unit.SetSelectUnit(false);
        }

        selectedUnits.Clear();
        selectedResearchers.Clear();

        if (selectedUnit.valid)
        {
            if (selectedUnit.type == AIType.Unit)
            {
                SelectUnit(selectedUnit.unitController);
            }
            else if (selectedUnit.type == AIType.Researcher)
            {
                SelectResearcher(selectedUnit.researchController);
            }
        }

        this.startPos = Input.mousePosition;
        startDrawTime = Time.time;
    }

    public void ClearUnitClaims()
    {
        claimedLocations.Clear();
    }

    public void MoveSelectedUnits(Vector3 pos, bool isUnit)
    {
        Vector3 tile = Utility.Instance.GetCenterOfTile(pos);
        if (isUnit)
        {
            foreach (AIUnitController unit in selectedUnits)
            {
                Vector3 nearestSpace = FindNearestOpenSpace(tile);
                unit.MoveToClickPos(nearestSpace);
                claimedLocations[unit.info.unitNumber] = Utility.Instance.GetTileCoordsFromPosition(nearestSpace);
            }
        }
        else
        {
            foreach (AIResearcherController researcher in selectedResearchers)
            {
                Vector3 nearestSpace = FindNearestOpenSpace(tile);
                researcher.MoveToClickPos(nearestSpace);
                claimedLocations[researcher.info.unitNumber] = Utility.Instance.GetTileCoordsFromPosition(nearestSpace);
            }
        }
    }

    public void MoveUnitsToOpenSpace(List<AIUnitController> units, Vector3 pos)
    {
        pos = Utility.Instance.GetCenterOfTile(pos);

        foreach (AIUnitController unit in units)
        {
            Vector3 nearestSpace = FindNearestOpenSpace(pos);
            unit.MoveToClickPos(nearestSpace);
            claimedLocations[unit.info.unitNumber] = Utility.Instance.GetTileCoordsFromPosition(nearestSpace);
        }
    }

    public void MoveResearchersToOpenSpace(List<AIResearcherController> researchers, Vector3 pos)
    {
        pos = Utility.Instance.GetCenterOfTile(pos);

        foreach (AIResearcherController researcher in researchers)
        {
            Vector3 nearestSpace = FindNearestOpenSpace(pos);
            researcher.transform.position = nearestSpace;
            claimedLocations[researcher.info.unitNumber] = Utility.Instance.GetTileCoordsFromPosition(nearestSpace);
        }
    }

    private Vector3 FindNearestOpenSpace(Vector3 pos)
    {
        int maxSearchAmount = 11;

        string claimedLocDebug = "";
        foreach (int loc in claimedLocations.Keys)
        {
            claimedLocDebug += $"{loc}: {claimedLocations[loc]} | ";
        }

        for (int i = 0; i < maxSearchAmount; i+=1)
        {
            Vector3 searchPos = pos - new Vector3(i, i);
            int searchDir = 1;

            for (int j = 0; j < (i * i) - (i-2 * i-2); j++)
            {
                Vector2Int tilePos = Utility.Instance.GetTileCoordsFromPosition(searchPos);

                if (!LevelManager.Instance.TileIsOccupiedBuilding(tilePos) && !claimedLocations.ContainsValue(tilePos))
                {
                    return searchPos;
                }
                else
                {
                    if (searchDir == 1)
                    {
                        searchPos.z += 1;
                        if (searchPos.z >= pos.z + i)
                        {
                            searchDir++;
                        }
                    }
                    else if (searchDir == 2)
                    {
                        searchPos.x += 1;
                        if (searchPos.x >= pos.x + i)
                        {
                            searchDir++;
                        }
                    }
                    else if (searchDir == 3)
                    {
                        searchPos.z -= 1;
                        if (searchPos.z <= pos.z - i)
                        {
                            searchDir++;
                        }
                    }
                    else if (searchDir == 4)
                    {
                        searchPos.x -= 1;
                        if (searchPos.x <= pos.x - i)
                        {
                            searchDir++;
                        }
                    }
                }
            }
            
        }

        Debug.LogError("Unable to find a nearby open spot to place unit");
        return Vector3.zero;
            
    }
}
