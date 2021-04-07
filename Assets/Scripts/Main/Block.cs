using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public static readonly Color32 AlphaColor = new Color32(255,255,255,0);
    
    public string blockID;
    public string name;

    // 前方贴图
    public int textureFrontX = 0;
    public int textureFrontY = 0;
    // 后方贴图
    public int textureBackX = 0;
    public int textureBackY = 0;
    // 左方贴图
    public int textureLeftX = 0;
    public int textureLeftY = 0;
    // 右方贴图
    public int textureRightX = 0;
    public int textureRightY = 0;
    // 上方贴图
    public int textureTopX = 0;
    public int textureTopY = 0;
    // 下方贴图
    public int textureBottomX = 0;
    public int textureBottomY = 0;

    
    public Color32 colorF = AlphaColor;
    public Color32 colorB = AlphaColor;
    public Color32 colorL = AlphaColor;
    public Color32 colorR = AlphaColor;
    public Color32 colorTop = AlphaColor;
    public Color32 colorBottom = AlphaColor;
    public bool isTransparent = false;
    public bool hasCollision = true;
    
    public Block(string id)
    {
        blockID = id;
        name = id;
    }

    public Block(string id, int textureFBLRTBX, int textureFBLRTBY) : this(id)
    {
        textureFrontX = textureFBLRTBX;
        textureFrontY = textureFBLRTBY;
        textureBackX = textureFBLRTBX;
        textureBackY = textureFBLRTBY;
        textureLeftX = textureFBLRTBX;
        textureLeftY = textureFBLRTBY;
        textureRightX = textureFBLRTBX;
        textureRightY = textureFBLRTBY;
        textureTopX = textureFBLRTBX;
        textureTopY = textureFBLRTBY;
        textureBottomX = textureFBLRTBX;
        textureBottomY = textureFBLRTBY;
    }
    
    public Block(string id, int textureFBLRX, int textureFBLRY, int textureTopX, int textureTopY, int textureBottomX, int textureBottomY) : this(id)
    {
        textureFrontX = textureFBLRX;
        textureFrontY = textureFBLRY;
        textureBackX = textureFBLRX;
        textureBackY = textureFBLRY;
        textureLeftX = textureFBLRX;
        textureLeftY = textureFBLRY;
        textureRightX = textureFBLRX;
        textureRightY = textureFBLRY;
        this.textureTopX = textureTopX;
        this.textureTopY = textureTopY;
        this.textureBottomX = textureBottomX;
        this.textureBottomY = textureBottomY;
    }

    public Block SetTransparent(bool setTransparent)
    {
        isTransparent = setTransparent;
        return this;
    }

    public Block SetNeedColor(Color32 setNeedColor)
    {
        colorF = setNeedColor;
        colorB = setNeedColor;
        colorL = setNeedColor;
        colorR = setNeedColor;
        colorTop = setNeedColor;
        colorBottom = setNeedColor;
        return this;
    }
    
    public Block SetNeedColor(Color32 setNeedColorF,Color32 setNeedColorB,Color32 setNeedColorL,
        Color32 setNeedColorR,Color32 setNeedColorTop,Color32 setNeedColorBottom)
    {
        colorF = setNeedColorF;
        colorB = setNeedColorB;
        colorL = setNeedColorL;
        colorR = setNeedColorR;
        colorTop = setNeedColorTop;
        colorBottom = setNeedColorBottom;
        return this;
    }
}
