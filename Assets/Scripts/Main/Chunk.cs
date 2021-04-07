using System;
using System.Collections;
using System.Collections.Generic;
using SkyFrameWork;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Threading;
using ThreadPriority = System.Threading.ThreadPriority;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public Vector3Int chunkSerial = new Vector3Int(0, 0, 0);
    public bool ready = false;

    /// <summary>
    /// 当needUpdate为true时，会自动调用多线程进行更新
    /// </summary>
    public bool needUpdate = false;

    public bool needSave = false;
    /// <summary>
    /// 是否初始化过
    /// </summary>
    [Disable]
    public bool isInit = false;
    
    private static readonly Color32 AlphaColor = new Color32(255,255,255,0);
    
    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Color32> colors = new List<Color32>();
    
    private List<int> trianglesSub = new List<int>();

    public Block[,,] map;

    private MeshCollider meshCollider;
    private MeshFilter meshFilter;
    private ChunkManager chunkManager;

    private Chunk frontChunk;
    private Chunk backChunk;
    private Chunk upChunk;
    private Chunk downChunk;
    private Chunk leftChunk;
    private Chunk rightChunk;

    private Thread thread;

    /// <summary>
    /// 当正在计算mesh或map时，该值为true
    /// </summary>
    private bool isLoading = false;

    void Awake()
    {
        chunkManager = ChunkManager.Instance;
        meshCollider = GetComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Start()
    {
        //ChunkSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        CheckChunk();
        if (ready && !isLoading)
        {
            StartCoroutine(CheckUpdate());
        }
    }
    
    private IEnumerator CheckUpdate()
    {
        if (needUpdate)
        {
            needUpdate = false;
            yield return StartCoroutine(CalculateMesh());
        }
    }

    private void OnDestroy()
    {
        if (thread != null)
        {
            thread.Abort();
            thread = null;
        }

        if (isInit == true && ready == false)
        {
            chunkManager.chunkLoadLocked = false;
        }
        chunkManager.chunks.Remove(chunkSerial);
    }

    public void CheckChunk()
    {
        if (WorldManager.Instance.playerIsNull)
        {
            return;
        }

        Vector3 transformPosition = WorldManager.Instance.player.transform.position;
        Vector3 position = transform.position;
        if (Vector2.Distance(new Vector2(transformPosition.x,transformPosition.z), 
                new Vector2(position.x,position.z)) >
            chunkManager.chunkHideDistance)
        {
            Destroy(this.gameObject);
        }
    }
    public Block SetBlock(Vector3 pos, Block block)
    {
        Vector3 localPos = pos;
        var floorToIntY = Mathf.FloorToInt(localPos.y);
        if (floorToIntY < 0 || floorToIntY >= chunkManager.height)
            return null;
        Block oldBlock = map[Mathf.FloorToInt(localPos.x), floorToIntY, Mathf.FloorToInt(localPos.z)];
        map[Mathf.FloorToInt(localPos.x), floorToIntY, Mathf.FloorToInt(localPos.z)] = block;
        if (localPos.x >= chunkManager.width - 1)
        {
            if (rightChunk == null)
            {
                rightChunk = chunkManager.GetChunk(chunkSerial + new Vector3Int(1, 0, 0));
                if (rightChunk != null)
                    rightChunk.needUpdate = true;
            }
            else
            {
                rightChunk.needUpdate = true;
            }
        }
        if (localPos.z >= chunkManager.length - 1)
        {
            if (frontChunk == null)
            {
                frontChunk = chunkManager.GetChunk(chunkSerial + new Vector3Int(0, 0, 1));
                if (frontChunk != null)
                    frontChunk.needUpdate = true;
            }
            else
            {
                frontChunk.needUpdate = true;
            }
        }
        /*if (localPos.y >= chunkManager.height - 1)
        {
            if (upChunk == null)
            {
                upChunk = chunkManager.GetChunk(chunkSerial + new Vector3Int(0, 1, 0));
                upChunk.needUpdate = true;
            }
            else
            {
                upChunk.needUpdate = true;
            }
        }*/
        if (localPos.x < 1f)
        {
            if (leftChunk == null)
            {
                leftChunk = chunkManager.GetChunk(chunkSerial + new Vector3Int(-1, 0, 0));
                if (leftChunk != null)
                    leftChunk.needUpdate = true;
            }
            else
            {
                leftChunk.needUpdate = true;
            }
        }
        if (localPos.z < 1f)
        {
            if (backChunk == null)
            {
                backChunk = chunkManager.GetChunk(chunkSerial + new Vector3Int(0, 0, -1));
                if (backChunk != null)
                    backChunk.needUpdate = true;
            }
            else
            {
                backChunk.needUpdate = true;
            }
        }
        /*if (localPos.y < 1f)
        {
            if (downChunk == null)
            {
                downChunk = chunkManager.GetChunk(chunkSerial + new Vector3Int(0, -1, 0));
                if (downChunk != null)
                    downChunk.needUpdate = true;
            }
            else
            {
                downChunk.needUpdate = true;
            }
        }*/
        needUpdate = true;
        return oldBlock;
    }

    public void ChunkSpawn()
    {
        if (isInit)
            return;
        isInit = true;
        StartCoroutine(CreateChunk());
    }
    
    private IEnumerator CreateChunk()
    {
        chunkManager.chunkLoadLocked = true;
        yield return StartCoroutine(CalculateMap());
        yield return StartCoroutine(CalculateMesh());
        chunkManager.chunkLoadLocked = false;
        ready = true;
    }
    
    private IEnumerator CalculateMap()
    {
        isLoading = true;
        thread = new Thread(CalculateMapValue);
        thread.Start();
        while (thread.IsAlive)
        {
            yield return null;
        }
        thread = null;
        isLoading = false;
    }

    private void CalculateMapValue()
    {
        map = chunkManager.GetOrCreateMap(chunkSerial,true);
    }

    private IEnumerator CalculateMeshSync()
    {
        if (isLoading)
            yield return null;
        mesh = new Mesh {name = "Chunk"};
        yield return null;
        CalculateMeshValue();
        CompleteMeshCreate();
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
    }
    
    /// <summary>
    /// 只计算mesh，不赋值
    /// </summary>
    private void CalculateMeshValue()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        trianglesSub = new List<int>();
        colors = new List<Color32>();
        uvs = new List<Vector2>();
        for (int x = 0; x < chunkManager.length; x++)
        {
            for (int y = 0; y < chunkManager.height; y++)
            {
                for (int z = 0; z < chunkManager.width; z++)
                {
                    if (map[x,y,z] == null)
                        continue;
                    Block block = map[x, y, z];
                    if (IsBlockTransparent(map[x,y,z],x+1,y,z))
                        AddCubeFront(x,y,z,block);
                    if (IsBlockTransparent(map[x,y,z],x-1,y,z))
                        AddCubeBack(x,y,z,block);
                    if (IsBlockTransparent(map[x,y,z],x,y,z-1))
                        AddCubeLeft(x,y,z,block);
                    if (IsBlockTransparent(map[x,y,z],x,y,z+1))
                        AddCubeRight(x,y,z,block);
                    if (IsBlockTransparent(map[x,y,z],x,y+1,z))
                        AddCubeTop(x,y,z,block);
                    if (IsBlockTransparent(map[x,y,z],x,y-1,z))
                        AddCubeBottom(x,y,z,block);
                }  
            }
        }
    }

    private void CompleteMeshCreate()
    {
        if (vertices.Count == 0 || mesh == null)
        {
            
            meshCollider.sharedMesh = null;
            meshFilter.mesh = null;
            mesh = null;
        }
        else
        {
            mesh.subMeshCount = 2;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles,0);
            mesh.SetUVs(0,uvs);
            mesh.SetTriangles(trianglesSub,1);
            mesh.SetColors(colors);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            meshCollider.sharedMesh = mesh;
            meshFilter.mesh = mesh;
        }
    }

    private bool IsBlockTransparent(Block self, int x, int y, int z)
    {
        Block block;
        if (x >= chunkManager.length || y >= chunkManager.height ||
            z >= chunkManager.width || x < 0 || y < 0 || z < 0)
        {
            block = chunkManager.GetTrueBlock(chunkSerial, x, y, z);
        }
        else
        {
            block = map[x, y, z];
        }

        if (block == null)
            return true;
        if (self.isTransparent && block.isTransparent && block.blockID == self.blockID)
            return false;
        return block.isTransparent;
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
