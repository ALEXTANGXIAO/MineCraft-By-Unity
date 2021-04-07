using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragComponent : MonoBehaviour
{
    private bool isDragging;
    private Canvas canvas;
    private Image image;
    private GameObject dragObject;
    private Camera mainCamera;

    public bool IsDragging
    {
        get => isDragging;
        set
        {
            isDragging = value;
        }
    }

    public bool ImageEnable
    {
        get => image.enabled;
        set
        {
            image.enabled = value;
        }
    }
    
    
    private void Start()
    {
        mainCamera = Camera.main;
        image = GetComponent<Image>();
        canvas = UIManager.Instance.canvas.GetComponent<Canvas>();
        DragReset();
    }
    
    void Update()
    {
        if (!isDragging) 
            return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,Input.mousePosition, canvas.worldCamera,out var globalMousePos);
        image.rectTransform.anchoredPosition = globalMousePos;
    }

    public void SetDragSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }
    
    public void SetImageSize(float width,float height)
    {
        image.rectTransform.sizeDelta = new Vector2(width,height);
    }
    
    public GameObject SetDragComponent(GameObject prefab)
    {
        ClearDragComponent();
        dragObject = Instantiate(prefab, transform);
        return dragObject;
    }
    
    public void ClearDragComponent()
    {
        if (dragObject != null)
        {
            Destroy(dragObject);
        }
    }

    public void DragReset()
    {
        if (image != null)
        {
            image.sprite = null;
            image.enabled = false;
        }
        ClearDragComponent();
        IsDragging = false;
        gameObject.SetActive(true);
    }
}
