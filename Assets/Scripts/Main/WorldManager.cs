using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SkyFrameWork;
using UnityEngine;
using LitJson;
using UnityEngine.SocialPlatforms;
using Random = System.Random;

public class WorldManager : SingletonMono<WorldManager>
{
    public GameObject player;
    public string archiveDir = "Default";
    public string archiveName = "Default";
    public int worldSeed = 0;

    [HideInInspector]
    public bool playerIsNull;

    public ArchiveData archiveData;
    public BlockMapData blockMapData;
    public PlayerData playerData;

    


    
    protected override void Awake()
    {
        instance = this;
        if (player == null)
            player = GameObject.FindWithTag("Player");
        playerIsNull = player == null;
    }

    private void Start()
    {
        if (GameManager.Instance.testMode)
        {
            InitArchive();
        }
    }

    public void InitArchive(bool isCreate = false)
    {
        ChunkManager.Instance.ClearAllChunk();
        GameManager.Instance.GetOrCreateArchiveDirectory(archiveDir,"Chunk");
        LoadArchive(isCreate);
        LoadBlockMap(isCreate);
        LoadPlayerData(isCreate);
        ChunkManager.Instance.Seed = worldSeed;
        ChunkManager.Instance.enableChunkLoad = true;
    }
    
    

    public void LoadArchive(bool isCreate)
    {
        string tagFilePath = GameManager.Instance.GetArchiveDataPath(archiveDir);
        if (!isCreate && File.Exists(tagFilePath))
        {
            using (var streamReader = new StreamReader(tagFilePath))
            {
                string text = streamReader.ReadToEnd();
                try
                {
                    archiveData = JsonMapper.ToObject<ArchiveData>(text);
                    worldSeed = archiveData.seed;
                    archiveName = archiveData.saveName;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    archiveData = new ArchiveData();
                    archiveData.seed = worldSeed;
                    archiveData.saveName = archiveName;
                }
            }
        }
        else
        {
            archiveData = new ArchiveData();
            archiveData.seed = worldSeed;
            archiveData.saveName = archiveName;
        }
    }
    
    public void LoadBlockMap(bool isCreate)
    {
        string tagFilePath = GameManager.Instance.GetBlockMapPath(archiveDir);
        if (!isCreate && File.Exists(tagFilePath))
        {
            using (var streamReader = new StreamReader(tagFilePath))
            {
                string text = streamReader.ReadToEnd();
                try
                {
                    blockMapData = JsonMapper.ToObject<BlockMapData>(text);
                    BlockManager.Instance.UpdateBlockMap(blockMapData);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    blockMapData = BlockManager.Instance.CreateBlockMap();
                }
            }
        }
        else
        {
            blockMapData = BlockManager.Instance.CreateBlockMap();
        }
    }
    
    public void LoadPlayerData(bool isCreate)
    {
        string tagFilePath = GameManager.Instance.GetPlayerDataPath(archiveDir);
        if (!isCreate && File.Exists(tagFilePath))
        {
            using (var streamReader = new StreamReader(tagFilePath))
            {
                string text = streamReader.ReadToEnd();
                try
                {
                    playerData = JsonMapper.ToObject<PlayerData>(text);
                    var player = WorldManager.Instance.player;
                    player.transform.position = new Vector3(playerData.playerPosX,playerData.playerPosY,playerData.playerPosZ);
                    player.GetComponent<PlayerCamera>().SetCinemaOffset(new Vector3(playerData.playerDirX,playerData.playerDirY,playerData.playerDirZ));
                    var playerInv = player.GetComponent<PlayerInventory>();
                    playerInv.inventoryItems.LoadArray(playerData.playerInventory);
                    playerInv.toolBarItems.LoadArray(playerData.playerTool);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    playerData = new PlayerData();
                }
            }
        }
        else
        {
            playerData = new PlayerData();
        }
    }

    public void SaveAllData()
    {
        Debug.Log($"Start Save Archive: {archiveDir}");
        SaveArchive();
        Debug.Log($"Save archive success");
        SaveBlockMap();
        Debug.Log($"Save BlockMap success");
        SaveAllChunk();
        Debug.Log($"Save Chunks success");
        SavePlayer();
        Debug.Log($"Save Player success");
        string tagFilePath = GameManager.Instance.GetArchiveDataPath(archiveDir);
        Debug.Log($"{tagFilePath} save success!");
    }
    
    public void SaveArchive()
    {
        string jsonContent = JsonMapper.ToJson(archiveData);
        string tagFilePath = GameManager.Instance.GetArchiveDataPath(archiveDir);
        using(var stream = new StreamWriter(tagFilePath))
        {
            stream.Write(jsonContent);
        }
    }
    
    public void SaveBlockMap()
    {
        string jsonContent = JsonMapper.ToJson(blockMapData);
        string tagFilePath = GameManager.Instance.GetBlockMapPath(archiveDir);;
        using(var stream = new StreamWriter(tagFilePath))
        {
            stream.Write(jsonContent);
        }
    }
    
    public void SavePlayer()
    {
        var player = WorldManager.Instance.player;
        var playerInv = player.GetComponent<PlayerInventory>();
        var playerPos = player.transform.position;
        playerData.playerPosX = playerPos.x;
        playerData.playerPosY = playerPos.y;
        playerData.playerPosZ = playerPos.z;
        var playerDir = player.GetComponent<PlayerCamera>().cameraOffset;
        playerData.playerDirX = playerDir.x;
        playerData.playerDirY = playerDir.y;
        playerData.playerDirZ = playerDir.z;
        playerData.playerInventory = playerInv.inventoryItems.ToArray();
        playerData.playerTool = playerInv.toolBarItems.ToArray();
        try
        {
            string jsonContent = JsonMapper.ToJson(playerData);
            string tagFilePath = GameManager.Instance.GetPlayerDataPath(archiveDir);
            using(var stream = new StreamWriter(tagFilePath))
            {
                stream.Write(jsonContent);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(playerData);
            throw;
        }
    }
    
    public static string ToBase64String(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(bytes);
    }

    private string GetChunkDataPath(Vector3Int chunkSerial)
    {
        return
            $"{GameManager.Instance.GetOrCreateArchiveDirectory(archiveDir,"Chunk")}/{ToBase64String($"MC{chunkSerial.x}.{chunkSerial.y}.{chunkSerial.z}.chunk")}.dat";
    }
    
    public void SaveAllChunk()
    {
        lock (ChunkManager.Instance.maps)
        {
            // foreach (var maps in ChunkManager.Instance.maps)
            // {
            //     Vector3Int chunkSerial = maps.Key;
            //     ChunkData chunkData = ChunkManager.Instance.GetChunkData(chunkSerial);
            //     string jsonContent = JsonMapper.ToJson(chunkData);
            //     string tagFilePath = GetChunkDataPath(chunkSerial);
            //     using(var stream = new StreamWriter(tagFilePath))
            //     {
            //         stream.Write(jsonContent);
            //     }
            //     //Debug.Log($"{tagFilePath} save success!");
            // }
            var keys = ChunkManager.Instance.mapsState.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                try
                {
                    var chunkSerial = keys[i];
                    if (!ChunkManager.Instance.maps.TryGetValue(chunkSerial, out _) ||
                        !ChunkManager.Instance.mapsState[chunkSerial]) continue;
                    ChunkData chunkData = ChunkManager.Instance.GetChunkData(chunkSerial);
                    string jsonContent = JsonMapper.ToJson(chunkData);
                    string tagFilePath = GetChunkDataPath(chunkSerial);
                    using(var stream = new StreamWriter(tagFilePath))
                    {
                        stream.Write(jsonContent);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }

    public void SaveChunk(Vector3Int chunkSerial)
    {
        ChunkData chunkData = ChunkManager.Instance.GetChunkData(chunkSerial);
        string jsonContent = JsonMapper.ToJson(chunkData);
        string tagFilePath = GetChunkDataPath(chunkSerial);
        using(var stream = new StreamWriter(tagFilePath))
        {
            stream.Write(jsonContent);
        }
    }

    public bool IsChunkExist(Vector3Int chunkSerial)
    {
        string tagFilePath = GetChunkDataPath(chunkSerial);
        return File.Exists(tagFilePath);
    }
    
    /// <summary>
    /// 从文件档案中获取chunk数据
    /// </summary>
    /// <returns></returns>
    public ChunkData LoadChunk(Vector3Int chunkSerial)
    {
        string tagFilePath = GetChunkDataPath(chunkSerial);
        if (!File.Exists(tagFilePath))
            return null;
        ChunkData chunkData;
        using (var streamReader = new StreamReader(tagFilePath))
        {
            string text = streamReader.ReadToEnd();
            try
            {
                chunkData = JsonMapper.ToObject<ChunkData>(text);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
        return chunkData;
    }

}
