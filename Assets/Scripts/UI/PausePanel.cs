using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour,IUIPanel
{
    [Disable]
    public bool isPause = false;

    public Button btnCancel;
    public Button btnReturn;
    public Button btnExit;
    
    private bool isOpen;

    public bool IsOpen => isOpen;

    public void Reset()
    {
        btnCancel.onClick.RemoveAllListeners();
        btnCancel.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            UIManager.Instance.ShowMainPanel();
        });
        btnReturn.onClick.RemoveAllListeners();
        btnReturn.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            WorldManager.Instance.SaveAllData();
            GameManager.Instance.LoadMainMenu();
        });
        btnExit.onClick.RemoveAllListeners();
        btnExit.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            WorldManager.Instance.SaveAllData();
            Application.Quit();
        });
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
        isOpen = false;
    }

    public void ShowPanel()
    {
        isOpen = true;
        gameObject.SetActive(true);
        Reset();
        
    }
}
