using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SkyFrameWork;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonMono<GameManager>
{
    public bool testMode = false;
    [Disable]
    public string persistentDataPath;

    [Disable] 
    public bool isOnline;
    [Disable]
    public bool isHost;
    

    protected override void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        base.Awake();
        persistentDataPath = Application.persistentDataPath;
    }

    public void CreateWorld(string worldName,int worldSeed)
    {
        StartCoroutine(LoadScene("Main", () =>
        {
            WorldManager.Instance.worldSeed = worldSeed;
            WorldManager.Instance.archiveName = worldName;
            WorldManager.Instance.archiveDir = worldName;
            WorldManager.Instance.InitArchive(true);
            Debug.Log($"Create New World : {worldName} seed : {worldSeed}");
        }));
    }
    
    public void LoadWorld(ArchiveData archiveData,string dir)
    {
        StartCoroutine(LoadScene("Main", () =>
        {
            WorldManager.Instance.worldSeed = archiveData.seed;
            WorldManager.Instance.archiveDir = dir;
            WorldManager.Instance.archiveName = archiveData.saveName;
            WorldManager.Instance.InitArchive(true);
            Debug.Log($"Load World : {archiveData.saveName} seed : {archiveData.seed}");
        }));
    }
    
    public void LoadMainMenu()
    {
        StartCoroutine(LoadScene("Menu"));
    }

    public IEnumerator LoadScene(string nextScene,Action callback = null)
    {
        if (nextScene != "")
        {
            SceneManager.LoadSceneAsync(nextScene);
            yield return StartCoroutine(waitForLevelToLoad(nextScene,callback));           
        }
    }
    
    private IEnumerator waitForLevelToLoad(string level,Action callback)
    {
        while (SceneManager.GetActiveScene().name != level)
        {
            yield return null;
        }
        callback?.Invoke();
    }
    
    public string GetOrCreateArchiveDirectory(string archiveName,string subDir = "")
    {
        string tagPath = $"{persistentDataPath}/SaveFile/{archiveName}";
        if (!string.IsNullOrEmpty(subDir))
        {
            tagPath += $"/{subDir}";
        }
        if (!Directory.Exists(tagPath))
        {
            Directory.CreateDirectory(tagPath);
        }
        return tagPath;
    }
    
    public string DeleteArchiveDirectory(string archiveName)
    {
        string tagPath = $"{persistentDataPath}/SaveFile/{archiveName}";
        if (Directory.Exists(tagPath))
        {
            Directory.Delete(tagPath,true);
        }
        return tagPath;
    }

    public string GetArchiveDataPath(string archiveName)
    {
        return $"{GetOrCreateArchiveDirectory(archiveName)}/Archive.dat";
    }
    
    public string GetBlockMapPath(string archiveName)
    {
        return $"{GetOrCreateArchiveDirectory(archiveName)}/BlockMap.dat";
    }
    
    public string GetPlayerDataPath(string archiveName)
    {
        return $"{GetOrCreateArchiveDirectory(archiveName)}/Player.dat";
    }

    public void SetServer(bool online, bool host)
    {
        isOnline = online;
        isHost = host;
    }
}
