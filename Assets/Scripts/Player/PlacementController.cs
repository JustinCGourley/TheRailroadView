using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementController : MonoBehaviour
{

    public GameObject cursorObj;

    private ObjectInfo curObj;
    private GameObject placingObj;
    private bool placing;
    private Transform levelMap;

    private LevelManager levelManager;
    private GameManager gameManager;

    Vector2Int curPos;


    // Start is called before the first frame update
    void Start()
    {
        levelManager = LevelManager.Instance;
        curPos = Vector2Int.zero;

        levelMap = GameObject.Find(Constants.GAMEOBJECT_LEVELMAP).transform;
        gameManager = GameObject.Find(Constants.GAMEOBJECT_GAMEMANAGER).GetComponent<GameManager>();

        placing = false;
    }

    /// <summary>
    /// Called by GameManager to update placement
    /// </summary>
    public void UpdatePlacement()
    {
        
    }

    private void Update()
    {
        CheckCursor();
    }

    public void DidLeftClick()
    {
        PlaceObject();
    }

    public void DidRightClick()
    {
        //dont allow this if the object is the nexus
        if (curObj.obj.name.Equals("nexus")) { return; }
        Destroy(placingObj);
        placing = false;
        placingObj = null;
        SetCursorEnabled(true);
        gameManager.StopBuilding(null, false);
    }

    private void CheckCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        int layerMask = LayerMask.GetMask("Selectable", "Default", "Building");
        if (Physics.Raycast(ray, out hitInfo, layerMask))//ray cast from mouse position out of camera
        {
            curPos = Vector2Int.FloorToInt(Utility.Instance.GetTileCoordsFromPosition(hitInfo.point));
            cursorObj.transform.position = new Vector3(
                curPos.x + ((float)(Constants.LEVEL_TILE_WIDTH) / 2),
                levelManager.GetHeightAtPos(curPos), 
                curPos.y + ((float)(Constants.LEVEL_TILE_HEIGHT) / 2)
                );
            //Debug.Log(hitInfo.transform.gameObject.name);
            if (gameManager.isDebug())
            {
                //update text ui for debug
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(cursorObj.transform.position);
                Vector3 textUIPos = screenPoint - new Vector3(Screen.width / 2, Screen.height / 2) + ((screenPoint.x > Screen.width / 2) ? new Vector3(-120, 0, 0) : new Vector3(120, 0, 0));
                UIManager.Instance.SetCursorTextAndPosition($"[x: {curPos.x}, y: {curPos.y}]", textUIPos);
            }
        }
    }

    public void StartPlaceObject(ObjectInfo objInfo)
    {
        //remove current object
        if (placingObj != null)
        {
            Destroy(placingObj);
            placingObj = null;
        }

        curObj = objInfo;

        placingObj = Instantiate(curObj.obj, cursorObj.transform);
        placingObj.transform.localPosition = Vector3.zero;

        placing = true;

        //SetCursorEnabled(false);
    }

    private void SetCursorEnabled(bool enabled)
    {
        //This is a temp solution for this
        //this should be changed once an actual model is made for this
        cursorObj.GetComponent<CursorActivate>().SetActive(enabled);
    }

    private void PlaceObject()
    {
        //is obj null
        if (placingObj == null)
        {
            return;
        }
        //is obj NOT the nexus AND is obj in placeable area
        if (!gameManager.isCoordInPlacementArea(curPos) && !curObj.obj.name.Equals("nexus"))
        {
            return;
        }

        // extra pipe check for when we want to connect a pipe to a building
        if (curObj.obj.GetComponent<Building>().towerType == TowerType.pipe)
        {
            //if we are just trying to connect to a building, return and connect it
            if (GameManager.Instance.CheckIsConnectingPipe(Utility.Instance.GetTileCoordsFromPosition(placingObj.transform.position)))
            {
                return;
            }
        }

        //is tile occupied
        if (levelManager.TileIsOccupied(curPos))
        {
            return;
        }

        //if pipe do an extra placement check
        if (curObj.obj.GetComponent<Building>().towerType == TowerType.pipe)
        {
            if (!gameManager.CheckCanPlacePipe(Utility.Instance.GetTileCoordsFromPosition(placingObj.transform.position)))
            {
                return;
            }
        }


        if (!ResourceManager.Instance.CheckResourceCost(curObj.woodCost, curObj.stoneCost, curObj.workerCost) || !gameManager.HasEnoughManaToSpend(curObj.manaCost))
        {
            Debug.Log("Nice try! You cant affort that");
            UIManager.Instance.AddInfoMessage("You dont have the resources for that");
            return;
        }


        //check to make sure there is still a valid path out of the nexus
        if (IsNexusBlockedWithBuilding(curPos, curObj, curObj.obj.GetComponent<Building>()) && !curObj.obj.name.Equals("nexus"))
        {
            Debug.Log("Placing this would mean there is no valid path");
            UIManager.Instance.AddInfoMessage("You have to have a valid path to your nexus, this would stop a valid path from existing");
            return;
        }

        placingObj.transform.parent = levelMap.transform;
        placingObj.transform.position = new Vector3(curPos.x + ((Constants.LEVEL_TILE_WIDTH + 0f)/2f), levelManager.GetHeightAtPos(curPos), curPos.y + ((Constants.LEVEL_TILE_HEIGHT + 0f) / 2f));

        levelManager.UpdateLevelWithBuilding(curPos, placingObj, curObj.objType);

        curObj.obj.GetComponent<Building>().buildingStart(placingObj.transform.position, Utility.Instance.GetTileCoordsFromPosition(placingObj.transform.position));

        ResourceManager.Instance.SpendResources(curObj.woodCost, curObj.stoneCost);
        GameManager.Instance.SpendMana(curObj.manaCost);

        bool stillPlacing = true;
        if (curObj.obj.name.Equals("nexus")) { stillPlacing = false; }
        gameManager.StopBuilding(placingObj, stillPlacing);

        placingObj = null;
        if (stillPlacing)
        {
            StartPlaceObject(curObj);
        }
    }

    private bool IsNexusBlockedWithBuilding(Vector2Int pos, ObjectInfo obj, Building building)
    {
        //temp change level to update PathFinding nav array, undo this at the end
        LevelManager.Instance.UpdateLevelWithBuilding(pos, building.gameObject, obj.objType);


        int[,] placementArea = GameManager.Instance.GetPlacementArea();

        Vector2Int emptyPlace = Vector2Int.zero;
        for (int i = 0; i < placementArea.GetLength(0); i++)
        {
            for (int j = 0; j < placementArea.GetLength(1); j++)
            {
                if (i != 0 && placementArea[i,j] == 1 && placementArea[i-1,j] == 0)
                {
                    emptyPlace = new Vector2Int(i-1,j);
                    break;
                }
            }
        }

        List<PathNode> nodes = PathFinding.Instance.FindPath(GameManager.Instance.GetNexusCoords(), emptyPlace, true);

        LevelManager.Instance.RemoveBuildingOnTile(pos);

        return nodes == null;
    }

}
