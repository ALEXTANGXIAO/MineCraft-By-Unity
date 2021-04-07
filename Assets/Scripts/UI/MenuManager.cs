using System;
using System.Collections;
using System.Collections.Generic;
using SkyFrameWork;
using UnityEngine;


public class MenuManager : SingletonMono<MenuManager>
{
    public MainMenuPanel menuPanel;
    public ChooseWorldPanel chooseWorldPanel;
    public CreateWorldPanel createWorldPanel;
    
    protected override void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ShowMainMenuPanel();
    }

    public void HideAllPanel()
    {
        menuPanel.gameObject.SetActive(false);
        chooseWorldPanel.gameObject.SetActive(false);
        createWorldPanel.gameObject.SetActive(false);
    }

    public void ShowMainMenuPanel()
    {
        HideAllPanel();
        menuPanel.gameObject.SetActive(true);
        menuPanel.Reset();
    }
    
    public void ShowChooseWorldPanel()
    {
        HideAllPanel();
        chooseWorldPanel.gameObject.SetActive(true);
        chooseWorldPanel.Reset();
    }
    
    public void ShowCreateWorldPanel()
    {
        HideAllPanel();
        createWorldPanel.gameObject.SetActive(true);
        createWorldPanel.Reset();
    }
    
}
