using System.Collections;
using System.Collections.Generic;
using SkyFrameWork;
using UnityEngine;

public class ItemManager : SingletonMono<ItemManager>
{
    public GameObject dropEntityPrefab;
    public GameObject dropEntityParent;
    [SerializeField]
    [HideInInspector]
    private List<ItemPrototype> itemCreator = new List<ItemPrototype>();

    private Dictionary<string, ItemPrototype> itemDic = new Dictionary<string, ItemPrototype>();
    protected override void Awake()
    {
        instance = this;
        LoadItemInfo();
    }

    public void LoadItemInfo()
    {
        itemDic = new Dictionary<string, ItemPrototype>();
        foreach (var item in itemCreator)
        {
            itemDic[item.id] = item;
        }
    }

    public ItemPrototype GetItemPrototype(string id)
    {
        itemDic.TryGetValue(id, out ItemPrototype itemPrototype);
        return itemPrototype;
    }

    public DropEntity CreateDropEntity(ItemEntity itemEntity,Vector3 pos = default,Vector3 force = default)
    {
        GameObject gameObject = Instantiate(dropEntityPrefab, dropEntityParent.transform);
        gameObject.transform.position = pos;
        DropEntity dropEntity = gameObject.GetComponent<DropEntity>();
        dropEntity.InitDropItem(itemEntity);
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        rigidbody.AddForce(force);
        return dropEntity;
    }
    
    public DropEntity CreateDropEntity(string itemID,Vector3 pos = default,Vector3 force = default)
    {
        GameObject gameObject = Instantiate(dropEntityPrefab, dropEntityParent.transform);
        gameObject.transform.position = pos;
        DropEntity dropEntity = gameObject.GetComponent<DropEntity>();
        dropEntity.InitDropItem(itemID);
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        rigidbody.AddForce(force,ForceMode.VelocityChange);
        return dropEntity;
    }

    public ItemEntity AddItemToPlayerInventory(ItemEntity itemEntity,bool dropOther = true)
    {
        var player = WorldManager.Instance.player;
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        ItemEntity lastItem = playerInventory.toolBarItems.AddItem(itemEntity);
        if (lastItem == null)
        {
            UIManager.Instance.RefreshUI();
            return null;
        }
        lastItem = playerInventory.inventoryItems.AddItem(itemEntity);
        if (lastItem == null)
        {
            UIManager.Instance.RefreshUI();
            return null;
        }
        if (dropOther)
        {
            for (int i = 0; i < lastItem.itemCount; i++)
            {
                CreateDropEntity(lastItem.itemPrototypeID, player.transform.position);
            }
        }
        UIManager.Instance.RefreshUI();
        return lastItem;
    }

    public ItemEntity RemoveItemFromPlayerToolbar(int index,int count = 1)
    {
        var player = WorldManager.Instance.player;
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        ItemEntity lastItem = playerInventory.toolBarItems.RemoveItemFromSlot(index, count);
        UIManager.Instance.RefreshUI();
        return lastItem;
    }
    
    
}
