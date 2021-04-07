using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonExpand : MonoBehaviour, 
    IPointerClickHandler,
    IDragHandler,
    IBeginDragHandler,
    IEndDragHandler
{
    public Action<ButtonExpand,PointerEventData> leftClick;
    public Action<ButtonExpand,PointerEventData> middleClick;
    public Action<ButtonExpand,PointerEventData> rightClick;
    public Action<ButtonExpand,PointerEventData> dragEvent;
    public Action<ButtonExpand,PointerEventData> beginDragEvent;
    public Action<ButtonExpand,PointerEventData> endDragEvent;

    [HideInInspector]
    public bool isDragging;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging)
            return;
        if (eventData.button == PointerEventData.InputButton.Left)
            leftClick?.Invoke(this,eventData);
        else if (eventData.button == PointerEventData.InputButton.Middle)
            middleClick?.Invoke(this,eventData);
        else if (eventData.button == PointerEventData.InputButton.Right)
            rightClick?.Invoke(this,eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragEvent?.Invoke(this,eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        beginDragEvent?.Invoke(this,eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        endDragEvent?.Invoke(this,eventData);
        isDragging = false;
    }

    public void ResetListener()
    {
        leftClick = null;
        rightClick = null;
        middleClick = null;
        dragEvent = null;
        middleClick = null;
        rightClick = null;
    }
}