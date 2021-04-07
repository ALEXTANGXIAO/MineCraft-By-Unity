using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public class DropEntity : MonoBehaviour
{
    public ItemEntity bindItem;
    public MeshFilter meshFilter;
    
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private static readonly Color32 AlphaColor = new Color32(255,255,255,0);
    private List<int> trianglesSub = new List<int>();
    private List<Color32> colors = new List<Color32>();
    
   
    private ChunkManager chunkManager;
    
    private Thread thread;
    private bool isLoading = false;
    private bool ready = false;

    private Tweener anim1;
    private Tweener anim2;
    
    private void Awake()
    {
        chunkManager = ChunkManager.Instance;
    }

    private void Start()
    {
        
        anim1 = meshFilter.gameObject.transform.DORotate(
                new Vector3(0, 360, 0), 4f,RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetAutoKill(false);
        anim1.onComplete = () =>
        {
            anim1.Restart();
        };
        MeshYPingPong(0.2f,1f);
    }
    
    private void MeshYPingPong(float from, float to)
    {
        anim2 = meshFilter.gameObject.transform.DOLocalMoveY(to, 2f)
            .OnComplete(() => MeshYPingPong(to, from))
            .SetEase(Ease.InOutSine);
    }

    private void OnDestroy()
    {
        if (thread != null)
        {
            thread.Abort();
            thread = null;
        }
        anim1.Kill();
        anim2.Kill();
    }

    private void Update()
    {
        CheckDrop();
    }

    private void CheckDrop()
    {
        if(!ready)
            return;
        var distance = Vector3.Distance(transform.position,WorldManager.Instance.player.transform.position);
        if(distance > 2)
            return;
        ItemEntity lastItem = ItemManager.Instance.AddItemToPlayerInventory(bindItem);
        if (lastItem == null)
        {
            Destroy(gameObject);
        }
        else
        {
            bindItem = lastItem;
        }
    }
    
    public void InitDropItem(string bindItemID)
    {
        bindItem = new ItemEntity(bindItemID, 1);
        StartCoroutine(CalculateMesh());
    }
    
    public void InitDropItem(ItemEntity bindItemEntity)
    {
        bindItem = bindItemEntity;
        StartCoroutine(CalculateMesh());
    }
    
    private IEnumerator CalculateMesh()
    {
        mesh = new Mesh {name = "Chunk"};
        isLoading = true;
        thread = new Thread(CalculateMeshValue);
        thread.Start();
        while (thread.IsAlive)
        {
            yield return null;
        }
        thread = null;
        CompleteMeshCreate();
        isLoading = false;
        yield return new WaitForSeconds(0.5f);
        ready = true;
    }
    
    /// <summary>
    /// 只计算mesh，不赋值
    /// </summary>
    private void CalculateMeshValue()
    {
        Block bindBlock = BlockManager.Instance.GetBlock(bindItem.ItemPrototype.bindBlock);
        vertices = new List<Vector3>();
        triangles = new List<int>();
        trianglesSub = new List<int>();
        colors = new List<Color32>();
        uvs = new List<Vector2>();
        AddCubeFront(0.5f, 0, -0.5f, bindBlock);
        AddCubeBack(0.5f, 0, -0.5f, bindBlock);
        AddCubeLeft(0.5f, 0, -0.5f, bindBlock);
        AddCubeRight(0.5f, 0, -0.5f, bindBlock);
        AddCubeTop(0.5f, 0, -0.5f, bindBlock);
        AddCubeBottom(0.5f, 0, -0.5f, bindBlock);
    }

    private void CompleteMeshCreate()
    {
        if (vertices.Count == 0 || mesh == null)
        {
            meshFilter.mesh = null;
            mesh = null;
        }
        else
        {
            mesh.subMeshCount = 2;
            mesh.vertices = vertices.ToArray();
            mesh.SetTriangles(triangles.ToArray(),0);
            mesh.SetUVs(0,uvs.ToArray());
            mesh.SetTriangles(trianglesSub.ToArray(),1);
            mesh.SetColors(colors);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
        }
    }

    #region 创建方块六个面

    private void AddCubeFront(float x,float y,float z,Block block)
    {
        int verticeOffset = vertices.Count;
        // 坐标与Unity坐标一致，后x右z上y
        vertices.Add(new Vector3(0 + x,0 + y,0 + z));
        vertices.Add(new Vector3(0 + x,0 + y,1 + z));
        vertices.Add(new Vector3(0 + x,1 + y,1 + z));
        vertices.Add(new Vector3(0 + x,1 + y,0 + z));
        
        // 第一个三角形顶点
        triangles.Add(0 + verticeOffset);
        triangles.Add(2 + verticeOffset);
        triangles.Add(1 + verticeOffset);
        // 第二个三角形顶点
        triangles.Add(0 + verticeOffset);
        triangles.Add(3 + verticeOffset);
        triangles.Add(2 + verticeOffset);
        
        // 贴上UV
        uvs.Add(new Vector2(block.textureFrontX * chunkManager.textureUVOffsetX, 
            block.textureFrontY * chunkManager.textureUVOffsetY) );
        uvs.Add(new Vector2(block.textureFrontX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureFrontY * chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureFrontX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureFrontY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureFrontX * chunkManager.textureUVOffsetX,
            block.textureFrontY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));
        
        
        // 特殊UV
        if (!block.colorF.Equals(AlphaColor))
        {
            // // 第一个三角形顶点
            trianglesSub.Add(0 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            trianglesSub.Add(1 + verticeOffset);
            // // 第二个三角形顶点
            trianglesSub.Add(0 + verticeOffset);   
            trianglesSub.Add(3 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            colors.Add(block.colorF);
            colors.Add(block.colorF);
            colors.Add(block.colorF);
            colors.Add(block.colorF);
        }
        else
        {
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
        }
    }
    
    private void AddCubeBack(float x,float y,float z,Block block)
    {
        int verticeOffset = vertices.Count;
        // 坐标与Unity坐标一致，后x右z上y
        vertices.Add(new Vector3(-1 + x,0 + y,1 + z));
        vertices.Add(new Vector3(-1 + x,0 + y,0 + z));
        vertices.Add(new Vector3(-1 + x,1 + y,0 + z));
        vertices.Add(new Vector3(-1 + x,1 + y,1 + z));
        
        // 第一个三角形顶点
        triangles.Add(0 + verticeOffset);
        triangles.Add(2 + verticeOffset);
        triangles.Add(1 + verticeOffset);
        // 第二个三角形顶点
        triangles.Add(0 + verticeOffset);
        triangles.Add(3 + verticeOffset);
        triangles.Add(2 + verticeOffset);
        
        // 贴上UV
        uvs.Add(new Vector2(block.textureBackX * chunkManager.textureUVOffsetX, 
            block.textureBackY * chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureBackX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureBackY * chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureBackX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureBackY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureBackX * chunkManager.textureUVOffsetX,
            block.textureBackY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));
        
        // 特殊UV
        if (!block.colorB.Equals(AlphaColor))
        {
            // // 第一个三角形顶点
            trianglesSub.Add(0 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            trianglesSub.Add(1 + verticeOffset);
            // // 第二个三角形顶点
            trianglesSub.Add(0 + verticeOffset);   
            trianglesSub.Add(3 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            colors.Add(block.colorB);
            colors.Add(block.colorB);
            colors.Add(block.colorB);
            colors.Add(block.colorB);
        }
        else
        {
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
        }
    }
    
    private void AddCubeLeft(float x,float y,float z,Block block)
    {
        int verticeOffset = vertices.Count;
        // 坐标与Unity坐标一致，后x右z上y
        vertices.Add(new Vector3(-1 + x,0 + y,0 + z));
        vertices.Add(new Vector3(0 + x,0 + y,0 + z));
        vertices.Add(new Vector3(0 + x,1 + y,0 + z));
        vertices.Add(new Vector3(-1 + x,1 + y,0 + z));
        
        // 第一个三角形顶点
        triangles.Add(0 + verticeOffset);
        triangles.Add(2 + verticeOffset);
        triangles.Add(1 + verticeOffset);
        // 第二个三角形顶点
        triangles.Add(0 + verticeOffset);
        triangles.Add(3 + verticeOffset);
        triangles.Add(2 + verticeOffset);
        
        // 贴上UV
        uvs.Add(new Vector2(block.textureLeftX * chunkManager.textureUVOffsetX, 
            block.textureLeftY * chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureLeftX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureLeftY * chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureLeftX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureLeftY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureLeftX * chunkManager.textureUVOffsetX,
            block.textureLeftY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));
        
        // 特殊UV
        if (!block.colorL.Equals(AlphaColor))
        {
            // // 第一个三角形顶点
            trianglesSub.Add(0 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            trianglesSub.Add(1 + verticeOffset);
            // // 第二个三角形顶点
            trianglesSub.Add(0 + verticeOffset);   
            trianglesSub.Add(3 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            colors.Add(block.colorL);
            colors.Add(block.colorL);
            colors.Add(block.colorL);
            colors.Add(block.colorL);
        }
        else
        {
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
        }
    }
    
    private void AddCubeRight(float x,float y,float z,Block block)
    {
        int verticeOffset = vertices.Count;
        // 坐标与Unity坐标一致，后x右z上y
        vertices.Add(new Vector3(0 + x,0 + y,1 + z));
        vertices.Add(new Vector3(-1 + x,0 + y,1 + z));
        vertices.Add(new Vector3(-1 + x,1 + y,1 + z));
        vertices.Add(new Vector3(0 + x,1 + y,1 + z));

        // 第一个三角形顶点
        triangles.Add(0 + verticeOffset);
        triangles.Add(2 + verticeOffset);
        triangles.Add(1 + verticeOffset);
        // 第二个三角形顶点
        triangles.Add(0 + verticeOffset);   
        triangles.Add(3 + verticeOffset);
        triangles.Add(2 + verticeOffset);
        
        // 贴上UV
        uvs.Add(new Vector2(block.textureRightX * chunkManager.textureUVOffsetX, 
            block.textureRightY * chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureRightX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureRightY * chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureRightX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureRightY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureRightX * chunkManager.textureUVOffsetX,
            block.textureRightY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));

        // 特殊UV
        if (!block.colorR.Equals(AlphaColor))
        {
            // // 第一个三角形顶点
            trianglesSub.Add(0 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            trianglesSub.Add(1 + verticeOffset);
            // // 第二个三角形顶点
            trianglesSub.Add(0 + verticeOffset);   
            trianglesSub.Add(3 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            colors.Add(block.colorR);
            colors.Add(block.colorR);
            colors.Add(block.colorR);
            colors.Add(block.colorR);
        }
        else
        {
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
        }
    }
    
    private void AddCubeTop(float x,float y,float z,Block block)
    {
        int verticeOffset = vertices.Count;
        // 坐标与Unity坐标一致，后x右z上y
        vertices.Add(new Vector3(0 + x,1 + y,0 + z));
        vertices.Add(new Vector3(0 + x,1 + y,1 + z));
        vertices.Add(new Vector3(-1 + x,1 + y,1 + z));
        vertices.Add(new Vector3(-1 + x,1 + y,0 + z));

        // 第一个三角形顶点
        triangles.Add(0 + verticeOffset);
        triangles.Add(2 + verticeOffset);
        triangles.Add(1 + verticeOffset);
        // 第二个三角形顶点
        triangles.Add(0 + verticeOffset);   
        triangles.Add(3 + verticeOffset);
        triangles.Add(2 + verticeOffset);
        
        // 贴上UV
        uvs.Add(new Vector2(block.textureTopX * chunkManager.textureUVOffsetX, 
            block.textureTopY * chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureTopX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureTopY * chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureTopX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureTopY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureTopX * chunkManager.textureUVOffsetX,
            block.textureTopY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));

        // 特殊UV
        if (!block.colorTop.Equals(AlphaColor))
        {
            // // 第一个三角形顶点
            trianglesSub.Add(0 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            trianglesSub.Add(1 + verticeOffset);
            // // 第二个三角形顶点
            trianglesSub.Add(0 + verticeOffset);   
            trianglesSub.Add(3 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            colors.Add(block.colorTop);
            colors.Add(block.colorTop);
            colors.Add(block.colorTop);
            colors.Add(block.colorTop);
        }
        else
        {
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
        }
    }
    
    private void AddCubeBottom(float x,float y,float z,Block block)
    {
        int verticeOffset = vertices.Count;
        // 坐标与Unity坐标一致，后x右z上y
        vertices.Add(new Vector3(-1 + x,0 + y,0 + z));
        vertices.Add(new Vector3(-1 + x,0 + y,1 + z));
        vertices.Add(new Vector3(0 + x,0 + y,1 + z));
        vertices.Add(new Vector3(0 + x,0 + y,0 + z));

        // 第一个三角形顶点
        triangles.Add(0 + verticeOffset);
        triangles.Add(2 + verticeOffset);
        triangles.Add(1 + verticeOffset);
        // 第二个三角形顶点
        triangles.Add(0 + verticeOffset);   
        triangles.Add(3 + verticeOffset);
        triangles.Add(2 + verticeOffset);

        // 贴上UV
        uvs.Add(new Vector2(block.textureBottomX * chunkManager.textureUVOffsetX, 
            block.textureBottomY * chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureBottomX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureBottomY * chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureBottomX * chunkManager.textureUVOffsetX + chunkManager.textureUVOffsetX,
            block.textureBottomY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));
        uvs.Add(new Vector2(block.textureBottomX * chunkManager.textureUVOffsetX,
            block.textureBottomY * chunkManager.textureUVOffsetY + chunkManager.textureUVOffsetY));
        
        // 特殊UV
        if (!block.colorBottom.Equals(AlphaColor))
        {
            // // 第一个三角形顶点
            trianglesSub.Add(0 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            trianglesSub.Add(1 + verticeOffset);
            // // 第二个三角形顶点
            trianglesSub.Add(0 + verticeOffset);   
            trianglesSub.Add(3 + verticeOffset);
            trianglesSub.Add(2 + verticeOffset);
            colors.Add(block.colorBottom);
            colors.Add(block.colorBottom);
            colors.Add(block.colorBottom);
            colors.Add(block.colorBottom);
        }
        else
        {
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
            colors.Add(AlphaColor);
        }
    }

    #endregion
}
