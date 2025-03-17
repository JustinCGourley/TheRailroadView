using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType : int
{
    water = 0,
    sand = 1,
    dead_grass = 2,
    grass = 3,
    rock = 4
};

public enum ObjectType : int
{
    none = 0,
    tree = 1,
    rock = 2,
    metal = 3,
    building = 4,
    nexus = 5,
    pipe = 6
};

public struct LevelData
{
    public float height;
    public Vector2Int position;
    public GameObject obj;
    public ObjectType objType;
    public TerrainType terrainType;

    public override string ToString()
    {
        return $"@{position.x}, {position.y} - | ObjType: {objType} |";
    }
}

public class LevelGeneration : MonoBehaviour
{
    private MeshBuilder meshBuilder;
    private TextureCreator textureCreator;
    private LevelManager levelManager;

    private GameObject levelObj;
    private GameObject levelCliffObj;

    private LevelData[,] levelMap;

    public float noiseScale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;


    public Transform resourceObjectTransform;

    //environment objects
    public List<GameObject> treeObj;
    public List<GameObject> stoneObj;


    public float planeWidth;
    public float planeHeight;

    public bool isWaterGenerated;

    // Start is called before the first frame update
    private void Start()
    {
        meshBuilder = new MeshBuilder();
        textureCreator = new TextureCreator();
        levelManager = LevelManager.Instance;

        levelObj = GameObject.Find(Constants.GAMEOBJECT_LEVELMAP);
        levelCliffObj = GameObject.Find(Constants.GAMEOBJECT_LEVELCLIFFMAP);

        levelMap = new LevelData[Constants.LEVEL_SIZE, Constants.LEVEL_SIZE];

        float[,] noiseMap = GenerateNoiseMap(Constants.LEVEL_SIZE, Constants.LEVEL_SIZE, seed, noiseScale, octaves, persistance, lacunarity, offset);
        float[,] objectNoiseMap = GenerateNoiseMap(Constants.LEVEL_SIZE, Constants.LEVEL_SIZE, (seed/2)+2, noiseScale, octaves, persistance, lacunarity, offset);
               
        List<GameObject> mainMeshObjs = new List<GameObject>();
        List<GameObject> cliffMeshObjs = new List<GameObject>();
        List<GameObject> treeMeshObjs = new List<GameObject>();

        for (int x = 0; x < levelMap.GetLength(0); x++)
        {
            for (int z = 0; z < levelMap.GetLength(1); z++)
            {
                SetupMap(x,z);
                GenerateTerrainTypeAndHeight(x,z,noiseMap[x,z]);
                GameObject treeObj = GenerateNaturalObjects(x,z,objectNoiseMap[x,z]);
                if (treeObj != null) { treeMeshObjs.Add(treeObj); }
                mainMeshObjs.Add(GenerateMesh(x,z));
            }
        }

        //generates cliffs (turning off for now)
        //for (int x = 0; x < levelMap.GetLength(0); x++)
        //{
        //    for (int z = 0; z < levelMap.GetLength(1); z++)
        //    {
        //        List<GameObject> cliffObjs = GenerateCliffMesh(x, z);
        //        cliffMeshObjs = Utility.Instance.CombineLists(cliffMeshObjs, cliffObjs);
        //    }
        //}

        Utility.Instance.CombineMesh(mainMeshObjs, levelObj.transform);
        levelObj.GetComponent<Renderer>().sharedMaterial.mainTexture = textureCreator.CreateTerrainTexture(levelMap, 3);
        Utility.Instance.CombineMesh(cliffMeshObjs, levelCliffObj.transform);

        levelObj.AddComponent<MeshCollider>();
        levelCliffObj.AddComponent<MeshCollider>();

        levelManager.SetLevel(levelMap, planeWidth, planeHeight);
        TreeManager.Instance.InitialSetup(treeMeshObjs);

        //add box collider
        levelObj.AddComponent<BoxCollider>();
    }

    /// <summary>
    /// Sets up levelMap
    /// </summary>
    private void SetupMap(int x, int z)
    {
        LevelData data = new LevelData();
        data.height = 0;
        data.position = new Vector2Int(x, z);
        data.obj = null;
        data.objType = ObjectType.none;
        levelMap[x, z] = data;
    }

    /// <summary>
    /// Generates heightmap to decide tiers of the tiles
    /// </summary>
    /// <param name="noiseMap"></param>
    private void GenerateTerrainTypeAndHeight(int x, int z, float height)
    {
        System.Random rng = new System.Random(seed + ((x + 1) * (x + 1)) * ((z + 1) * (z + 1)) * ((x * z) + 2));

        int rand = rng.Next(0, 100);

        //tier 0
        if (height <= 0.3f)
        {
            if (!isWaterGenerated)
            {
                levelMap[x, z].terrainType = (height <= .15f) ? TerrainType.grass : TerrainType.dead_grass;
            }
            else
            {
                levelMap[x, z].terrainType = TerrainType.water;
            }
            height = Constants.TERRAIN_HEIGHT_TIER_OCEAN;
        }
        else if (height > 0.3f && height < 0.7f)
        {
            height = Constants.TERRAIN_HEIGHT_TIER_1;
            if (rand <= 75)
            {
                levelMap[x, z].terrainType = TerrainType.grass;
            }
            else if (rand <= 98)
            {
                levelMap[x, z].terrainType = TerrainType.dead_grass;
            }
            else
            {
                levelMap[x, z].terrainType = TerrainType.sand;
            }
        }
        //tier 2
        else if (height >= 0.7f)
        {
            height = Constants.TERRAIN_HEIGHT_TIER_2;
            if (rand <= 25)
            {
                levelMap[x, z].terrainType = TerrainType.grass;
            }
            else if (rand <= 80)
            {
                levelMap[x, z].terrainType = TerrainType.dead_grass;
            }
            else
            {
                levelMap[x, z].terrainType = TerrainType.rock;
            }
        }

        //ADD THIS LINE TO MAKE THE CLIFFS ACTUALLY WORK :)
        //levelMap[x, z].height = height;
    }

    private GameObject GenerateNaturalObjects(int x, int z, float noise)
    {
        System.Random rng = new System.Random(seed + ((x + 1) * (x + 1)) * ((z + 1) * (z + 1)) * ((x * z) + 2));
        
        if ((levelMap[x,z].terrainType == TerrainType.grass && (noise >=0.4f && noise <= 0.6f)) ||
            (levelMap[x, z].terrainType == TerrainType.dead_grass && (noise >= 0.5f && noise <= 0.55f)))
        {
            int rand = rng.Next(0, treeObj.Count);
            //Debug.Log("Random tree! - " + rand);
            levelMap[x, z].objType = ObjectType.tree;
            return Instantiate(
                treeObj[rand],
                new Vector3(x + ((Constants.LEVEL_TILE_WIDTH + 0f) / 2), levelMap[x, z].height, z + ((Constants.LEVEL_TILE_HEIGHT + 0f) / 2)),
                Quaternion.Euler(0, Random.Range(0f, 360f), 0),
                resourceObjectTransform);
        }
        else if (levelMap[x, z].terrainType == TerrainType.rock && (noise >= 0.55f && noise <= 0.7f))
        {
            //TODO: FIX THE FUCKING RANDOM SEED BULLSHIT
            int rand = Random.Range(0, stoneObj.Count);

            //Debug.Log("Random Rock! - " + rand);

            levelMap[x, z].objType = ObjectType.rock;
            levelMap[x, z].obj = Instantiate(
                stoneObj[rand],
                new Vector3(x + ((Constants.LEVEL_TILE_WIDTH + 0f) / 2), levelMap[x, z].height, z + ((Constants.LEVEL_TILE_HEIGHT + 0f) / 2)),
                Quaternion.Euler(0, Random.Range(0f, 360f), 0),
                resourceObjectTransform);
        }

        return null;
    }

    /// <summary>
    /// generates main mesh for all tiles of the map (not including cliff faces)
    /// </summary>
    private GameObject GenerateMesh(int x, int z)
    {

        float uvX = (x + 0.0f) / (levelMap.GetLength(0) + 0.0f);
        float uvY = (z + 0.0f) / (levelMap.GetLength(1) + 0.0f);
        float uvSize = (planeWidth + 0.0f) / (levelMap.GetLength(0) + 0.0f);
        GameObject newObj = meshBuilder.BuildTerrainPlane(planeWidth, planeHeight, uvX, uvY, uvSize);
    
        newObj.transform.parent = levelObj.transform;

        LevelData data = levelMap[x, z];
        newObj.transform.position = new Vector3(data.position.x, data.height, data.position.y);

        return newObj;
    }

    private List<GameObject> GenerateCliffMesh(int x, int z)
    {
        List<GameObject> objs = new List<GameObject>();

        //check left
        if (x != 0)
        {
            if (levelMap[x, z].height > levelMap[x - 1, z].height)
            {
                objs.Add(meshBuilder.BuildCliffPlane(planeWidth, Mathf.Abs(levelMap[x, z].height - levelMap[x - 1, z].height)));
                objs[objs.Count - 1].transform.parent = levelCliffObj.transform;
                objs[objs.Count - 1].transform.Rotate(new Vector3(0, 90, 0));
                objs[objs.Count - 1].transform.position = new Vector3(levelMap[x, z].position.x, levelMap[x - 1, z].height, levelMap[x, z].position.y + planeWidth);
            }
        }
        else if (levelMap[x, z].height != 0.0f)
        {
            objs.Add(meshBuilder.BuildCliffPlane(planeWidth, levelMap[x, z].height));
            objs[objs.Count - 1].transform.parent = levelCliffObj.transform;
            objs[objs.Count - 1].transform.Rotate(new Vector3(0, 90, 0));
            objs[objs.Count - 1].transform.position = new Vector3(levelMap[x, z].position.x, 0, levelMap[x, z].position.y + planeWidth);
        }

        //check right
        if (x != levelMap.GetLength(0) - 1)
        {
            if (levelMap[x, z].height > levelMap[x + 1, z].height)
            {
                objs.Add(meshBuilder.BuildCliffPlane(planeWidth, Mathf.Abs(levelMap[x, z].height - levelMap[x + 1, z].height)));
                objs[objs.Count - 1].transform.parent = levelCliffObj.transform;
                objs[objs.Count - 1].transform.Rotate(new Vector3(0, 270, 0));
                objs[objs.Count - 1].transform.position = new Vector3(levelMap[x, z].position.x + planeWidth, levelMap[x + 1, z].height, levelMap[x, z].position.y);
            }
        }
        else if (levelMap[x, z].height != 0.0f)
        {
            objs.Add(meshBuilder.BuildCliffPlane(planeWidth, levelMap[x, z].height));
            objs[objs.Count - 1].transform.parent = levelCliffObj.transform;
            objs[objs.Count - 1].transform.Rotate(new Vector3(0, 270, 0));
            objs[objs.Count - 1].transform.position = new Vector3(levelMap[x, z].position.x + planeWidth, 0, levelMap[x, z].position.y);
        }

        //check up
        if (z != levelMap.GetLength(1) - 1)
        {
            if (levelMap[x, z].height > levelMap[x, z + 1].height)
            {
                objs.Add(meshBuilder.BuildCliffPlane(planeWidth, Mathf.Abs(levelMap[x, z].height - levelMap[x, z + 1].height)));
                objs[objs.Count - 1].transform.parent = levelCliffObj.transform;
                objs[objs.Count - 1].transform.Rotate(new Vector3(0, 180, 0));
                objs[objs.Count - 1].transform.position = new Vector3(levelMap[x, z].position.x + planeWidth, levelMap[x, z + 1].height, levelMap[x, z].position.y + planeWidth);
            }
        }
        else if (levelMap[x, z].height != 0.0f)
        {
            objs.Add(meshBuilder.BuildCliffPlane(planeWidth, levelMap[x, z].height));
            objs[objs.Count - 1].transform.parent = levelCliffObj.transform;
            objs[objs.Count - 1].transform.Rotate(new Vector3(0, 180, 0));
            objs[objs.Count - 1].transform.position = new Vector3(levelMap[x, z].position.x + planeWidth, 0, levelMap[x, z].position.y + planeWidth);
        }

        //check down
        if (z != 0)
        {
            if (levelMap[x, z].height > levelMap[x, z - 1].height)
            {
                objs.Add(meshBuilder.BuildCliffPlane(planeWidth, Mathf.Abs(levelMap[x, z].height - levelMap[x, z - 1].height)));
                objs[objs.Count - 1].transform.parent = levelCliffObj.transform;
                objs[objs.Count - 1].transform.position = new Vector3(levelMap[x, z].position.x, levelMap[x, z - 1].height, levelMap[x, z].position.y);
            }
        }
        else if (levelMap[x, z].height != 0.0f)
        {
            objs.Add(meshBuilder.BuildCliffPlane(planeWidth, levelMap[x, z].height));
            objs[objs.Count - 1].transform.parent = levelCliffObj.transform;
            objs[objs.Count - 1].transform.position = new Vector3(levelMap[x, z].position.x, 0, levelMap[x, z].position.y);
        }

        return objs;
    }

    /// <summary>
    /// Generates a noise map using [octives] layers of perlin noise
    /// </summary>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    /// <param name="seed"></param>
    /// <param name="scale"></param>
    /// <param name="octaves"></param>
    /// <param name="persistance"></param>
    /// <param name="lacunarity"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

}
