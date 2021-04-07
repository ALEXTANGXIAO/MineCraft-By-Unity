using System;
using System.Collections;
using System.Collections.Generic;
using SkyFrameWork;
using UnityEngine;

public class UIManager : SingletonMono<UIManager>
{
    public GameObject canvas;
    public InventoryPanel inventoryPanel;
    public ToolBarPanel toolBarPanel;
    public DragComponent dragComponent;
    public PausePanel pausePanel;
    public bool isInventoryPanelNotNull;
    
    protected override void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        isInventoryPanelNotNull = inventoryPanel != null;
        ShowMainPanel();
    }

    public void HideAllPanel()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        inventoryPanel.HidePanel();
        toolBarPanel.HidePanel();
        pausePanel.HidePanel();
        SetPlayerControl(false);
    }

    public void ShowMainPanel()
    {
        HideAllPanel();
        toolBarPanel.ShowPanel();
        Cursor.lockState = CursorLockMode.Locked;
        SetPlayerControl(true);
    }

    public void ShowInventoryPanel()
    {
        HideAllPanel();
        inventoryPanel.ShowPanel();
    }

    public void ShowPausePanel()
    {
        HideAllPanel();
        pausePanel.ShowPanel();
    }

    public void RefreshUI()
    {
        toolBarPanel.RefreshToolBar();
        inventoryPanel.RefreshInventory();
    }
    
    public void SetPlayerControl(bool open)
    {
        PlayerControl playerControl = WorldManager.Instance.player.GetComponent<PlayerControl>();
        PlayerCamera playerCamera = WorldManager.Instance.player.GetComponent<PlayerCamera>();
        if (playerControl != null)
        {
            playerControl.allowPlayerControl = open;
            playerCamera.allowPlayerControl = open;
        }
    }
    
}
