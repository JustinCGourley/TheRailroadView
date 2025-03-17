using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public struct ClickUnitResult
{
    public AIUnitController unitController;
    public AIResearcherController researchController;
    public AIType type;
    public bool valid;
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    public GameObject theSun;
    [SerializeField] ObjectInfo nexusInfo;
    public ObjectInfo GetNexusInfo() { return nexusInfo; }
    public GameObject placeableTileObj;
    public Material placementAreaMaterial;

    GameObject nexus = null;
    Vector2Int nexusCoords;
    bool placingNexus = false;

    public bool isPlacingBuilding = false;
    public enum PlayState
    {
        start = 0,
        day = 1,
        night = 2
    };
    PlayState playState = PlayState.start;
    public PlayState GetPlayState { get { return playState; } }

    int placementAreaRadius = 0;
    GameObject placementAreaObj = null;
    int[,] placementArea;
    public int[,] GetPlacementArea() { return placementArea; }
    int nexusHealth;
    int currentMana;

    PlacementController placementController;
    SelectionController selectionController;
    LevelManager levelManager;
    TextureCreator textureCreator;
    WaveManager waveManager;
    UIManager uiManager;
    SunController sunController;
    AIManager aiManager;
    PipeManager pipeManager;

    //positions for the sun to be in dur   | start | day | night |
    List<float> sunPositions = new List<float> { 100, 250, 360 };

    List<Building> buildings = new List<Building>();
    List<Building_ControlledTower> controlledTowers = new List<Building_ControlledTower>();
    List<Building_UnitTower> unitTowers = new List<Building_UnitTower>();
    List<Building> pillarBuildings = new List<Building>();

    UseButtonUI selectedControlledBuilding = null;

    //used specifically for showing buildingUI
    ObjectInfo selectedBuildingInfo = null;
    GameObject selectedBuildingObj = null;

    bool debug;
    int lastSkipDay = -10;

    //initialization
    void Start()
    {
        placementController = GameObject.Find(Constants.GAMEOBJECT_PLAYERCONTROLLER).GetComponent<PlacementController>();
        selectionController = GameObject.Find(Constants.GAMEOBJECT_PLAYERCONTROLLER).GetComponent<SelectionController>();
        aiManager = GameObject.Find(Constants.GAMEOBJECT_AIMANAGER).GetComponent<AIManager>();
        levelManager = GameObject.Find(Constants.GAMEOBJECT_LEVELMANAGER).GetComponent<LevelManager>();
        waveManager = WaveManager.Instance;
        uiManager = GameObject.Find(Constants.GAMEOBJECT_UI).GetComponent<UIManager>();
        sunController = GameObject.Find(Constants.GAMEOBJECT_SUN).GetComponent<SunController>();
        placementArea = new int[Constants.LEVEL_SIZE, Constants.LEVEL_SIZE];
        pipeManager = this.GetComponent<PipeManager>();

        GameObject levelMapObj = GameObject.Find(Constants.GAMEOBJECT_LEVELMAP);
        placementAreaObj = Instantiate(levelMapObj);
        textureCreator = new TextureCreator();
        placementAreaMaterial.mainTexture = textureCreator.CreatePlacementTexture(placementArea, 1);
        placementAreaObj.GetComponent<Renderer>().material = placementAreaMaterial;

        placementAreaObj.transform.name = Constants.GAMEOBJECT_PLACEMENTMAP;
        placementAreaObj.transform.position += new Vector3(0f, 0.01f, 0f);
        Utility.Instance.CombineAllChildrenMeshes(placementAreaObj);
        SetVisualizeBoundsActive(false);
        
        sunController.MoveSunToRotation(sunPositions[0], 0f);

        nexusHealth = 100;

        uiManager.OpenBuildingMenu(false);
        uiManager.SetBottomMenuActive(true);
        uiManager.UpdatePopulationUI();

        uiManager.AddInfoMessage("Welcome to the game!\nPlace down your nexus to continue!");
    }

    // ============================================================================================
    // UPDATE BABY

    void Update()
    {
        CheatStuff();


        if (playState == PlayState.start)
        {
            if (needToPlaceNexus())
            {
                DealWithNexusPlacement();
                return; //if we are dealing with nexus placement only deal with this
            }
        }
        //if (playState == PlayState.day)
        //{
            
        //}
        //if (playState == PlayState.night)
        //{
            
        //}

        if (selectionController.isSelecting)
        {
            selectionController.UpdateController(playState);
        }
        foreach (Building building in buildings)
        {
            if (building.doesBuildingUpdateInDay())
            {
                building.buildingUpdate();
            }
        }

        UpdateTowerAndEnemies();
    }

    private void CheatStuff()
    {
        //TODO: remove this
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Space) && playState == PlayState.day)
        {
            uiManager.AddInfoMessage("Skipping to night!");
            ForceEndDay();
            lastSkipDay = ProgressionManager.Instance.CurrentDay;
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
        {
            currentMana += 100;
            ResourceManager.Instance.AddResource(ResourceType.wood, 100);
            ResourceManager.Instance.AddResource(ResourceType.stone, 100);
            ResourceManager.Instance.AddResource(ResourceType.researchOrb, 50);
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.K))
        {
            aiManager.KillRandomPerson();
            if (aiManager.GetTotalWorkerCount() == 0)
            {
                aiManager.GenerateAI(nexus.transform.position);
            }
            uiManager.UpdatePopulationUI();
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L))
        {
            aiManager.GenerateAI(nexus.transform.position);
            uiManager.UpdatePopulationUI();
        }
    }

    private void UpdateTowerAndEnemies()
    {
        //run all towers except controlled + unit
        foreach (Building building in buildings)
        {
            if (building.towerType != TowerType.controlled && (building.doesBuildingUpdate() || building.isTower()))
            {
                building.buildingUpdate();
            }
        }

        //run controlled buildings
        foreach (Building_ControlledTower controlledTower in controlledTowers)
        {
            controlledTower.buildingUpdate();
        }

        uiManager.UpdateUseTowerUI(selectedControlledBuilding);

        if (selectionController.isSelecting)
        {
            selectionController.UpdateController(playState);
        }


        waveManager.UpdateEnemies();
        waveManager.UpdateSpawner();
    }
    // ============================================================================================


    // ============================================================================================
    // New Day Phase stuff 
    public void StartNewDay()
    {
        if (playState != PlayState.start) //not on first play
        {
            aiManager.CyclePeople();
            aiManager.TimeToEat();
        }
        playState = PlayState.start;
        sunController.MoveSunToRotation(sunPositions[0], 0f);
        sunController.SetRadial(1f);

        //uiManager.SetBottomMenuActive(true);
        //uiManager.SetStartButton(true);
        uiManager.CloseUseTowerUI();
        EnvironmentManager.Instance.DestroyAllHazards();

        ClearSelected();
        uiManager.CloseUseTowerUI();

        ProgressionManager.Instance.ProgressDay();

        // kill all units
        foreach (Building_UnitTower unitTower in unitTowers)
        {
            unitTower.KillUnits();
        }

        //add mana
        AddManaForDay();

        uiManager.UpdateDayText();
        uiManager.UpdatePopulationUI();

        StartDay();
    }

    // ============================================================================================
    // Day Phase stuff (anything that happens after user has clicked start day)
    #region Day Stuff
    public void StartDay()
    {

        if (uiManager.IsOverProvisioned())
        {
            Debug.LogWarning("Unable to start day as you are overprovised in the work menu!!!!");
            return;
        }

        playState = PlayState.day;

        sunController.MoveSunToRotation(sunPositions[1], Constants.DAY_END_TIME);
        sunController.StartRadial(Constants.DAY_END_TIME);
        //uiManager.SetStartButton(false);
        //uiManager.OpenResearchMenu(false);

        //TODO - idk consider how u wana do the end button thing, might not have it at all and just have time changing buttons instead
        //uiManager.SetEndButtonActive(true);

        //create research orbs
        OrbManager.Instance.CreateResearchOrbs(20);

        Debug.Log("Creating Day Jobs");
        JobManager.Instance.CreateDayJobs();
        selectionController.ClearUnitClaims();
        uiManager.UpdatePopulationUI();
        //uiManager.SetupBuildingMenu();
        //uiManager.OpenBuildingMenu(true);
        //uiManager.SetBottomMenuActive(false);

    }

    public void ForceEndDay()
    {
        if (ProgressionManager.Instance.CurrentDay - lastSkipDay <= 0) return;
        EndDay();
        sunController.SetRadial(0f);
    }

    public void EndDay()
    {
        float timeTracker = Time.time;
        int startDay = ProgressionManager.Instance.CurrentDay;

        if (playState == PlayState.night)
        {
            Debug.LogWarning("Its already night, skipping endDay");
            return;
        }

        aiManager.EndDay();
        //placementController.DidRightClick(); //stop any building placement
        UpdateAllBuildings();
        uiManager.SetUpUseTowerUI(GetControlledBuildings());

        //if (!aiManager.AreAllWorkersDone())
        //{
        //    aiManager.FinishWorkerJobs();
        //}
        bool skip = false;
        while (playState != PlayState.night && !skip)
        {
            if (playState == PlayState.night || playState == PlayState.start || startDay != ProgressionManager.Instance.CurrentDay)
            {
                Debug.LogWarning("Its already the next day/night, skipping endDay");
                skip = true;
            }
            else
            {
                StartNight();
            }
        }

    }

    public int GetAvailableWorkers()
    {
        return aiManager.GetAvailableWorkerCount();
    }

    // ran at the end of the day
    // rn its just to update all pillar's info to buildings
    private void UpdateAllBuildings()
    {
        foreach (Building pillarBuilding in pillarBuildings)
        {
            List<Building> connections = pipeManager.GetConnectionsToPillar(pillarBuilding);
            foreach (Building connection in connections)
            {
                Debug.Log($"Updating {connection.name} to element {pillarBuilding.element}");
                connection.element = pillarBuilding.element;
            }
        }
    }

    public Vector3 GetClosestStoragePosition(Vector3 position)
    {
        return Utility.Instance.GetPositionFromTileCoords(nexusCoords, true);
    }

    public void BuildingButtonPressed(ObjectInfo objInfo)
    {
        if ((ResourceManager.Instance.CheckResourceCost(objInfo.woodCost, objInfo.stoneCost, objInfo.workerCost) && HasEnoughManaToSpend(objInfo.manaCost)))
        {
            isPlacingBuilding = true;

            placementController.StartPlaceObject(objInfo);
            SetVisualizeBoundsActive(true);
        }
        else
        {
            Debug.Log(".... You cant affort that dawg");
            uiManager.AddInfoMessage("You cant afford that building");
        }

    }

    // NOTE: Anything Added to this method should also be added to the 'DestroyBuilding' method
    public void StopBuilding(GameObject obj, bool stillPlacing)
    {
        isPlacingBuilding = stillPlacing;
        SetVisualizeBoundsActive(stillPlacing);

        if (obj == null || (obj.name.Equals("nexus(Clone)") && nexus != null)) return;

        if (obj.GetComponent<Building>() == null)
        {
            Debug.LogError("Building came through as null");
            return;
        }

        TowerType towerType = obj.GetComponent<Building>().towerType;


        if (towerType == TowerType.nexus)
        {
            SetNexusCoords(Utility.Instance.GetTileCoordsFromPosition(obj.transform.position), obj);
            SetPlacementArea(0, 4);
            StartNewDay();
            return;
        }


        if (towerType == TowerType.pillar)
        {
            Debug.Log("Adding pillar " + obj.name);
            pillarBuildings.Add(obj.GetComponent<Building>());
            return;
        }

        buildings.Add(obj.GetComponent<Building>());
        Debug.Log("Adding building " + obj.name);


        if (towerType == TowerType.controlled)
        {
            Debug.Log("Adding controlled tower");
            Debug.Log("Adding controlled: " + obj.GetComponent<Building_ControlledTower>());
            controlledTowers.Add(obj.GetComponent<Building_ControlledTower>());
        }
        else if (towerType == TowerType.unit)
        {
            Debug.Log("Adding unit: " + obj.GetComponent<Building_UnitTower>());
            Debug.Log("Adding unit tower");
            unitTowers.Add(obj.GetComponent<Building_UnitTower>());
            JobManager.Instance.GetWorkersForUnitBuilding(obj);
        }
        else if (towerType == TowerType.storage)
        {
            if (obj.GetComponent<Building_Storage>().storageType == StorageType.mana)
            {
                uiManager.UpdateManaUI(currentMana, GetAllManaStorage());
            }
        }
        else if (towerType == TowerType.pipe)
        {
            pipeManager.PlacePipe(obj);
        }
        uiManager.UpdatePopulationUI();

        // spend workers for building
        JobManager.Instance.SpendWorkersForBuilding(obj.GetComponent<Building>());
    }

    #endregion

    // ============================================================================================
    // Night phase stuff
    #region Night Stuff
    public void StartNight()
    {
        Debug.Log("Starting Night");

        playState = PlayState.night;
        // spawn a new wave every 2 days
        if (true || ProgressionManager.Instance.CurrentDay % 2 == 0)
        {
            waveManager.GenerateWaveSpawn();
        }
        sunController.MoveSunToRotation(sunPositions[2], 2f);
        sunController.StartRadial(0f);

        //uiManager.OpenBuildingMenu(false);
        //uiManager.SetEndButtonActive(false);

        aiManager.RemoveResearchers();

        OrbManager.Instance.RemoveOrbs();

        // spawn all units
        foreach (Building_UnitTower unitTower in unitTowers)
        {
            unitTower.SpawnUnits();
        }
    }

    public void TakeDamage(int damage)
    {
        nexusHealth -= damage;
        uiManager.UpdateHealthUI(nexusHealth, 100);
        if (nexusHealth < 0)
        {
            DieIGuess();
        }
    }

    // returns list of controllers (anyone found in active controller list) based on a given start and end position
    // startPos - the START of where the PLAYER CLICKed
    // endPos - the END of where the PLAYER CLICKed
    // isUnits - distinction between which list to use
    // NOTE - startPos is not necessarily the furtherest left point nor is endPos the farthest up/down
    public List<T> GetControllerInBounds<T>(Vector3 startPos, Vector3 endPos, bool isUnits)
    {

        UIManager.Instance.UpdateSelectorUI(startPos, endPos);

        Vector3 startWorldPos = Utility.Instance.GetWorldPositionFromMousePosition(startPos);
        Vector3 endWorldPos = Utility.Instance.GetWorldPositionFromMousePosition(endPos);

        // get mid-point for each position 
        float cX = Utility.Instance.GetPositionCenter(startWorldPos.x, endWorldPos.x);
        float cZ = Utility.Instance.GetPositionCenter(startWorldPos.z, endWorldPos.z);

        float sizeX = Utility.Instance.GetSizeFromCenter(cX, startWorldPos.x);
        float sizeZ = Utility.Instance.GetSizeFromCenter(cZ, startWorldPos.z);


        List<T> selectedUnits = new List<T>();
        foreach (T unit in aiManager.GetActiveControllers<T>(isUnits))
        {
            AIController unitC = unit as AIController;
            Vector3 unitPos = unitC.aiMovement.Position;

            if (unitPos.x >= cX - sizeZ && unitPos.x <= cX + sizeX &&
                unitPos.z >= cZ - sizeZ && unitPos.z <= cZ + sizeZ)
            {
                selectedUnits.Add(unit);
            }
        }
        Vector3 start = new Vector3(cX - sizeX, 0, cZ - sizeZ);
        Vector3 end = new Vector3(cX + sizeX, 0, cZ + sizeZ);

        return selectedUnits;
    }

    public List<AIUnitController> GetUnitsInRange(Vector3 pos, float radius)
    {
        return aiManager.GetUnitsInRange(pos, radius);
    }

    /// <summary>
    /// Select a tower (controlled / unit)
    /// </summary>
    /// <param name="type"> 0 (controlled) or 1 (unit)</param>
    /// <param name="index"></param>
    public void SelectControlledTower(UseButtonUI building)
    {
        Debug.Log($"Selected building from {building.objs.Count}");

        selectedControlledBuilding = building;
    }

    public void ClearSelected(bool updateUI = true)
    {
        selectedControlledBuilding = null;
    }

    private void UseSelectedTower()
    {
        Debug.Log("Using tower");

        foreach (Building_ControlledTower tower in selectedControlledBuilding.objs)
        {
            if (tower.timeLeftUntilFire() <= 0)
            {
                Debug.Log("didClick active");
                tower.didClick();
                return;
            }
        }
    }

    #endregion
    // ============================================================================================
    // Mana Stuff
    #region Mana Stuff

    private void AddManaForDay()
    {
        // add x amount of mana to pool per person active
        int manaToAdd = aiManager.GetTotalWorkerCount() * Constants.MANA_ADD_PERSON_AMOUNT;
        int maxMana = GetAllManaStorage();

        Debug.Log($"Adding mana! {aiManager.GetTotalWorkerCount()} people gives us {manaToAdd}");

        if (manaToAdd + currentMana > maxMana)
        {
            currentMana = maxMana;
        }
        else
        {
            currentMana += manaToAdd;
        }


        uiManager.UpdateManaUI(currentMana, maxMana);
    }

    private int GetAllManaStorage()
    {
        int storageAmount = Constants.MANA_NEXUS_START;
        foreach (Building building in buildings)
        {
            if (building.towerType == TowerType.storage && building.GetComponent<Building_Storage>().storageType == StorageType.mana)
            {
                Debug.Log("Adding storage!");
                storageAmount += building.GetComponent<Building_Storage>().storageAmount;
                Debug.Log(storageAmount);
            }
        }

        return storageAmount;
    }

    public bool HasEnoughManaToSpend(int amount)
    {
        return currentMana - amount >= 0;
    }

    public void SpendMana(int amount)
    {
        if (currentMana < amount)
        {
            Debug.LogError($"Unable to spend {amount} of mana!!");
        }

        currentMana -= amount;
        uiManager.UpdateManaUI(currentMana, GetAllManaStorage());
    }
    #endregion
    // ============================================================================================
    // game ui / tower stuff?
    #region gameuiorsomething


    //TODO -> REDO UPGRADE STUFF
    public void UpgradeSelectedBuilding()
    {

        if (selectedBuildingInfo != null)
        {
            if (!ResourceManager.Instance.SpendResources(selectedBuildingInfo))
                return;

            Transform parent = selectedBuildingObj.transform.parent;
            Vector3 pos = selectedBuildingObj.transform.position;
            Quaternion rot = selectedBuildingObj.transform.rotation;
            int newTeir = selectedBuildingObj.GetComponent<Building>().buildingTier + 1;

            Debug.Log("Upgrading building");

            //check if building is part of a pipeline



            GameObject newObj = Instantiate(selectedBuildingInfo.obj, pos, rot, parent);

            pipeManager.UpdateConnectedBuilding(selectedBuildingObj.GetComponent<Building>(), newObj.GetComponent<Building>());

            DestroySelectedBuilding();

            Vector2Int tilePos = Utility.Instance.GetTileCoordsFromPosition(pos);
            LevelData data = levelManager.GetTileData(tilePos);
            data.obj = newObj;
            
            //update level and start building again
            newObj.GetComponent<Building>().buildingStart(pos, tilePos);

            levelManager.UpdateLevelData(data, tilePos);
            StopBuilding(newObj, false);

            selectedBuildingObj = newObj;
            uiManager.OpenBuildingInfoUI(selectedBuildingInfo, selectedBuildingObj);
        }
    }

    private void DestroySelectedBuilding()
    {
        buildings.Remove(selectedBuildingObj.GetComponent<Building>());

        TowerType towerType = selectedBuildingObj.GetComponent<Building>().towerType;
        if (towerType == TowerType.controlled)
        {
            controlledTowers.Remove(selectedBuildingObj.GetComponent<Building_ControlledTower>());
        }
        else if (towerType == TowerType.unit)
        {
            unitTowers.Remove(selectedBuildingObj.GetComponent<Building_UnitTower>());
        }
        else if (towerType == TowerType.storage)
        {
            if (selectedBuildingObj.GetComponent<Building_Storage>().storageType == StorageType.mana)
            {
                uiManager.UpdateManaUI(currentMana, GetAllManaStorage());
            }
        }
        else if (towerType == TowerType.pipe)
        {
            int pipelineNum = selectedBuildingObj.GetComponent<PipeAdjuster>().pipeLineNum;

            Building building = pipeManager.GetEndOfPipeBuilding(pipelineNum);
            if (building != null)
            {
                building.element = Element.none;
                building.isPiped = false;
            }

            List<Pipe> pipeline = pipeManager.GetPipeLine(pipelineNum);
            for (int i = 0; i < pipeline.Count; i++)
            {
                if (pipeline[i].adjuster != null)
                {
                    levelManager.UpdateLevelWithBuilding(Utility.Instance.GetTileCoordsFromPosition(pipeline[i].adjuster.transform.position), null, ObjectType.none);
                    Destroy(pipeline[i].adjuster.gameObject);
                }
            }

            pipeManager.DestroyPipeline(pipelineNum);

            return;
        }


        //check if we need to remove pipeline connection
        if (towerType == TowerType.controlled || towerType == TowerType.auto)
        {
            pipeManager.GetPipelineForEndBuilding(selectedBuildingObj.GetComponent<Building>());
        }

        levelManager.UpdateLevelWithBuilding(Utility.Instance.GetTileCoordsFromPosition(selectedBuildingObj.transform.position), null, ObjectType.none);

        Destroy(selectedBuildingObj);
    }

    public void SellSelectedBuilding()
    {
        uiManager.CloseBuildingInfoUI();
        DestroySelectedBuilding();
    }

    private void CheckClickBuilding()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))//ray cast from mouse position out of camera
        {
            if (hitInfo.collider.gameObject.CompareTag(Constants.TAG_BUILDING))
            {
                ObjectInfo objInfo = ProgressionManager.Instance.GetObjectInfoFromBuilding(hitInfo.collider.gameObject.GetComponent<Building>());
                if (objInfo != null)
                {
                    uiManager.OpenBuildingInfoUI(objInfo, hitInfo.collider.gameObject);
                    selectedBuildingInfo = objInfo;
                    selectedBuildingObj = hitInfo.collider.gameObject;
                }
                return;
            }
            else
            {
                uiManager.CloseBuildingInfoUI();
            }
        }
    }

    private ClickUnitResult CheckClickUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        ClickUnitResult result = new ClickUnitResult();
        result.valid = false;

        RaycastHit hitInfo;
        if (Physics.SphereCast(ray, Constants.SPHERE_CAST_SELECT_RADIUS, out hitInfo))
        {
            if (hitInfo.collider.gameObject.CompareTag(Constants.TAG_PERSON))
            {
                AIType type = hitInfo.collider.gameObject.GetComponent<AIController>().aiType;
                result.type = type;
                result.valid = type != AIType.Worker;

                switch (type)
                {
                    case AIType.Unit:
                        result.unitController = hitInfo.collider.gameObject.GetComponent<AIUnitController>();
                        break;
                    case AIType.Researcher:
                        result.researchController = hitInfo.collider.gameObject.GetComponent<AIResearcherController>();
                        break;
                }
            }
        }

        return result;
    }

    public void StartPipeBuild()
    {
        SetVisualizeBoundsActive(true);

        placementController.StartPlaceObject(pipeManager.GetBasePipeInfo);
        isPlacingBuilding = true;
        pipeManager.StartPlacePipe(selectedBuildingObj.GetComponent<Building>());
        uiManager.CycleBuildingMenu();
        uiManager.CloseBuildingInfoUI();
    }

    public void EditPipeBuild()
    {
        SetVisualizeBoundsActive(true);

        placementController.StartPlaceObject(pipeManager.GetBasePipeInfo);
        isPlacingBuilding = true;
        pipeManager.StartPlacePipe(selectedBuildingObj.GetComponent<PipeAdjuster>().pipeLineNum);
        uiManager.CycleBuildingMenu();
        uiManager.CloseBuildingInfoUI();
    }

    public void StopPipeBuild()
    {
        if (!pipeManager.isBuildingPipe) { return; }
        placementController.DidRightClick();
        pipeManager.EndPlacePipe();
        uiManager.OpenBuildingMenu(playState == PlayState.day);
        uiManager.OpenBuildingInfoUI(selectedBuildingInfo, selectedBuildingObj);
    }

    public bool CheckCanPlacePipe(Vector2Int pos)
    {
        return pipeManager.CanPlacePipe(pos);
    }

    public bool CheckIsConnectingPipe(Vector2Int pos)
    {
        return pipeManager.CheckIsConnectingPipe(pos);
    }

    public ObjectInfo GetPipeObjectInfo()
    {
        return pipeManager.GetBasePipeInfo;
    }

    public bool IsSelectedPipelineComplete(GameObject obj)
    {
        List<Pipe> pipeline = pipeManager.GetPipeLine(obj.GetComponent<PipeAdjuster>().pipeLineNum);
        return pipeline[pipeline.Count - 1].building != null;
    }

    public bool IsSelectedPillarFull(Building build)
    {
        int count = pipeManager.GetConnectedPipesToPillarCount(build);
        return count < 2;
    }

    // ============================================================================================`
    #endregion
    // ============================================================================================
    // Other stuff idk
    #region Other stuff

    public int WorkersNeededForJobs()
    {
        int jobsNeeded = 0;
        // go through all buildings and see how many people are currently going to get jobs
        foreach (Building building in GetAllBuildings())
        {
            jobsNeeded += building.maxJobs;
        }
        return jobsNeeded;
    }

    public int GetTotalWorkers()
    {
        return aiManager.GetTotalWorkerCount();
    }

    private void DealWithNexusPlacement()
    {
        if (needToPlaceNexus() && !placingNexus)
        {
            placementController.StartPlaceObject(nexusInfo);
            placingNexus = true;
            Debug.Log("Need to place your nexus!");
        }
        else if (needToPlaceNexus() && placingNexus)
        {
            placementController.UpdatePlacement();
        }
    }

    /// <summary>
    /// Display available building placement area
    /// </summary>
    private void InitializeBoundsVisualize()
    {
        placementArea = new int[Constants.LEVEL_SIZE, Constants.LEVEL_SIZE];
        Vector2Int curCoord = nexusCoords - new Vector2Int(placementAreaRadius, placementAreaRadius);
        int visualizeCount = Mathf.FloorToInt(Mathf.Pow(placementAreaRadius * 2 + 1, 2));
        for (int i = 0; i < visualizeCount; i++)
        {
            if (curCoord.x > 0 && curCoord.x < Constants.LEVEL_SIZE && curCoord.y > 0 && curCoord.y < Constants.LEVEL_SIZE)
            {
                placementArea[curCoord.x, curCoord.y] = 1;
            }
            curCoord.x ++;
            if (Mathf.Abs(curCoord.x - nexusCoords.x) > placementAreaRadius)
            {
                curCoord.x = nexusCoords.x - placementAreaRadius;
                curCoord.y ++;
            }
        }

        placementAreaMaterial.mainTexture = textureCreator.CreatePlacementTexture(placementArea, 1);
        placementAreaObj.GetComponent<Renderer>().material = placementAreaMaterial;
    }

    public void AddPlacementBounds(Vector2Int pos, int radius)
    {
        for (int i = 0; i < Constants.LEVEL_SIZE; i++)
        {
            for (int j = 0; j < Constants.LEVEL_SIZE; j++)
            {
                Vector2Int curPos = new Vector2Int(i, j);
                if ((curPos - pos).magnitude <= radius)
                {
                    placementArea[i, j] = 1;
                }
            }
        }
        placementAreaMaterial.mainTexture = textureCreator.CreatePlacementTexture(placementArea, 1);
        placementAreaObj.GetComponent<Renderer>().material = placementAreaMaterial;
    }

    public bool isCoordInPlacementArea(Vector2Int coords)
    {
        return placementArea[coords.x, coords.y] == 1;
    }

    public void SetVisualizeBoundsActive(bool active)
    {
        placementAreaObj.SetActive(active);
    }

    private bool needToPlaceNexus()
    {
        return nexus == null;
    }

    public void SetNexusCoords(Vector2Int coords, GameObject nexusObj)
    {
        Debug.Log($"Setting nexus coords to {coords} | obj: {nexusObj.name}");
        nexusCoords = coords;
        this.nexus = nexusObj;
    }

    public Vector3 GetNexusPosition()
    {
        return Utility.Instance.GetPositionFromTileCoords(nexusCoords, true);
    }

    public Vector2Int GetNexusCoords()
    {
        return nexusCoords;
    }

    public void SetPlacementArea(int boundsAdd, int boundsSet = -1)
    {
        if (boundsSet != -1)
        {
            placementAreaRadius = boundsSet;
        }
        else
        {
            placementAreaRadius += boundsAdd;
        }

        InitializeBoundsVisualize();
    }


    private void DieIGuess()
    {
        //Debug.Log("Im dead oops");
        //TODO - die?
    }

    private List<Building_ControlledTower> GetControlledBuildings()
    {
        return controlledTowers;
    }

    public List<Building_UnitTower> GetUnitBuildings()
    {
        return unitTowers;
    }

    public List<Building> GetAllBuildings()
    {
        return buildings;
    }

    public List<Building> GetBuildingsOfType(TowerType type)
    {
        List<Building> t = new List<Building>();
        foreach (Building building in buildings)
        {
            if (building.towerType == type)
            {
                t.Add(building);
            }
        }

        return t;
    }

    #endregion
    //===========================================================================
    // deal with input



    public void DidClick(bool leftClick)
    {
        bool isOverUI = IsPointerOverUIElement();
        if ((playState == PlayState.day || playState == PlayState.start) && !isOverUI)
        {
            if (leftClick)
            {
                if (!isPlacingBuilding)
                {
                    CheckClickBuilding();
                    
                    selectionController.StartDraw(CheckClickUnit());
                }
                placementController.DidLeftClick();
            }
            //right click
            else
            {
                placementController.DidRightClick();
                StopPipeBuild();
                selectionController.MoveSelectedUnits(Utility.Instance.GetWorldPositionFromMousePosition(), false);
                uiManager.CloseBuildingInfoUI();
            }
        }
        else if (playState == PlayState.night)
        {
            if (leftClick)
            {
                if (!isPlacingBuilding)
                {
                    CheckClickBuilding();

                    selectionController.StartDraw(CheckClickUnit());
                }

                ClearSelected();
                placementController.DidLeftClick();
            }

            //right click
            else
            {

                if (selectedControlledBuilding != null)
                {
                    UseSelectedTower();
                }
                placementController.DidRightClick();

                selectionController.MoveSelectedUnits(Utility.Instance.GetWorldPositionFromMousePosition(), true);
                uiManager.CloseBuildingInfoUI();
            }
        }
    }

    public void DidStopClick(bool leftClick)
    {
        if (playState == PlayState.night || playState == PlayState.day)
        {
            if (leftClick)
            {
                selectionController.EndDraw();
            }
        }
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == Constants.LAYER_UI)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    private List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    // =============================================================================


    public void ActivateDebug()
    {
        debug = !debug;
        if (!debug)
        {
            UIManager.Instance.SetCursorTextAndPosition("", Vector3.zero);
        }
    }

    public bool isDebug()
    {
        return debug;
    }
}
