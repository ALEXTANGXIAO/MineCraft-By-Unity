using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateWorldPanel : MonoBehaviour
{
    public TMP_InputField worldNameInput;
    public TMP_InputField worldSeedInput;
    public TMP_Text errorText;
    public Button createButton;
    public Button cancelButton;

    public void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        worldNameInput.text = "";
        worldSeedInput.text = "";
        errorText.text = "";
        createButton.onClick.RemoveAllListeners();
        createButton.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            CreateWorld();
        });
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            EventSystem.current.SetSelectedGameObject(null);
            MenuManager.Instance.ShowChooseWorldPanel();
        });
    }

    public int StrToInt(string str)
    {
        byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        return bytes.Aggregate(0, (current, b) => current + b);
    }
    
    public void CreateWorld()
    {
        errorText.text = "";
        if (string.IsNullOrEmpty(worldNameInput.text))
        {
            errorText.text = "请输入世界名称";
            return;
        }
        if (string.IsNullOrEmpty(worldSeedInput.text))
        {
            errorText.text = "请输入世界种子";
            return;
        }
        GameManager.Instance.CreateWorld(worldNameInput.text,StrToInt(worldSeedInput.text));
    }
}
