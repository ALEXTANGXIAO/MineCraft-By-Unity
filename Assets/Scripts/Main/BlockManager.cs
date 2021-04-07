using System;
using System.Collections;
using System.Collections.Generic;
using SkyFrameWork;
using UnityEngine;

public class BlockManager : SingletonMono<BlockManager>
{
    public Dictionary<string, Block> blocks = new Dictionary<string, Block>();

    protected override void Awake()
    {
        instance = this;
        Initialized();
    }

    public void Initialized()
    {
        blocks.Add("Dirt",new Block("Dirt",2,15));
        blocks.Add("Stone",new Block("Stone",1,15));
        blocks.Add("Sand",new Block("Sand",2,14));
        blocks.Add("RoundStone",new Block("RoundStone",0,14));
        blocks.Add("BedRock",new Block("BedRock",1,14));
        blocks.Add("Grass", new Block("Grass", 3, 15, 
            0, 15, 2, 15)
            .SetNeedColor(Block.AlphaColor,
                Block.AlphaColor,
                Block.AlphaColor,
                Block.AlphaColor,
                new Color32(70,180,70,128),
                Block.AlphaColor));
        blocks.Add("Glass", new Block("Glass", 1, 12)
            .SetTransparent(true));

        blocks.Add("OakLog", new Block("OakLog", 4, 14,
            5, 14, 5, 14));
        blocks.Add("OakLeaves",new Block("OakLeaves",5,12)
            .SetNeedColor(new Color32(28, 71, 9, 128)));
    }

    public Block GetBlock(string id)
    {
        blocks.TryGetValue(id, out Block block);
        return block;
    }
    
    public bool TryGetBlock(string id,out Block block)
    {
        return blocks.TryGetValue(id, out block);
    }

    public BlockMapData CreateBlockMap()
    {
        BlockMapData blockMapData = new BlockMapData();
        blockMapData.blockMap = new Dictionary<string, string>();
        int i = 0;
        foreach (var item in blocks)
        {
            blockMapData.blockMap.Add(i.ToString(),item.Value.blockID);
            i++;
        }

        return blockMapData;
    }
    
    public void UpdateBlockMap(BlockMapData blockMapData)
    {
        if (blockMapData.blockMap == null)
        {
            blockMapData.blockMap = new Dictionary<string, string>();
        }
        int i = 0;
        foreach (var item in blocks)
        {
            if(blockMapData.blockMap.ContainsKey(i.ToString()))
            {
                i++;
                continue;
            }
            if(blockMapData.blockMap.ContainsValue(item.Value.blockID))
                continue;
            i++;
        }
    }
    
    /// <summary>
    /// 翻转键值对
    /// </summary>
    /// <param name="blockMapData"></param>
    /// <returns></returns>
    public Dictionary<string, string> GetBlockMapOverturn(BlockMapData blockMapData)
    {
        Dictionary<string, string> mapOverturn = new Dictionary<string, string>();
        foreach (var item in blockMapData.blockMap)
        {
           mapOverturn.Add(item.Value,item.Key);
        }

        return mapOverturn;
    }
}
