using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToolBarPanel : MonoBehaviour,IUIPanel
{
    public GameObject ItemSlotPrefab;
    public GameObject ToolBarGroup;
    public GameObject ChooseCursor;
    
    private List<ItemSlot> toolbarItemSlots = new List<ItemSlot>();
    private bool isOpen = false;
    private int curSelectIndex = 0;

    private PlayerInventory playerInventory;

    public int CurSelectIndex
    {
        get => curSelectIndex;
        set => SwitchCursor(value);
    }

    private void Start()
    {
        playerInventory = WorldManager.Instance.player.GetComponent<PlayerInventory>();
        CreateToolBarSlot();
        RefreshToolBar();
    }

    public void CreateToolBarSlot()
    {
        if (WorldManager.Instance.playerIsNull)
            return;
        if (playerInventory == null)
            return;

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

        curSelectIndex = 0;
    }

    public void RefreshToolBar()
    {
        if (WorldManager.Instance.playerIsNull)
            return;
        if (isOpen == false)
            return;
        if (playerInventory == null)
            return;

        if (playerInventory.toolBarItems.Capacity != toolbarItemSlots.Count)
            CreateToolBarSlot();

        var playerToolBarItems = playerInventory.toolBarItems;
        for (int i = 0; i < toolbarItemSlots.Count; i++)
        {
            BindItemSlot(
                toolbarItemSlots[i], 
                playerToolBarItems.GetItemFromSlot(i),
                new SlotData() {slotType = 0, slotIndex = i});
        }

        ItemSlot curSelectSlot = toolbarItemSlots[curSelectIndex];
        ((RectTransform) ChooseCursor.transform).position = curSelectSlot.rectTransform.position;
    }
    
    private void BindItemSlot(ItemSlot itemSlot, ItemEntity itemEntity, SlotData slotData)
    {
        itemSlot.data = slotData;
        itemSlot.SetShowItem(itemEntity,false,true);
    }

    public bool IsOpen => isOpen;

    public void HidePanel()
    {
        this.gameObject.SetActive(false);
        isOpen = false;
    }

    public void ShowPanel()
    {
        isOpen = true;
        this.gameObject.SetActive(true);
        RefreshToolBar();
    }

    public void SwitchCursor(int index)
    {
        if(!isOpen)
            return;
        curSelectIndex = Mathf.Clamp((index + toolbarItemSlots.Count) % toolbarItemSlots.Count, 0, toolbarItemSlots.Count);
        RefreshToolBar();
    }
    
}
