using System;
using System.Collections;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Generator;
using SkyFrameWork;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChunkManager : SingletonMono<ChunkManager>
{
    public GameObject chunkPrefab;
    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    public Dictionary<Vector3Int, bool> mapsState = new Dictionary<Vector3Int, bool>();
    public Dictionary<Vector3Int, Block[,,]> maps = new Dictionary<Vector3Int, Block[,,]>();

    /// <summary>
    /// chunk X长度
    /// </summary>
    public readonly int length = 16;

    /// <summary>
    /// chunk Z长度
    /// </summary>
    public readonly int width = 16;

    /// <summary>
    /// chunk Y长度
    /// </summary>
    public readonly int height = 256;

    //public int heightMax = 256;

    public int chunkHideDistance = 128;

    // UV偏移
    public float textureUVOffsetX = 1 / 16f;

    public float textureUVOffsetY = 1 / 16f;

    // 贴图偏移
    public Vector2 shrinkSize1 = new Vector2(0.005f, 0.005f);
    public Vector2 shrinkSize2 = new Vector2(-0.005f, 0.005f);
    public Vector2 shrinkSize3 = new Vector2(-0.005f, -0.005f);

    public Vector2 shrinkSize4 = new Vector2(0.005f, -0.005f);

    // 柏林噪音偏移
    public Vector3 offset;
    public Vector3 offsetLand;

    public Vector3 chunkSize = new Vector3(0.1f, 0.1f, 0.1f);
#if UNITY_EDITOR
    [Disable] public int chunksCount = 0;
    [Disable] public int chunksReadyCount = 0;
#endif

    public bool enableChunkLoad = false;

    public bool chunkLoadLocked = false;
    [Disable] [SerializeField] private int seed;
    private BlockManager blockManager;
    private LibNoise.Generator.Perlin noise;
    private LibNoise.Generator.Perlin noiseLand;

    public int Seed
    {
        get => seed;
        set
        {
            seed = value;
            print($"seed is {seed}");
            Random.InitState(seed);
            offset = new Vector3(Random.value * 1000, Random.value * 1000, Random.value * 1000);
            offsetLand = new Vector3(Random.value * 1000, Random.value * 1000, Random.value * 1000);
            noise = new Perlin(0.7f, 0.2f, 0.1f, 12, seed, QualityMode.High);
            noiseLand = new Perlin(0.05f, 2f, 0.5f, 2, seed, QualityMode.High);
        }
    }

    protected override void Awake()
    {
        blockManager = BlockManager.Instance;
        //Seed = Random.Range(-99999999, 99999999);
        //seed = -31428468;
        instance = this;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (chunks != null)
            chunksCount = chunks.Count;
#endif
        if (enableChunkLoad)
        {
            LoadChunk();
        }
    }

    private void LoadChunk()
    {
        if (WorldManager.Instance.playerIsNull)
            return;
        float nearDistance = Mathf.Infinity;
        Chunk loadChunk = null;
#if UNITY_EDITOR
        chunksReadyCount = 0;
#endif
        foreach (Chunk item in chunks.Values)
        {
            float tagDistance =
                Vector3.Distance(item.transform.position, WorldManager.Instance.player.transform.position);
            if (tagDistance > nearDistance)
                continue;
            if (item.ready)
            {
#if UNITY_EDITOR
                chunksReadyCount += 1;
#endif
                continue;
            }

            nearDistance = tagDistance;
            loadChunk = item;
        }

        if (loadChunk != null && loadChunk.isInit == false)
            loadChunk.ChunkSpawn();
    }

    public void CreatChunk(Vector3Int chunkSerial)
    {
        Chunk chunk = ((GameObject) Instantiate(chunkPrefab,
            new Vector3(chunkSerial.x * length * chunkSize.x, chunkSerial.y * height * chunkSize.y, chunkSerial.z * width * chunkSize.z), Quaternion.identity,
            transform)).GetComponent<Chunk>();
        chunk.chunkSerial = chunkSerial;
        chunk.transform.localScale = chunkSize;
        chunk.name = $"Chunk {chunkSerial}";
        chunks.Add(chunkSerial, chunk);
    }

    public Chunk GetChunk(Vector3Int chunkSerial)
    {
        chunks.TryGetValue(chunkSerial, out Chunk tagChunk);
        return tagChunk;
    }

    public bool HasChunk(Vector3Int chunkSerial)
    {
        return chunks.ContainsKey(chunkSerial);
    }

    public void ClearChunk(Vector3Int chunkSerial)
    {
        if (chunks.TryGetValue(chunkSerial, out Chunk tagChunk))
        {
            Destroy(tagChunk.gameObject);
            chunks.Remove(chunkSerial);
        }

        if (maps.ContainsKey(chunkSerial))
        {
            maps.Remove(chunkSerial);
        }
    }

    public void ClearAllChunk()
    {
        foreach (var item in chunks)
        {
            Destroy(item.Value.gameObject);
        }

        chunks.Clear();
        maps.Clear();
    }

    public Block[,,] CreatMap(Vector3Int chunkSerial,bool createStruct = false)
    {
        Block[,,] map;
        List<Vector3Int> grassList = new List<Vector3Int>();
        if (!maps.TryGetValue(chunkSerial,out map))
        {
            map = new Block[length, height, width];
            for (int x = 0; x < length; x++)
            {

                for (int y = 0; y < height; y++)
                {

                    for (int z = 0; z < width; z++)
                    {
                        Block block = GetTheoreticalBlock(chunkSerial, x, y, z);
                        if (block != null)
                        {
                            if (block.blockID == "Dirt" && GetTheoreticalBlock(chunkSerial, x, y + 1, z) == null)
                            {
                                block = BlockManager.Instance.GetBlock("Grass");
                                grassList.Add(new Vector3Int(x, y, z));
                            }
                        }
                        map[x, y, z] = block;
                    }
                }
            }
            mapsState[chunkSerial] = false;
        }
        else
        {
            for (int x = 0; x < length; x++)
            {

                for (int y = 0; y < height; y++)
                {

                    for (int z = 0; z < width; z++)
                    {
                        Block block = map[x,y,z];
                        if (block != null && block.blockID == "Grass")
                        {
                            grassList.Add(new Vector3Int(x, y, z));
                        }
                    }
                }
            }
        }
        if (createStruct)
        {
            CreateTree(chunkSerial, map, grassList);
            mapsState[chunkSerial] = true;
        }
        
        return map;
    }

    public bool IsMapExist(Vector3Int chunkSerial)
    {
        return WorldManager.Instance.IsChunkExist(chunkSerial);
    }

    public Block[,,] GetOrCreateMap(Vector3Int chunkSerial,bool createStruct = false)
    {
        lock (maps)
        {
            if (maps.ContainsKey(chunkSerial) && (!createStruct || mapsState.TryGetValue(chunkSerial, out bool mapState) && mapState))
            {
                return maps[chunkSerial];
            }
        }

        Block[,,] tagMap;
        ChunkData chunkData = WorldManager.Instance.LoadChunk(chunkSerial);

        if (chunkData == null)
        {
            tagMap = CreatMap(chunkSerial,createStruct);
        }
        else
        {
            tagMap = GetMapData(chunkData);
            mapsState[chunkSerial] = true;
        }
        
        lock (maps)
        {
            if (maps.ContainsKey(chunkSerial))
            {
                return maps[chunkSerial];
            }
            maps.Add(chunkSerial, tagMap);
        }

        return tagMap;
    }


    /// <summary>
    /// 从chunk数据中读取map
    /// </summary>
    /// <param name="chunkData"></param>
    /// <returns></returns>
    public Block[,,] GetMapData(ChunkData chunkData)
    {
        Block[,,] map = new Block[length, height, width];
        var blockMap = WorldManager.Instance.blockMapData;
        int i = 0;
        for (int x = 0; x < length; x++)
        {

            for (int y = 0; y < height; y++)
            {

                for (int z = 0; z < width; z++)
                {
                    int curBlockNum = chunkData.chunkBlocks[i];
                    if (curBlockNum != -1 && blockMap.blockMap.TryGetValue(curBlockNum.ToString(), out string blockID))
                    {
                        map[x, y, z] = BlockManager.Instance.GetBlock(blockID);
                    }
                    else
                    {
                        map[x, y, z] = null;
                    }

                    i++;
                }
            }
        }

        return map;
    }

    /// <summary>
    /// 从当前的数据流中读取chunk数据
    /// </summary>
    /// <param name="chunkSerial"></param>
    /// <returns></returns>
    public ChunkData GetChunkData(Vector3Int chunkSerial)
    {
        ChunkData chunkData = new ChunkData();
        Block[,,] map = GetOrCreateMap(chunkSerial,true);
        var blockMap = BlockManager.Instance.GetBlockMapOverturn(WorldManager.Instance.blockMapData);
        chunkData.chunkBlocks =
            new int[length * height * width];
        int i = 0;
        for (int x = 0; x < length; x++)
        {
            
            for (int y = 0; y < height; y++)
            {
                
                for (int z = 0; z < width; z++)
                {
                    if (map[x, y, z] != null && blockMap.TryGetValue(map[x, y, z].blockID,out string blockNum))
                    {
                        chunkData.chunkBlocks[i] = Convert.ToInt32(blockNum);
                    }
                    else
                    {
                        chunkData.chunkBlocks[i] = -1;
                    }
                    i++;
                }
            }
        }
        return chunkData;
    }
    
    
    public Vector3Int PositionToChunkSerial(Vector3 position)
    {
        int chunkX = Mathf.FloorToInt((float)Mathf.FloorToInt(position.x / chunkSize.x) / length);
        int chunkY = Mathf.FloorToInt((float)Mathf.FloorToInt(position.y / chunkSize.y) / height);
        int chunkZ = Mathf.FloorToInt((float)Mathf.FloorToInt(position.z / chunkSize.z) / width);
        return new Vector3Int(chunkX, chunkY, chunkZ);
    }
    
    public double GetNoiseValue(float posX, float posY, float posZ)
    {
        double noiseX = Mathf.Abs((posX + offset.x) / 20);
        double noiseY = Mathf.Abs((posY + offset.y) / 20);
        double noiseZ = Mathf.Abs((posZ + offset.z) / 20);
        double noiseValue = noise.GetValue(noiseX,noiseY,noiseZ);
        noiseValue += (80 - posY)/18;
        noiseValue /= posY / 22;
        return noiseValue;
    }
    
    public double GetLandNoiseValue(float posX, float posY, float posZ)
    {
        double noiseX = Mathf.Abs((posX + offsetLand.x) / 20);
        double noiseY = Mathf.Abs((posY + offsetLand.y) / 20);
        double noiseZ = Mathf.Abs((posZ + offsetLand.z) / 20);
        double noiseValue = noiseLand.GetValue(noiseX,noiseY,noiseZ);
        return noiseValue;
    }
    
    public Block GetTheoreticalBlock(Vector3Int chunkSerial,int x, int y, int z)
    {
        float posX = chunkSerial.x * Instance.length + x;
        float posY = chunkSerial.y * Instance.height + y;
        float posZ = chunkSerial.z * Instance.width + z;
        double noiseValue;
        double landNoiseValue = GetLandNoiseValue(posX, 50, posZ);
        
        if (landNoiseValue > 0.1)
        {
            noiseValue = GetNoiseValue(posX, posY, posZ);
            noiseValue /= (Mathf.Pow((float) (landNoiseValue * 10),1.2f));
            //noiseValue /= (landNoiseValue * 10);
            if (noiseValue > 0.25)
            {
                if (noiseValue > 0.4)
                {
                    return blockManager.GetBlock("Stone");
                }

                if (landNoiseValue > 0.15 && (y < 54 || noiseValue < 0.3))
                {
                    return blockManager.GetBlock("Sand");
                }
                else
                {
                    return blockManager.GetBlock("Dirt");
                }
            }
        }
        else
        {
            noiseValue = GetNoiseValue(posX, posY, posZ);
            if (noiseValue > 0.25)
            {
                if (noiseValue > 0.4)
                {
                    return blockManager.GetBlock("Stone");
                }

                return blockManager.GetBlock("Dirt");
            }
        }
        //print($"{posY} {noiseValue}");
        return null;
    }

    public void CreateTree(Vector3Int chunkSerial,Block[,,] blocks,List<Vector3Int> grassList)
    {
        System.Random random = new System.Random(seed + chunkSerial.x + chunkSerial.z);
        Block treeChunk = BlockManager.Instance.GetBlock("OakLog");
        Block treeLeaves = BlockManager.Instance.GetBlock("OakLeaves");
        foreach (var pos in grassList)
        {
            if (random.Next(0, 900) < 6)
            {
                int treeHeight = random.Next(3, 7);
                int treeWidth = random.Next(1, 3);
                Vector3 treeHead = new Vector3(pos.x, pos.y, pos.z);
                for (int i = 1; i <= treeHeight && i+pos.y < height && blocks[pos.x, pos.y + i, pos.z] == null; i++)
                {
                    SetTrueBlock(chunkSerial, pos.x, pos.y + i, pos.z, treeChunk);
                    treeHead = new Vector3(pos.x, pos.y + i , pos.z);
                }

                
                for (int x = pos.x -treeWidth; x <= pos.x + treeWidth; x++)
                {
                    for (int y = (int)treeHead.y - treeWidth; y <= (int)treeHead.y + treeWidth; y++)
                    {
                        for (int z = pos.z - treeWidth; z <= pos.z + treeWidth; z++)
                        {
                            float distance = Vector3.Distance(new Vector3(x, y, z),treeHead);
                            if (distance > treeWidth)
                                continue;
                            Block block;
                            if (x >= length || y >= height ||
                                z >= width || x < 0 || y < 0 || z < 0)
                            {
                                block = GetTrueBlock(chunkSerial, x, y, z);
                            }
                            else
                            {
                                block = blocks[x, y, z];
                            }
                            if (block != null)
                                continue;
                            SetTrueBlock(chunkSerial, x, y, z, treeLeaves);
                        }
                    }
                }
            }

            
        }
    }
    
    public Block GetTrueBlock(Vector3Int chunkSerial, int x, int y, int z)
    {
        int posX = chunkSerial.x * Instance.length + x;
        int posY = y;
        int posZ = chunkSerial.z * Instance.width + z;
        if (posY >= height || posY < 0)
            return null;
        Vector3Int tagSerial = PositionToChunkSerial(new Vector3(posX * chunkSize.x, posY * chunkSize.y, posZ * chunkSize.z));
        Block[,,] map = GetOrCreateMap(tagSerial,false);
        return map[(x + length) % length, (y + height) % height, (z + width) % width];
    }
    
    public void SetTrueBlock(Vector3Int chunkSerial, int x, int y, int z,Block setBlock)
    {
        int posX = chunkSerial.x * Instance.length + x;
        int posY = y;
        int posZ = chunkSerial.z * Instance.width + z;
        if (posY >= height || posY < 0)
            return;
        Vector3Int tagSerial = PositionToChunkSerial(new Vector3(posX * chunkSize.x, posY * chunkSize.y, posZ * chunkSize.z));
        Block[,,] map = GetOrCreateMap(tagSerial,false);
        if (chunks.TryGetValue(tagSerial, out Chunk chunk) && chunk != null)
        {
            if (chunk.ready)
            {
                chunk.SetBlock(new Vector3((x + length) % length,
                    (y + height) % height, (z + width) % width), setBlock);
            }
            else
            {
                chunk.needUpdate = true;
                map[(x + length) % length, (y + height) % height, (z + width) % width] = setBlock;
            }
        }
        else
        {
            map[(x + length) % length, (y + height) % height, (z + width) % width] = setBlock;
        }
    }
}
