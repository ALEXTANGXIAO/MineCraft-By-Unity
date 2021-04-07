using System.Collections;
using System.Collections.Generic;
using LitJson.Extensions;
using UnityEngine;

public class ItemEntity
{
    [JsonIgnore]
    private ItemPrototype itemPrototype;

    public string itemPrototypeID;
    public int itemCount;
    
    [JsonIgnore]
    public ItemPrototype ItemPrototype
    {
        get
        {
            if (itemPrototype == null)
            {
                itemPrototype = ItemManager.Instance.GetItemPrototype(itemPrototypeID);
            }

            return itemPrototype;
        }
    }

    public ItemEntity()
    {
        itemPrototypeID = "";
        itemPrototype = null;
        itemCount = 1;
    }

    public ItemEntity(string id) : this()
    {
        itemPrototypeID = id;
        itemPrototype = ItemManager.Instance.GetItemPrototype(id);
    }
    
    public ItemEntity(string id,int count) : this(id)
    {
        itemCount = count;
    }
    
    public ItemEntity(ItemPrototype itemPrototypePrototype) : this()
    {
        itemPrototypeID = itemPrototypePrototype.id;
        itemPrototype = itemPrototypePrototype;
    }
    
    public ItemEntity(ItemPrototype itemPrototypePrototype,int count) : this(itemPrototypePrototype)
    {
        itemCount = count;
    }
    
    public ItemEntity(ItemEntity itemEntity) : this()
    {
        itemPrototypeID = itemEntity.itemPrototypeID;
        itemPrototype = itemEntity.itemPrototype;
        itemCount = itemEntity.itemCount;
    }
    
    public ItemEntity(ItemEntity itemEntity,int count) : this()
    {
        itemPrototypeID = itemEntity.itemPrototypeID;
        itemPrototype = itemEntity.itemPrototype;
        itemCount = count;
    }

    public void Cost(int count)
    {
        itemCount = Mathf.Max(itemCount - count, 1);
    }

    public bool ItemEqual(ItemEntity itemEntity)
    {
        if (itemEntity == null)
            return false;
        if (itemEntity.itemPrototype == itemPrototype)
            return true;
        return false;
    }
    
    public int Add(int count)
    {
        int totalCount = count + itemCount;
        int itemStackMax = itemPrototype.stackMax;
        int surplusCount = -(itemStackMax - totalCount);
        if (surplusCount <= 0)
        {
            itemCount = totalCount;
            return 0;
        }
        else
        {
            itemCount = itemStackMax;
            return surplusCount;
        }
    }

    public ItemEntity Clone(int count = 0)
    {
        if (count > 0)
        {
            return new ItemEntity(itemPrototypeID, count);
        }
        else
        {
            return new ItemEntity(itemPrototypeID, itemCount);
        }
    }
}
