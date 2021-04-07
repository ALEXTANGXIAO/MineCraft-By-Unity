using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ItemPrototype
{
    [SerializeField]
    public string id;
    [SerializeField]
    public Sprite icon;
    [SerializeField]
    public string bindBlock;
    [SerializeField]
    [Range(1, 1024)]
    public int stackMax = 64;
    
    public ItemPrototype()
    {
        
    }
    
    public ItemPrototype(string id)
    {
        this.id = id;
    }

    public ItemPrototype SetIcon(Sprite tagTexture)
    {
        icon = tagTexture;
        return this;
    }
}
