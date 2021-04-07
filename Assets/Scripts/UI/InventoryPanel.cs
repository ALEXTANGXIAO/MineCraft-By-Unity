using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour,IUIPanel
{
    public GameObject ItemSlotPrefab;
    public GameObject BagGroup;
    public GameObject ToolBarGroup;
    private bool isOpen = false;
    
    private ItemEntity curDragItem = null;
    private List<ItemSlot> inventoryItemSlots = new List<ItemSlot>();
    private List<ItemSlot> toolbarItemSlots = new List<ItemSlot>();
    private PlayerInventory playerInventory;

    
    public bool IsOpen => isOpen;

    public void Awake()
    {
        playerInventory = WorldManager.Instance.player.GetComponent<PlayerInventory>();
        CreateInventorySlot();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (curDragItem != null)
                {
                    DragItem(null);
                }
            }
        }
    }

    public void CreateInventorySlot()
    {
        if (WorldManager.Instance.playerIsNull)
            return;
        if (playerInventory == null)
            return;
        
        foreach (var o in inventoryItemSlots)
        {
            Destroy(o.gameObject);
        }
        inventoryItemSlots.Clear();
        for (int i = 0; i < playerInventory.inventoryItems.Capacity; i++)
        {
            var o = Instantiate(ItemSlotPrefab,BagGroup.transform);
            inventoryItemSlots.Add(o.GetComponent<ItemSlot>());
        }
        
        foreach (var o in toolbarItemSlots)
        {
            Destroy(o.gameObject);
        }
        toolbarItemSlots.Clear();
        for (int i = 0; i < playerInventory.toolBarItems.Capacity; i++)
        {
            var o = Instantiate(ItemSlotPrefab,ToolBarGroup.transform);
            toolbarItemSlots.Add(o.GetComponent<ItemSlot>());
        }
        
    }

    public void RefreshInventory()
    {
        if (WorldManager.Instance.playerIsNull)
            return;
        if (isOpen == false)
            return;
        PlayerInventory playerInventory = WorldManager.Instance.player.GetComponent<PlayerInventory>();
        if (playerInventory == null)
            return;

        if (playerInventory.inventoryCapacity != inventoryItemSlots.Count)
            CreateInventorySlot();
        
        var playerInventoryItems = playerInventory.inventoryItems;
        for (int i = 0; i < inventoryItemSlots.Count; i++)
        {
            BindItemSlot(
                inventoryItemSlots[i], 
                playerInventoryItems.GetItemFromSlot(i),
                new SlotData() {slotType = 0, slotIndex = i});
        }
        
        var playerToolBarItems = playerInventory.toolBarItems;
        for (int i = 0; i < toolbarItemSlots.Count; i++)
        {
            BindItemSlot(
                toolbarItemSlots[i], 
                playerToolBarItems.GetItemFromSlot(i),
                new SlotData() {slotType = 1, slotIndex = i});
        }
    }

    private void BindItemSlot(ItemSlot itemSlot, ItemEntity itemEntity, SlotData slotData)
    {
        itemSlot.data = slotData;
        itemSlot.SetShowItem(itemEntity);
        itemSlot.btn.leftClick = BtnLeftClick;
        itemSlot.btn.rightClick = BtnRightClick;
        itemSlot.btn.beginDragEvent = BtnBeginDragEvent;
        itemSlot.btn.endDragEvent = BtnEndDragEvent;
    }
    
    private ItemContainer GetTagContainer(int containerType)
    {
        switch (containerType)
        {
            case 0:
                return playerInventory.inventoryItems;
            case 1:
                return playerInventory.toolBarItems;
            default:
                return null;
        }
    }
    
    private void BtnLeftClick(ButtonExpand buttonExpand,PointerEventData eventData)
    {
        Debug.Log("Left Click");
        EventSystem.current.SetSelectedGameObject(null);
        SlotData curSlotData= (SlotData)buttonExpand.transform.parent.GetComponent<ItemSlot>().data;
        ItemContainer curContainer = GetTagContainer(curSlotData.slotType);
        ItemEntity oldItem = curContainer.AddItemToSlot(curSlotData.slotIndex, curDragItem);
        DragItem(oldItem);
        RefreshInventory();
    }
    
    private void BtnRightClick(ButtonExpand buttonExpand,PointerEventData eventData)
    {
        //Debug.Log("Right Click");
        EventSystem.current.SetSelectedGameObject(null);
        SlotData curSlotData= (SlotData)buttonExpand.transform.parent.GetComponent<ItemSlot>().data;
        ItemContainer curContainer = GetTagContainer(curSlotData.slotType);
        if (curDragItem != null)
        {
            curContainer.AddItem(DragItem(null));
        }
        else
        {
            ItemEntity oldItem = curContainer.GetItemFromSlot(curSlotData.slotIndex);
            if (oldItem != null)
            {
                if (oldItem.itemCount > 1)
                {
                    int halfCount = oldItem.itemCount / 2;
                    oldItem.Cost(halfCount);
                    ItemEntity tagItem = new ItemEntity(oldItem, halfCount);
                    DragItem(tagItem);
                }
                else
                {
                    curContainer.RemoveItemFromSlot(curSlotData.slotIndex);
                    DragItem(oldItem);
                }
            }
        }
        RefreshInventory();
    }
    
    private void BtnBeginDragEvent(ButtonExpand buttonExpand,PointerEventData eventData)
    {
        SlotData curSlotData= (SlotData)buttonExpand.transform.parent.GetComponent<ItemSlot>().data;
        ItemContainer curContainer = GetTagContainer(curSlotData.slotType);
        if (curDragItem != null)
        {
            buttonExpand.isDragging = false;
            return;
        }

        //Debug.Log("Start Drag");
        EventSystem.current.SetSelectedGameObject(null);
        ItemEntity oldItem = curContainer.RemoveItemFromSlot(curSlotData.slotIndex);
        DragItem(oldItem);
        RefreshInventory();
    }
    
    private void BtnEndDragEvent(ButtonExpand buttonExpand,PointerEventData eventData)
    {
        SlotData curSlotData= (SlotData)buttonExpand.transform.parent.GetComponent<ItemSlot>().data;
        ItemContainer curContainer = GetTagContainer(curSlotData.slotType);
        if (buttonExpand.isDragging == false) 
            return;
        //Debug.Log("End Drag");
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, results);
        // 防止错误拖动，验证目标按钮
        bool isDragSuccess = false;
        foreach (RaycastResult raycastResult in results)
        {
            var btnExpand = raycastResult.gameObject.GetComponent<ButtonExpand>();
            if (btnExpand == null) 
                break;
            var parent = btnExpand.transform.parent;
            if (parent == null) 
                break;
            var tagItemSlot = parent.gameObject.GetComponent<ItemSlot>();
            if (tagItemSlot.data is SlotData tagSlotData)
            {
                // 置换目标与原格子的物品
                ItemContainer tagContainer = GetTagContainer(tagSlotData.slotType);
                ItemEntity oldItem = tagContainer.RemoveItemFromSlot(tagSlotData.slotIndex);
                tagContainer.AddItemToSlot(tagSlotData.slotIndex, DragItem(null));
                curContainer.AddItemToSlot(curSlotData.slotIndex, oldItem);
                RefreshInventory();
                isDragSuccess = true;
                break;
            }
        }
        // 如果未能拖拽成功，置回原状态
        if (!isDragSuccess)
        {
            curContainer.AddItemToSlot(curSlotData.slotIndex, DragItem(null));
        }
    }

    public ItemEntity DragItem(ItemEntity itemEntity)
    {
        if (itemEntity != null)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
        ItemEntity oldItem = curDragItem;
        curDragItem = itemEntity;
        var dragImage = UIManager.Instance.dragComponent;
        if (curDragItem != null)
        {
            ItemSlot itemSlot = dragImage.SetDragComponent(ItemSlotPrefab).GetComponent<ItemSlot>();
            var vector2Center = new Vector2(0.5f, 0.5f);
            itemSlot.rectTransform.pivot = vector2Center;
            itemSlot.rectTransform.anchorMin = vector2Center;
            itemSlot.rectTransform.anchorMax = vector2Center;
            itemSlot.rectTransform.anchoredPosition = new Vector2(0, 0);
            itemSlot.SetShowItem(curDragItem,false,true);
            dragImage.IsDragging = true;
        }
        else
        {
            dragImage.ClearDragComponent();
            dragImage.IsDragging = false;
        }
        return oldItem;
    }
    
    public void ShowPanel()
    {
        isOpen = true;
        gameObject.SetActive(true);
        RefreshInventory();
    }


    public void HidePanel()
    {
        var dragCom = UIManager.Instance.dragComponent;
        dragCom.DragReset();
        curDragItem = null;
        gameObject.SetActive(false);
        isOpen = false;
    }
}
