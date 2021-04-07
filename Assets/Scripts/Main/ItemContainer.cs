using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainer
{
    private int capacity = 10;
    
    public ItemEntity[] items;
    
    public ItemContainer(int tagCapacity = 30)
    {
        Capacity = tagCapacity;
    }
    
    public int Capacity
    {
        get => capacity;
        set
        {
            capacity = value;
            if (items == null)
            {
                items = new ItemEntity[capacity];
            }
            else
            {
                ItemEntity[] oldInventoryItems = items;
                int oldCapacity = oldInventoryItems.Length;
                items = new ItemEntity[oldCapacity];
                int smallCapacity = capacity > oldCapacity ? oldCapacity : capacity;
                for (int i = 0; i < smallCapacity; i++)
                {
                    items[i] = oldInventoryItems[i];
                }
            }
        }
    }

    public ItemEntity[] ToArray()
    {
        ItemEntity[] itemEntities = new ItemEntity[capacity];
        for (int i = 0; i < capacity; i++)
        {
            if (items[i] != null)
                itemEntities[i] = items[i].Clone();
        }

        return itemEntities;
    }

    public void LoadArray(ItemEntity[] itemEntities)
    {
        int length = Mathf.Min(itemEntities.Length, items.Length);
        for (int i = 0; i < length; i++)
        {
            if (itemEntities[i] != null)
                items[i] = itemEntities[i].Clone();
        }
    }
    
    /// <summary>
    /// 返回物品添加是否全部成功
    /// 如果未全部添加成功，则itemEntity为剩余物品
    /// </summary>
    /// <param name="itemEntity"></param>
    /// <returns></returns>
    private bool AddItemInternal(ItemEntity itemEntity)
    {
        var itemStackMax = itemEntity.ItemPrototype.stackMax;
        for (int i = 0; i < items.Length; i++)
        {
            ItemEntity curItem = items[i];
            if (curItem == null)
            {
                
                if (itemEntity.itemCount <= itemStackMax)
                {
                    items[i] = itemEntity;
                    return true;
                }
                else
                {
                    ItemEntity newItem = new ItemEntity(itemEntity, itemStackMax);
                    itemEntity.Cost(itemStackMax);
                    items[i] = newItem;
                }
            }
            else if(curItem.ItemEqual(itemEntity))
            {
                int surplusCount = curItem.Add(itemEntity.itemCount);
                if (surplusCount > 0)
                {
                    itemEntity.itemCount = surplusCount;
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    public ItemEntity AddItem(ItemEntity itemEntity)
    {
        ItemEntity lastItem = itemEntity;
        if (!AddItemInternal(lastItem))
        {
            return lastItem;
        }
        return null;
    }

    public ItemEntity AddItem(ItemPrototype itemPrototype, int itemNum = 1)
    {
        if (itemPrototype != null)
        {
            ItemEntity itemEntity = new ItemEntity(itemPrototype, itemNum);
            return AddItem(itemEntity);
        }

        return null;
    }

    public ItemEntity AddItem(string itemID, int itemNum = 1)
    {
        ItemPrototype itemPrototype = ItemManager.Instance.GetItemPrototype(itemID);
        return AddItem(itemPrototype,itemNum);
    }

    /// <summary>
    /// 物品相同时返回多余物品
    /// 物品不同时返回原有位置物品
    /// </summary>
    /// <param name="index"></param>
    /// <param name="itemEntity"></param>
    /// <returns></returns>
    public ItemEntity AddItemToSlot(int index,ItemEntity itemEntity)
    {
        ItemEntity oldItem = items[index];
        if (oldItem != null)
        {
            if (oldItem.ItemEqual(itemEntity))
            {
                int surplusCount = oldItem.Add(itemEntity.itemCount);
                if (surplusCount > 0)
                {
                    itemEntity.itemCount = surplusCount;
                    return itemEntity;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                items[index] = itemEntity;
                return oldItem;
            }
        }
        else
        {
            items[index] = itemEntity;
            return null;
        }
    }

    /// <summary>
    /// 返回被移除的物品
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ItemEntity RemoveItemFromSlot(int index)
    {
        ItemEntity oldItem = items[index];
        items[index] = null;
        return oldItem;
    }

    /// <summary>
    /// 返回被移除的物品
    /// </summary>
    /// <param name="index"></param>
    /// <param name="count">要移除的数量</param>
    /// <returns></returns>
    public ItemEntity RemoveItemFromSlot(int index,int count)
    {
        ItemEntity oldItem = items[index];
        if (count >= oldItem.itemCount)
        {
            items[index] = null;
            return oldItem;
        }
        else
        {
            ItemEntity newItem = oldItem.Clone(count);
            oldItem.Cost(count);
            return newItem;
        }
    }

    public ItemEntity GetItemFromSlot(int index)
    {
        ItemEntity oldItem = items[index];
        return oldItem;
    }
}
