using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    public Button btnSingleGame;
    public Button btnMutiGame;
    public Button btnOptionalGame;
    public Button btnExitGame;
    
    public void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        btnSingleGame.onClick.RemoveAllListeners();
        btnSingleGame.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            MenuManager.Instance.ShowChooseWorldPanel();
        });
        btnMutiGame.onClick.RemoveAllListeners();
        btnMutiGame.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            
        });
        btnOptionalGame.onClick.RemoveAllListeners();
        btnOptionalGame.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            
        });
        btnExitGame.onClick.RemoveAllListeners();
        btnExitGame.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            Application.Quit();
        });
    }
}