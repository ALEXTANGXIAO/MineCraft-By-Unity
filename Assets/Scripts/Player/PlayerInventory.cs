using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int toolBarCapacity = 9;
    public int inventoryCapacity = 27;

    public ItemContainer toolBarItems;
    public ItemContainer inventoryItems;
    private void Awake()
    {
        toolBarItems = new ItemContainer(toolBarCapacity);
        inventoryItems = new ItemContainer(inventoryCapacity);
    }
}
