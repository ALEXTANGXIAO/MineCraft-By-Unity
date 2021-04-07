using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChooseWorldPanel : MonoBehaviour
{
    public Button btnCreateNewWorld;
    public Button btnEnterWorld;
    public Button btnModifyWorld;
    public Button btnDeleteWorld;
    public Button btnCancel;

    public GameObject saveSlotList;
    public GameObject saveSlotPrefab;

    private SaveInfoSlot curSlot = null;
    private List<SaveInfoSlot> slots = new List<SaveInfoSlot>();
    
    public void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        
        btnCreateNewWorld.onClick.RemoveAllListeners();
        btnCreateNewWorld.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            MenuManager.Instance.ShowCreateWorldPanel();
        });
        btnEnterWorld.onClick.RemoveAllListeners();
        btnEnterWorld.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (curSlot != null)
            {
                GameManager.Instance.LoadWorld(curSlot.bindArchive, curSlot.saveDir);
            }
        });
        btnModifyWorld.onClick.RemoveAllListeners();
        btnModifyWorld.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            
        });
        btnDeleteWorld.onClick.RemoveAllListeners();
        btnDeleteWorld.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (curSlot != null)
            {
                Debug.Log(curSlot);
                GameManager.Instance.DeleteArchiveDirectory(curSlot.saveDir);
                Reset();
            }
        });
        btnCancel.onClick.RemoveAllListeners();
        btnCancel.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            MenuManager.Instance.ShowMainMenuPanel();
        });
        var saveSlotTransform = saveSlotList.transform;
        for (int i = 0; i < saveSlotTransform.childCount; i++)
        {
            Destroy(saveSlotTransform.GetChild(i).gameObject);
        }
        EventSystem.current.SetSelectedGameObject(null);
        UnSelAllSlot();
        SetBtnState();
        slots.Clear();
        var archiveDatas = GetAllArchiveDatas();
        foreach (var item in archiveDatas)
        {
            GameObject obj = Instantiate(saveSlotPrefab, saveSlotTransform);
            SaveInfoSlot saveInfoSlot = obj.GetComponent<SaveInfoSlot>();
            saveInfoSlot.saveNameText.text = item.Value.saveName;
            saveInfoSlot.saveDir = item.Key;
            saveInfoSlot.bindArchive = item.Value;
            slots.Add(saveInfoSlot);
            Button btn = obj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                UnSelAllSlot();
                curSlot = saveInfoSlot;
                saveInfoSlot.maskImage.SetActive(true);
            });
        }

        RectTransform rectTransform = saveSlotList.GetComponent<RectTransform>();
        var sizeDeltaY = saveSlotPrefab.GetComponent<RectTransform>().sizeDelta.y * archiveDatas.Count;
        rectTransform.sizeDelta = new Vector2(
            rectTransform.sizeDelta.x,
            sizeDeltaY);
    }

    private void Update()
    {
        SetBtnState();
    }

    private void UnSelAllSlot()
    {
        foreach (var slot in slots)
        {
            slot.maskImage.SetActive(false);
        }

        curSlot = null;
    }
    
    private void SetBtnState()
    {
        if (curSlot == null)
        {
            btnEnterWorld.interactable = false;
            btnModifyWorld.interactable = false;
            btnDeleteWorld.interactable = false;
        }
        else
        {
            btnEnterWorld.interactable = true;
            btnModifyWorld.interactable = true;
            btnDeleteWorld.interactable = true;
        }
    }

    private Dictionary<string,ArchiveData> GetAllArchiveDatas()
    {
        Dictionary<string,ArchiveData> archiveDatas = new Dictionary<string,ArchiveData>();
        DirectoryInfo directoryInfo = new DirectoryInfo($"{GameManager.Instance.persistentDataPath}/SaveFile");
        DirectoryInfo[] dirs = directoryInfo.GetDirectories();
        foreach (DirectoryInfo info in dirs)
        {
            ArchiveData archiveData = GetArchiveData(GameManager.Instance.GetArchiveDataPath(info.Name));
            if (archiveData != null)
            {
                archiveDatas.Add(info.Name,archiveData);
            }
        }
        return archiveDatas;
    }

    private ArchiveData GetArchiveData(string path)
    {
        ArchiveData archiveData = null;
        string tagFilePath = path;
        if (File.Exists(tagFilePath))
        {
            using (var streamReader = new StreamReader(tagFilePath))
            {
                string text = streamReader.ReadToEnd();
                try
                {
                    archiveData = JsonMapper.ToObject<ArchiveData>(text);
                }
                catch (Exception)
                {
                    Debug.LogError($"读取存档 {path} 失败");
                }
            }
        }
        return archiveData;
    }
}
