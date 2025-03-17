using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{

    // ===== Scene GameObjects

    public const string GAMEOBJECT_LEVELMANAGER = "LevelManager";
    public const string GAMEOBJECT_LEVELMAP = "LevelMap";
    public const string GAMEOBJECT_LEVELCLIFFMAP= "CliffMap";
    public const string GAMEOBJECT_AIMANAGER = "AIManager";
    public const string GAMEOBJECT_PLAYERCONTROLLER = "PlayerController";
    public const string GAMEOBJECT_GAMEMANAGER = "GameManager";
    public const string GAMEOBJECT_PLACEMENTMAP = "PlacementMap";
    public const string GAMEOBJECT_UI = "UI";
    public const string GAMEOBJECT_SUN = "THE SUN";
    public const string GAMEOBJECT_RESEARCHUI = "ResearchUI";

    // ======== obj names

    public const string NEXUS_NAME = "nexusBuilding";

    // ===== level constants
    public const int LEVEL_SIZE = 100;

    public const int LEVEL_TILE_WIDTH = 1;
    public const int LEVEL_TILE_HEIGHT = 1;


    // ===== Tags
    public const string TAG_ENEMY = "Enemy";
    public const string TAG_BUILDING_HOUSE = "Building_Housing";
    public const string TAG_BUILDING_ELEMENT_PILLAR = "Element_Pillar";
    public const string TAG_BUILDING_FARM = "Building_Farm";
    public const string TAG_BUILDING = "Building";
    public const string TAG_PERSON = "Person";
    public const string TAG_UI = "UI";

    // ===== Layers

    public const int LAYER_UI = 5;

    // ===== terrain constants

    public const float TERRAIN_HEIGHT_TIER_OCEAN = 0.0f;
    public const float TERRAIN_HEIGHT_TIER_1 = 0.0f;
    public const float TERRAIN_HEIGHT_TIER_2 = 0.5f;
    public const float TERRAIN_HEIGHT_TIER_3 = 1.0f;
    public const float TERRAIN_HEIGHT_TIER_4 = 1.5f;

    // colors
    public Color TERRAIN_COLOR_WATER = new Color(0.247f, 0.698f, 0.87f, 1.0f);
    public Color TERRAIN_COLOR_GRASS = new Color(0.047f, 0.584f, 0.054f, 1.0f);
    public Color TERRAIN_COLOR_DEAD_GRASS = new Color32(111, 156, 14, 255);
    public Color TERRAIN_COLOR_SAND = new Color(0.831f, 0.85f, 0.462f, 1.0f);
    public Color TERRAIN_COLOR_ROCK = new Color(0.5f, 0.5f, 0.5f, 1.0f);

    // ===== Camera

    public const float CAMERA_SPEED_UP_TIME = 1.0f;
    public const float CAMERA_MOUSE_FULL_ROTATION_AMOUNT = 500;

    // ===== AI constnats

    //movement
    public const float AI_BASE_ACCELERATION = 0.3f; // this should be ~1/10th max speed
    public const float AI_BASE_MAX_SPEED = 1.0f;
    public const float AI_SEEK_CLOSE_DISTANCE = 0.2f;
    public const float AI_SEEK_ARRIVED_DISTANCE = 0.15f;
    public const float AI_UNIT_FOLLOW_ARRIVED_DISTANCE = 0.05f;
    public const float AI_MINIMUM_MOVEMENT_VALUE = 0.01f;
    public const float AI_START_JOB_ARRIVED_DISTANCE = 1.5f;
    public const float AI_REACHED_HOME_DISTANCE = 0.5f;

    //tree cutting / resource collecting
    public const float AI_TIME_RESOURCE_COLLECT_WOOD = 2f;
    public const float AI_TREE_ARRIVED_JOB_DISTANCE = 1f;
    public const float AI_TREE_ARRIVED_TREE_DISTANCE = 0.02f;
    public const string AI_TREE_COLLECTION_POINTS_GAMEOBJECT = "CollectionPoints";

    public const float AI_TIME_RESOURCE_COLLECT_STONE = 2f;

    //holding stuff
    public const int AI_MAX_HOLD_COUNT = 4;



    // ===== Unit AI

    public const float AI_UNIT_NEAR_ENEMY = 2f;
    public const float AI_UNIT_AT_ENEMY = 0.2f;
    public const float AI_UNIT_ATTACK_ENEMY_TIME = 1f;

    // ===== Debug Stuff

    public const float DEBUG_AI_MOVE_VECTOR_SPHERE_RADIUS = 0.02f;
    public const float DEBUG_AI_WANDER_SEEK_POINT_RADIUS = 0.04f;

    // ===== Resource Loading

    public const string RESOURCES_PREFAB_SELECTION_CUBE = "Prefabs/SelectionCube";

    // ===== Resource Random Stuff idk

    public const string RESOURCE_WOOD = "wood";
    public const string RESOURCE_STONE = "stone";
    public const string RESOURCE_WORK = "work";
    public const string RESOURCE_RESEARCH = "research";
    public const string RESOURCE_FARM = "farm";

    // ===== Wave stuff

    public const float WAVE_MANAGER_SPAWN_POINTS = 1.4f;
    public const int WAVE_MANAGER_STARTING_POINTS = 2;
    public const float WAVE_MANAGER_STARTING_POITNS_MULT = 1.5f;


    // ===== Enemy Stuff

    public const float ENEMY_NEAR_NEXUS_DISTANCE = 0.7f;
    public const float ENEMY_HIT_NEXUS_COOLDOWN = 1f;
    public const float ENEMY_HIT_UNIT_COOLDOWN = 1f;
    public const float ENEMY_NEAR_UNIT_DISTANCE = 2f;
    public const float ENEMY_HIT_ENEMY_DISTANCE = 0.2f;
    public const float ENEMY_ON_FIRE_HIT_TIME = 1f;
    public const float ENEMY_IS_SLOW_AMOUNT = 0.6f;
    public const float ENEMY_MUD_SLOW_TIME = 5f;

    // ===== Tower Stuff

    public const float PROJECTILE_HIT_DISTANCE = 0.5f;
    public const float PROJECTILE_GROUND_LIFETIME = 4f;
    public const float PROJECTILE_ELECTRIC_DEFAULT_RANGE = 10f;
    public const float PROJECTILE_DESTROY_HEIGHT = -0.05f;
    public const float PROJECTILE_HOMING_MULTIPLIER = 1f;
    public const float PROJECTILE_HOMING_RADIUS = 1f;
    public const float PROJECTILE_DESTROY_TIME = 30f;

    public static Vector3 PROJECTILE_GRAVITY_FORCE = new Vector3(0, -5, 0);

    // ===== Resource Amounts

    public const int STONE_DEFAULT_MINE_AMOUNT = 100;

    public const int NEXUS_GATHER_JOBS = 4;

    // ===== Day cycle stuff?

    public const float DAY_END_TIME = 45f;
    public const float DAY_END_WAIT_FOR_JOBS = 0f;

    // ===== farm stuff

    public const float FARM_GROW_TIME = 10f; //how many seconds does it take for the farm to grow
    public const float FARM_WANDER_RADIUS = 0.5f;
    public const int FARM_RESOURCE_SPAWN = 5; // number of resources to spawn for the farm


    // ===== UI

    public const int UI_INFO_MAX_MESSAGES = 100;

    public const float UI_WORKER_UI_SPACING = 50;

    // ===== Mana

    public const int MANA_ADD_PERSON_AMOUNT = 5;
    public const int MANA_NEXUS_START = 25;

    // ===== IDK / Other

    public const float START_DRAW_DELAY_TIME = 0.1f;

    public const float SPHERE_CAST_SELECT_RADIUS = 1.0f;
}
