using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image image;
    public TMP_Text textMesh;
    public ButtonExpand btn;
    public RectTransform rectTransform;

    /// <summary>
    /// 自定义数据
    /// </summary>
    public object data;
    // Start is called before the first frame update

    public void SetShowItem(ItemEntity itemEntity,bool btnState = true,bool numState = true)
    {
        if (itemEntity?.ItemPrototype != null)
        {
            image.gameObject.SetActive(true);
            image.sprite = itemEntity.ItemPrototype.icon;
            textMesh.gameObject.SetActive(numState);
            textMesh.text = itemEntity.itemCount.ToString();
        }
        else
        {
            image.gameObject.SetActive(false);
            textMesh.gameObject.SetActive(false);
        }
        btn.gameObject.SetActive(btnState);
        btn.ResetListener();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
