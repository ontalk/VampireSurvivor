using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class TerrainController : MonoBehaviour
{
    [SerializeField]
    private GameObject terrainTilePrefab = null;
    [SerializeField]
    private Vector3 terrainSize = new Vector3(20, 1, 20);
    public Vector3 TerrainSize { get { return terrainSize; } }
    [SerializeField]
    private Gradient gradient;
    [SerializeField]
    private float noiseScale = 3, cellSize = 1;
    [SerializeField]
    private int radiusToRender = 5;
    [SerializeField]
    private Transform[] gameTransforms;
    [SerializeField]
    private Transform playerTransform;
    [SerializeField]
    private Transform water;
    public Transform Water { get { return water; } }
    [SerializeField]
    private int seed;
    [SerializeField]
    private GameObject[] placeableObjects;
    public GameObject[] PlaceableObjects { get { return placeableObjects; } }
    [SerializeField]
    private Vector3[] placeableObjectSizes;
    public Vector3[] PlaceableObjectSizes { get { return placeableObjectSizes; } }
    [SerializeField]
    private int minObjectsPerTile = 0, maxObjectsPerTile = 20;
    public int MinObjectsPerTile { get { return minObjectsPerTile; } }
    public int MaxObjectsPerTile { get { return maxObjectsPerTile; } }
    [SerializeField]
    private float destroyDistance = 1000;
    [SerializeField]
    private bool usePerlinNoise = true;
    [SerializeField]
    private Texture2D noise;
    public static float[][] noisePixels;

    private Vector2 startOffset;
    private Dictionary<Vector2, GameObject> terrainTiles = new Dictionary<Vector2, GameObject>();
    private Vector2[] previousCenterTiles;
    private List<GameObject> previousTileObjects = new List<GameObject>();
    public Transform Level { get; set; }
    private Vector2 noiseRange;
    private NavMeshSurface navMeshSurface;

    private void Awake()
    {
        if (noise)
            noisePixels = GetGrayScalePixels(noise);
        GenerateMesh.UsePerlinNoise = usePerlinNoise;
        noiseRange = usePerlinNoise ? Vector2.one * 256 : new Vector2(noisePixels.Length, noisePixels[0].Length);
    }

    private void Start()
    {
        InitialLoad();
    }

    public void InitialLoad()
    {
        DestroyTerrain();

        Level = new GameObject("Level").transform;
        water.parent = Level;
        playerTransform.parent = Level;
        foreach (Transform t in gameTransforms)
            t.parent = Level;

        float waterSideLength = radiusToRender * 2 + 1;
        water.localScale = new Vector3(terrainSize.x / 10 * waterSideLength, 1, terrainSize.z / 10 * waterSideLength);

        Random.InitState(seed);
        startOffset = new Vector2(Random.Range(0f, noiseRange.x), Random.Range(0f, noiseRange.y));
        RandomizeInitState();

        // Initialize and configure NavMeshSurface
        GameObject navMeshGameObject = new GameObject("NavMeshSurface");
        navMeshGameObject.transform.parent = Level;
        navMeshSurface = navMeshGameObject.AddComponent<NavMeshSurface>();
        navMeshSurface.collectObjects = CollectObjects.Children;

        // Generate initial tiles and bake NavMesh
        GenerateInitialTiles();
        navMeshSurface.BuildNavMesh();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        Vector2 playerTile = TileFromPosition(playerTransform.localPosition);
        List<Vector2> centerTiles = new List<Vector2> { playerTile };
        foreach (Transform t in gameTransforms)
            centerTiles.Add(TileFromPosition(t.localPosition));

        if (previousCenterTiles == null || HaveTilesChanged(centerTiles))
        {
            List<GameObject> tileObjects = new List<GameObject>();
            foreach (Vector2 tile in centerTiles)
            {
                bool isPlayerTile = tile == playerTile;
                int radius = isPlayerTile ? radiusToRender : 1;
                for (int i = -radius; i <= radius; i++)
                    for (int j = -radius; j <= radius; j++)
                        ActivateOrCreateTile((int)tile.x + i, (int)tile.y + j, tileObjects);
                if (isPlayerTile)
                    water.localPosition = new Vector3(tile.x * terrainSize.x, water.localPosition.y, tile.y * terrainSize.z);
            }

            foreach (GameObject g in previousTileObjects)
                if (!tileObjects.Contains(g))
                    g.SetActive(false);

            List<Vector2> keysToRemove = new List<Vector2>();
            foreach (KeyValuePair<Vector2, GameObject> kv in terrainTiles)
            {
                if (Vector3.Distance(playerTransform.position, kv.Value.transform.position) > destroyDistance && !kv.Value.activeSelf)
                {
                    keysToRemove.Add(kv.Key);
                    Destroy(kv.Value);
                }
            }
            foreach (Vector2 key in keysToRemove)
                terrainTiles.Remove(key);

            previousTileObjects = new List<GameObject>(tileObjects);

            if (navMeshSurface != null)
            {
                navMeshSurface.BuildNavMesh();
            }
        }

        previousCenterTiles = centerTiles.ToArray();
    }

    private void ActivateOrCreateTile(int xIndex, int yIndex, List<GameObject> tileObjects)
    {
        Vector2 tileKey = new Vector2(xIndex, yIndex);

        if (!terrainTiles.ContainsKey(tileKey))
        {
            tileObjects.Add(CreateTile(xIndex, yIndex));
        }
        else
        {
            GameObject t = terrainTiles[tileKey];
            tileObjects.Add(t);
            if (!t.activeSelf)
                t.SetActive(true);
        }
    }

    private GameObject CreateTile(int xIndex, int yIndex)
    {
        GameObject terrain = Instantiate(
            terrainTilePrefab,
            Vector3.zero,
            Quaternion.identity,
            Level
        );
        terrain.transform.localPosition = new Vector3(terrainSize.x * xIndex, terrainSize.y, terrainSize.z * yIndex);
        terrain.name = TrimEnd(terrain.name, "(Clone)") + " [" + xIndex + " , " + yIndex + "]";

        terrainTiles.Add(new Vector2(xIndex, yIndex), terrain);

        GenerateMesh gm = terrain.GetComponent<GenerateMesh>();
        gm.TerrainSize = terrainSize;
        gm.Gradient = gradient;
        gm.NoiseScale = noiseScale;
        gm.CellSize = cellSize;
        gm.NoiseOffset = NoiseOffset(xIndex, yIndex);
        gm.Generate();

        Random.InitState((int)(seed + (long)xIndex * 100 + yIndex));
        PlaceObjects po = gm.GetComponent<PlaceObjects>();
        po.TerrainController = this;
        po.Place();
        RandomizeInitState();

        return terrain;
    }

    private Vector2 NoiseOffset(int xIndex, int yIndex)
    {
        Vector2 noiseOffset = new Vector2(
            (xIndex * noiseScale + startOffset.x) % noiseRange.x,
            (yIndex * noiseScale + startOffset.y) % noiseRange.y
        );
        if (noiseOffset.x < 0)
            noiseOffset = new Vector2(noiseOffset.x + noiseRange.x, noiseOffset.y);
        if (noiseOffset.y < 0)
            noiseOffset = new Vector2(noiseOffset.x, noiseOffset.y + noiseRange.y);
        return noiseOffset;
    }

    private Vector2 TileFromPosition(Vector3 position)
    {
        return new Vector2(Mathf.FloorToInt(position.x / terrainSize.x + .5f), Mathf.FloorToInt(position.z / terrainSize.z + .5f));
    }

    private void RandomizeInitState()
    {
        Random.InitState((int)System.DateTime.UtcNow.Ticks);
    }

    private bool HaveTilesChanged(List<Vector2> centerTiles)
    {
        if (previousCenterTiles == null || previousCenterTiles.Length != centerTiles.Count)
            return true;
        for (int i = 0; i < previousCenterTiles.Length; i++)
            if (previousCenterTiles[i] != centerTiles[i])
                return true;
        return false;
    }

    public void DestroyTerrain()
    {
        if (water != null) water.parent = null;
        if (playerTransform != null) playerTransform.parent = null;
        foreach (Transform t in gameTransforms)
            t.parent = Level;
        if (Level != null)
            Destroy(Level.gameObject);
        terrainTiles.Clear();
    }

    private static string TrimEnd(string str, string end)
    {
        if (str.EndsWith(end))
            return str.Substring(0, str.LastIndexOf(end));
        return str;
    }

    public static float[][] GetGrayScalePixels(Texture2D texture2D)
    {
        List<float> grayscale = texture2D.GetPixels().Select(c => c.grayscale).ToList();

        List<List<float>> grayscale2d = new List<List<float>>();
        for (int i = 0; i < grayscale.Count; i += texture2D.width)
            grayscale2d.Add(grayscale.GetRange(i, texture2D.width));

        return grayscale2d.Select(a => a.ToArray()).ToArray();
    }

    private void GenerateInitialTiles()
    {
        Vector2 playerTile = TileFromPosition(playerTransform.localPosition);
        List<Vector2> centerTiles = new List<Vector2> { playerTile };

        foreach (Transform t in gameTransforms)
        {
            centerTiles.Add(TileFromPosition(t.localPosition));
        }

        foreach (Vector2 tile in centerTiles)
        {
            bool isPlayerTile = tile == playerTile;
            int radius = isPlayerTile ? radiusToRender : 1;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    ActivateOrCreateTile((int)tile.x + i, (int)tile.y + j, new List<GameObject>()); 
                }
            }
        }
        navMeshSurface.BuildNavMesh();
    }
}
