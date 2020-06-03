using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyArena : MonoBehaviour
{
    public static DummyArena _instance;
    [Header("World generation rules")]
    public int WorldSize;
    public float NoiseScale;
    public List<GameObject> Blocks;
    public GameObject Player_0;
    public GameObject Player_1;
    public List<Color> TeamColors = new List<Color>();
    public GameObject Wall;
    public GameObject Box;
    public int MaxBoxes;
    public GameObject Bomb;
    public int MaxBombs;
    public UnityEngine.Random randomGen = new UnityEngine.Random();

    [Header("Settings")]
    public LayerMask PlayersCantSpawnOn;
    [HideInInspector]
    public List<GameObject> PlayerDummies = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> BoxDummies = new List<GameObject>();

    public List<BombDummy> BombDummies = new List<BombDummy>();
    public List<DummyExplosion> ExplosionDummies = new List<DummyExplosion>();
    public List<GameObject> PickUpDummies = new List<GameObject>();

    //Level
    [System.NonSerialized]
    public GameObject[][] Tiles;
    private GameObject level;
    public void Awake()
    {
        _instance = this;
    }
  
    [ContextMenu("Clear")]
    public void Clear()
    {
        Destroy(level);
        level = new GameObject("Level");
        level.transform.parent = transform;
        level.transform.localPosition = Vector3.zero;
    }
    [ContextMenu("Generate")]
    public void Generate()
    {
        Tiles = new GameObject[WorldSize][];
        for (int i = 0; i < WorldSize; i++)
            Tiles[i] = new GameObject[WorldSize];

        Clear();
        for (int x = 0; x < WorldSize; x++)
            for (int y = 0; y < WorldSize; y++)
                PlaceBlock(x, y, (float)WorldSize);

        //Boxes
        BoxDummies = PlaceObjects(Box, (int)(Random.value * MaxBoxes));

        //Bombs
        PlaceObjects(Bomb, (int)(Random.value * MaxBombs));

        GameObject player1 = PlaceObjects(Player_0, 1)[0];
        player1.GetComponentInChildren<MeshRenderer>().material.color = TeamColors[0];
        PlayerDummies.Add(player1);
        GameObject player2 = PlaceObjects(Player_1, 1)[0];
        player2.GetComponentInChildren<MeshRenderer>().material.color = TeamColors[1];
        PlayerDummies.Add(player2);
    }

    public void Generate(int seed)
    {
        Random.InitState(seed);
        Generate();
    }
    public void PlaceBlock(int x, int y, float worldSize)
    {
        float block = Mathf.Clamp01(Mathf.PerlinNoise((float)x / worldSize * NoiseScale + Random.value, (float)y / worldSize * NoiseScale + Random.value)) * 0.999f * Blocks.Count;
        bool Border = x == 0 || y == 0 || x == WorldSize - 1 || y == WorldSize - 1;
        Tiles[x][y] = Instantiate(Border ? Wall : Blocks[Mathf.FloorToInt(block)], new Vector3(x, 0, y) + transform.position, Quaternion.identity, level.transform);
    }

    public List<GameObject> PlaceObjects(GameObject obj, int amount)
    {
        List<GameObject> objs = new List<GameObject>();
        for (int i = 0; i < amount; i++)
        {
            //find unoccupied
            Vector3 placement = new Vector3(Mathf.FloorToInt(Random.value * WorldSize), 0f, Mathf.FloorToInt(Random.value * WorldSize));
            while (Physics.CheckBox(placement + transform.position, Vector3.one / 2.1f, Quaternion.identity, PlayersCantSpawnOn))
                placement = new Vector3(Mathf.FloorToInt(Random.value * WorldSize), 0f, Mathf.FloorToInt(Random.value * WorldSize));

            GameObject objinz = Instantiate(obj, level.transform);
            objinz.transform.localPosition = placement;
            objs.Add(objinz);
        }
        return objs;
    }
}
