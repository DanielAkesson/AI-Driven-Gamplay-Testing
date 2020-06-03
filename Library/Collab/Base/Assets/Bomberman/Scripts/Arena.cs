using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Arena : MonoBehaviour
{
    public static Arena _instance;
    [Header("World generation rules")]
    public int WorldSize;
    public float NoiseScale;
    public List<GameObject> Blocks;
    public GameObject Player_0;
    public GameObject Player_1;
    public List<Color> TeamColors = new List<Color>();
    public GameObject Wall;
    public GameObject Box;
    public GameObject BugBox;
    public int MaxBoxes;
    public GameObject Bomb;
    public int MaxBombs;
    public UnityEngine.Random randomGen = new UnityEngine.Random();

    [Header("Game rules")]
    [SerializeField]
    private float TimeStepSeconds;
    public static float TimeStep;
    public LayerMask PlayersCantSpawnOn;
    [System.NonSerialized]
    public Texture2D Minimap;
    public int MinimapTextureSize;
    [System.NonSerialized]
    public Texture2D NearMap;
    public int NearMapTextureSize;
    [Header("Optimization")]
    public bool RenderAiInputMaps;
    //Objects
    [System.NonSerialized]
    public List<GameObject> GameObjects = new List<GameObject>();
    [System.NonSerialized]
    public List<GameObject> Boxes = new List<GameObject>();
    [System.NonSerialized]
    public List<PlayerController> Players = new List<PlayerController>();
    [System.NonSerialized]
    public List<BombController> Bombs = new List<BombController>();
    [System.NonSerialized]
    public GameObject[][] Tiles;
    private GameObject level;
    public enum Mapping
    {
        Unwalkable, Freindly, Enemy, Bomb, Box, Wall, Powerup
    }

    public void RegisterGameObject(GameObject gameObject)
    {
        GameObjects.Add(gameObject);
        gameObject.transform.parent = level.transform;
        PlayerController player = gameObject.GetComponentInParent<PlayerController>();
        BombController bomb = gameObject.GetComponentInParent<BombController>();
        BoxController box = gameObject.GetComponentInParent<BoxController>();
        if (player != null)
            Players.Add(player);
        else if (bomb != null)
            Bombs.Add(bomb);
        else if (box != null)
            Boxes.Add(gameObject);
    }
    public void DestroyGameObject(GameObject gameObject)
    {
        GameObjects.Remove(gameObject);
        PlayerController player = gameObject.GetComponentInParent<PlayerController>();
        BombController bomb = gameObject.GetComponentInParent<BombController>();
        if (player != null)
            Players.Remove(player);
        else if (bomb != null)
            Bombs.Remove(bomb);
        else
            Boxes.Remove(gameObject);
        Destroy(gameObject);
    }
    public void Start()
    {
        _instance = this;
        Minimap = new Texture2D(MinimapTextureSize, MinimapTextureSize, TextureFormat.ARGB32, false, true);
        Minimap.filterMode = FilterMode.Point;
        NearMap = new Texture2D(NearMapTextureSize, NearMapTextureSize, TextureFormat.ARGB32, false, true);
        NearMap.filterMode = FilterMode.Point;

        for (int x = 0; x < MinimapTextureSize; x++)
            for (int z = 0; z < MinimapTextureSize; z++)
                Minimap.SetPixel(x, z, Color.gray);
        Minimap.Apply();

        for (int x = 0; x < NearMapTextureSize; x++)
            for (int z = 0; z < NearMapTextureSize; z++)
                NearMap.SetPixel(x, z, Color.gray);
        NearMap.Apply();

        TimeStep = TimeStepSeconds;
        Generate();
    }
    public void FixedUpdate()
    {
        if (RenderAiInputMaps)
        {
            drawCompass();
            drawNearby();
        }
        BombermanSettings.OnUpdate(this);
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        GameObjects.Clear();
        Bombs.Clear();
        Players.Clear();
        Boxes.Clear();
        //DestroyImmediate(level);
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
        List<GameObject> objs = PlaceObjects(BombermanSettings.GetEvno(this).BugColliderBoxes ? BugBox : Box, (int)(Random.value * MaxBoxes));
        foreach (GameObject ob in objs)
            ob.GetComponentInChildren<BoxController>().Arena = this;

        //Bombs
        objs = PlaceObjects(Bomb, (int)(Random.value * MaxBombs));
        foreach (GameObject ob in objs)
            ob.GetComponent<BombController>().Arena = this;

        PlayerController play0 = PlaceObjects(Player_0, 1)[0].GetComponentInChildren<PlayerController>();
        play0.Arena = this;
        play0.gameObject.GetComponentInChildren<MeshRenderer>().material.color = TeamColors[0];
        PlayerController play1 = PlaceObjects(Player_1, 1)[0].GetComponentInChildren<PlayerController>();
        play1.Arena = this;
        play1.gameObject.GetComponentInChildren<MeshRenderer>().material.color = TeamColors[1];
        //PlacePlayers();
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
            RegisterGameObject(objinz.gameObject);
            objs.Add(objinz);
        }
        return objs;
    }
    public void PlacePlayers()
    {
        for (int i = 0; i < 2; i++)
        {
            //find unoccupied
            Vector3 placement = new Vector3(Mathf.FloorToInt(Random.value * WorldSize), .5f, Mathf.FloorToInt(Random.value * WorldSize));
            while (Physics.CheckBox(placement + transform.position, Vector3.one / 2.1f, Quaternion.identity, PlayersCantSpawnOn))
            {
                placement = new Vector3(Mathf.FloorToInt(Random.value * WorldSize), .5f, Mathf.FloorToInt(Random.value * WorldSize));
            }

            PlayerController player = Instantiate(i == 0 ? Player_0 : Player_1, level.transform).GetComponent<PlayerController>();
            player.transform.localPosition = placement;
            player.Arena = this;
            player.gameObject.GetComponentInChildren<MeshRenderer>().material.color = TeamColors.Count > i ? TeamColors[i] : Color.red;
            RegisterGameObject(player.gameObject);
        }
    }

    //Minimap
    /* fix implementation if you want to use this dummy
    public int[] GetMinimap(int team, params Mapping[] types)
    {
        int CellTypeAmount = types.Length;
        int[] minimapMask = new int[WorldSize * WorldSize * CellTypeAmount];
        if (Tiles == null)
            return minimapMask;

        for (int x = 0; x < WorldSize; x++)
        {
            for (int z = 0; z < WorldSize; z++)
            {
                float[] cell = checkObject(Tiles[x][z], team);
                for (int c = 0; c < CellTypeAmount; c++)
                    minimapMask[(z * WorldSize * CellTypeAmount) + (x * CellTypeAmount) + c] = cell[c];
            }
        }

        //Add all dynamic objects
        foreach (GameObject d in GameObjects)
        {
            int x = Mathf.RoundToInt(d.transform.localPosition.x);
            int z = Mathf.RoundToInt(d.transform.localPosition.z);
            if (x < 0 || z < 0 || x >= WorldSize || z >= WorldSize)
                continue;

            int cell = (z * WorldSize * CellTypeAmount) + (x * CellTypeAmount);
            int[] cellData = checkObject(d, team);
            //Add mask
            minimapMask[cell] = cellData[0] == 0 ? 0 : minimapMask[cell];
            for (int c = 1; c < 5; c++)
                minimapMask[cell + c] = cellData[c];
        }
        return minimapMask;
    }
    */
    public float[] GetNearby(PlayerController Player, int halfExtends, params Mapping[] types)
    {
        int CellTypeAmount = types.Length;
        int Centerx = Mathf.RoundToInt(Player.transform.localPosition.x);
        int Centerz = Mathf.RoundToInt(Player.transform.localPosition.z);
        int size = halfExtends * 2 + 1;
        float[] nearMap = new float[size * size * CellTypeAmount];
        if (Tiles == null)
            return nearMap;

        //Get all objects to check
        List<GameObject> objectsToCheck = new List<GameObject>();
        objectsToCheck.AddRange(GameObjects);
        for(int i=0;i< Tiles.Length;i++)
            objectsToCheck.AddRange(Tiles[i]);

        //Add all dynamic objects
        foreach (GameObject o in objectsToCheck)
        {
            //Ignore player
            if (o == Player.gameObject)
                continue;

            int x = Mathf.RoundToInt(o.transform.localPosition.x) - Centerx + halfExtends;
            int z = Mathf.RoundToInt(o.transform.localPosition.z) - Centerz + halfExtends;
            if (x < 0 || z < 0 || x >= size || z >= size)
                continue;

            int cell = (z * size * CellTypeAmount) + (x * CellTypeAmount);
            float[] cellData = checkObject(o, Player.Team);
            //Add mask
            for (int c = 0; c < CellTypeAmount; c++)
                nearMap[cell + c] = Mathf.Max(nearMap[cell + c], cellData[(int)types[c]]);
        }
        return nearMap;
    }
    public float[] GetCompass(PlayerController Player, int halfExtends, params Mapping[] types)
    {
        int CellTypeAmount = types.Length;
        int Centerx = Mathf.RoundToInt(Player.transform.localPosition.x);
        int Centerz = Mathf.RoundToInt(Player.transform.localPosition.z);
        int NearSize = halfExtends * 2 + 1;
        int compassSideSize = halfExtends * 2 + 2;
        float[] compassMap = new float[compassSideSize * 4 * CellTypeAmount];

        //Get all objects to check
        List<GameObject> objectsToCheck = new List<GameObject>();
        objectsToCheck.AddRange(GameObjects);
        for (int i = 0; i < Tiles.Length; i++)
            objectsToCheck.AddRange(Tiles[i]);

        //Add all dynamic objects
        foreach (GameObject o in objectsToCheck)
        {
            int x = Mathf.RoundToInt(o.transform.localPosition.x) - Centerx + halfExtends;
            int z = Mathf.RoundToInt(o.transform.localPosition.z) - Centerz + halfExtends;
            //Is outside nearbymap!
            bool OutOfNearby = x < 0 || z < 0 || x >= NearSize || z >= NearSize;
            if (!OutOfNearby)
                continue;

            float[] cellData = checkObject(o, Player.Team);
            int cellIndex = 0;
            if (x >= NearSize && z >= 0)
            {
                //Right side
                int zclamp = z > NearSize ? NearSize : z;
                cellIndex = (compassSideSize * CellTypeAmount * 0) + (zclamp * CellTypeAmount);
            }
            else if (x >= 0 && z < 0)
            {
                //Down side
                int xclamp = x > NearSize ? NearSize : x;
                cellIndex = (compassSideSize * CellTypeAmount * 1) + (xclamp * CellTypeAmount);
            }
            else if (x < 0 && z < NearSize)
            {
                //Left Side
                int zclamp = z + 1 < 0 ? 0 : z + 1;
                cellIndex = (compassSideSize * CellTypeAmount * 2) + (zclamp * CellTypeAmount);
            }
            else
            {
                //Up
                int xclamp = x + 1 < 0 ? 0 : x + 1;
                cellIndex = (compassSideSize * CellTypeAmount * 3) + (xclamp * CellTypeAmount);
            }
            //Add mask
            for (int c = 0; c < CellTypeAmount; c++)
                compassMap[cellIndex + c] = Mathf.Max(compassMap[cellIndex + c], cellData[(int)types[c]]);
        }
        return compassMap;
    }
    private float[] checkObject(GameObject o, int team)
    {
        //drawMinimap();
        float[] mask = new float[10];
        mask[0] = 1;
        switch (o.layer)
        {
            case 8:
                if (o.GetComponentInParent<PlayerController>().Team == team)
                    mask[1] = 1;
                else
                    mask[2] = 1;
                break;
            case 9:
                BombController bomb = o.GetComponentInParent<BombController>();
                mask[3] = 1f - Mathf.Clamp01(bomb.CurrentFuseTime / bomb.FuseTime);
                if (!bomb.IsWalkable())
                    mask[0] = 1;
                break;
            case 10:
                mask[0] = 1;
                mask[5] = 1;
                break;
            case 11:
                mask[0] = 1;
                mask[4] = 1;
                break;
            case 15:
                mask[6] = 1;
                break;
        }
        return mask;
    }
    //simple
    /*
    private void drawMinimap()
    {
        int[] minimapdata = GetMinimap(0);
        for (int x = 0; x < WorldSize; x++)
        {
            for (int z = 0; z < WorldSize; z++)
            {
                float walkable = minimapdata[(z * WorldSize * CellTypeAmount) + (x * CellTypeAmount) + 0];
                float Friend = minimapdata[(z * WorldSize * CellTypeAmount) + (x * CellTypeAmount) + 1];
                float Enemy = minimapdata[(z * WorldSize * CellTypeAmount) + (x * CellTypeAmount) + 2];
                float Bomb = minimapdata[(z * WorldSize * CellTypeAmount) + (x * CellTypeAmount) + 3];
                float box = minimapdata[(z * WorldSize * CellTypeAmount) + (x * CellTypeAmount) + 4];

                //Choose color for things
                Color color = Color.black;
                if (walkable > 0.5f)
                    color = Color.white;
                if (box > 0.5f)
                    color = new Color(0.6f, 0f, 0.6f);
                if (Friend > 0.5f || Enemy > 0.5f || Bomb > 0.5f)
                    color = new Color(Enemy, Friend, Bomb);

                Minimap.SetPixel(x, z, color);
            }
        }
        Minimap.Apply();

        int[] nearmap = GetNearByMap(Players[0], 5);
        for (int x = 0; x < 11; x++)
        {
            for (int z = 0; z < 11; z++)
            {
                float walkable = nearmap[(z * 11 * CellTypeAmount) + (x * CellTypeAmount) + 0];
                float Friend = nearmap[(z * 11 * CellTypeAmount) + (x * CellTypeAmount) + 1];
                float Enemy = nearmap[(z * 11 * CellTypeAmount) + (x * CellTypeAmount) + 2];
                float Bomb = nearmap[(z * 11 * CellTypeAmount) + (x * CellTypeAmount) + 3];
                float box = nearmap[(z * 11 * CellTypeAmount) + (x * CellTypeAmount) + 4];

                //Choose color for things
                Color color = Color.black;
                if (walkable > 0.5f)
                    color = Color.white;
                if (box > 0.5f)
                    color = new Color(0.6f, 0f, 0.6f);
                if (Friend > 0.5f || Enemy > 0.5f || Bomb > 0.5f)
                    color = new Color(Enemy, Friend, Bomb);

                NearMap.SetPixel(x, z, color);
            }
        }
        NearMap.Apply();
    }*/
    private void drawCompass()
    {
        float[] compass = GetCompass(Players[0], 4, Mapping.Bomb, Mapping.Enemy);
        for (int i = 0; i < 6; i++)
        {
            for (int d = 0; d < 4; d++)
            {
                float bomb = compass[(6 * 2 * d) + (i * 2) + 0];
                float enemy = compass[(6 * 2 * d) + (i * 2) + 1];
                //Draw in right
                if (d == 0)
                    Minimap.SetPixel(6, i + 1, new Color(enemy, 0, bomb));
                //Draw in down
                if (d == 1)
                    Minimap.SetPixel(i + 1, 0, new Color(enemy, 0, bomb));
                //Draw in left
                if (d == 2)
                    Minimap.SetPixel(0, i, new Color(enemy, 0, bomb));
                //Draw in up
                if (d == 3)
                    Minimap.SetPixel(i, 6, new Color(enemy, 0, bomb));
            }
        }
        Minimap.Apply();
    }
    private void drawNearby()
    {
        float[] nearmap = GetNearby(Players[0], 2, Mapping.Bomb, Mapping.Enemy, Mapping.Wall, Mapping.Box);
        for (int x = 0; x < 5; x++)
        {
            for (int z = 0; z < 5; z++)
            {
                float bomb = nearmap[(z * 5 * 4) + (x * 4) + 0];
                float enemy = nearmap[(z * 5 * 4) + (x * 4) + 1];
                float wall = nearmap[(z * 5 * 4) + (x * 4) + 2];
                float box = nearmap[(z * 5 * 4) + (x * 4) + 3];
                //Choose color for things
                Color color = Color.black;
                if (wall > 0.5f)
                    color = Color.white;
                if (box > 0.5f)
                    color = new Color(0.6f, 0f, 0.6f);
                if (enemy > 0.5f || bomb > 0.1f)
                    color = new Color(enemy, 0f, bomb);
                NearMap.SetPixel(x, z, x == 2 && z == 2 ? Color.green : color);
            }
        }
        NearMap.Apply();
    }
}
