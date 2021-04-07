using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float viewRangeXZ = 64f;
    //public float viewRangeY = 32f;
    public Transform player;
    public Transform mainCam;
    public float playerSpeed = 0.01f;
    public LayerMask rayCastMask;
    public GameObject hightBlock;

    public float dropForce = 10f;
    
    public bool allowPlayerControl = true;
    
    private Quaternion screenMovementSpace;
    private Vector3  screenMovementForward, screenMovementRight;

    private float checkInterval = 1f;
    private float checkPassTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        BindPlayer();
        UpdateChunks();
    }

    // Update is called once per frame
    void Update()
    {
        if(allowPlayerControl)
        {
            PlayerMove();
            BlockController();
        }
        UIController();
        CheckUpdate();
        TestController();
    }

    private void UIController()
    {
        var uiManager = UIManager.Instance;
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (uiManager.isInventoryPanelNotNull)
            {
                if (uiManager.inventoryPanel.IsOpen)
                {
                    uiManager.ShowMainPanel();
                }
                else
                {
                    uiManager.ShowInventoryPanel();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (uiManager.inventoryPanel.IsOpen || uiManager.pausePanel.IsOpen)
            {
                uiManager.ShowMainPanel();
            }
            else
            {
                uiManager.ShowPausePanel();
            }
        }

        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                uiManager.toolBarPanel.SwitchCursor(i);
            }
        }

        float mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScrollWheel > 0)
        {
            uiManager.toolBarPanel.CurSelectIndex -= 1;
        }
        else if (mouseScrollWheel < 0)
        {
            uiManager.toolBarPanel.CurSelectIndex += 1;
        }
    }

    private void BindPlayer()
    {
        if (mainCam == null)
        {
            if (!(Camera.main is null)) mainCam = Camera.main.transform;
        }
        if (player == null)
        {
            player = this.transform;
        }
    }

    private void CheckUpdate()
    {
        if (!ChunkManager.Instance.enableChunkLoad)
            return;
        checkPassTime += Time.deltaTime;
        if (checkPassTime > checkInterval)
        {
            checkPassTime = 0f;
            UpdateChunks();
            
        }
    }
    
    private void UpdateChunks()
    {
        var chunkManager = ChunkManager.Instance;
        //int yChunkCount = chunkManager.heightMax / chunkManager.height;
        for (float x = transform.position.x - viewRangeXZ * chunkManager.chunkSize.x; x <  transform.position.x + viewRangeXZ* chunkManager.chunkSize.x; x += chunkManager.length * chunkManager.chunkSize.x)
        {
            // for (float y = transform.position.y - viewRangeY; y <  transform.position.y + viewRangeY; y += chunkManager.width)
            // {
                for (float z = transform.position.z - viewRangeXZ* chunkManager.chunkSize.z; z <  transform.position.z + viewRangeXZ* chunkManager.chunkSize.z; z += chunkManager.width* chunkManager.chunkSize.z)
                {
                    Vector3Int chunkSerial = chunkManager.PositionToChunkSerial(new Vector3(x,0,z));
                    // if (chunkSerial.y < 0 || chunkSerial.y >= yChunkCount)
                    //     continue;
                    if (!chunkManager.HasChunk(chunkSerial))
                        chunkManager.CreatChunk(chunkSerial);
                }
            // }
        }
    }
    
    private void PlayerMove()
    {
        //get movement axis relative to camera
        screenMovementSpace = Quaternion.Euler (0, mainCam.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace * Vector3.forward;
        screenMovementRight = screenMovementSpace * Vector3.right;

        float h = Input.GetAxisRaw ("Horizontal");
        float v = Input.GetAxisRaw ("Vertical");
        float f = Input.GetAxisRaw ("Flight");

        var position = player.position;
        position += screenMovementForward * (playerSpeed * Time.deltaTime * v);
        position += screenMovementRight * (playerSpeed * Time.deltaTime * h);
        position += Vector3.up * (playerSpeed * Time.deltaTime * f);
        player.position = position;
    }

    private void BlockController()
    {
        PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
        if (Physics.Raycast(mainCam.position, mainCam.forward, out var hitInfo,10f,rayCastMask))
        {
            ChunkManager chunkManager = ChunkManager.Instance;
            Vector3 hitInfoPoint = hitInfo.point - hitInfo.normal/2;
            hitInfoPoint = new Vector3(Mathf.CeilToInt(hitInfoPoint.x),
                Mathf.FloorToInt(hitInfoPoint.y), Mathf.FloorToInt(hitInfoPoint.z));
            hightBlock.SetActive(true);
            hightBlock.transform.position = hitInfoPoint;
            Block oldBlock = null;
            if (Input.GetMouseButtonDown(1))
            {
                hitInfoPoint = hitInfoPoint + hitInfo.normal;
                Vector3Int chunkSerial = ChunkManager.Instance.PositionToChunkSerial(hitInfoPoint);
                Chunk chunk = ChunkManager.Instance.GetChunk(chunkSerial);
                
                int curSelectIndex = UIManager.Instance.toolBarPanel.CurSelectIndex;
                ItemEntity curSelEntity =
                    playerInventory.toolBarItems.GetItemFromSlot(curSelectIndex);
                if (curSelEntity != null)
                {
                    oldBlock = chunk.SetBlock(hitInfoPoint - new Vector3(chunkSerial.x * chunkManager.length,0,chunkSerial.z * chunkManager.width),
                        BlockManager.Instance.GetBlock(curSelEntity.ItemPrototype.bindBlock));
                    ItemManager.Instance.RemoveItemFromPlayerToolbar(curSelectIndex);
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                Vector3Int chunkSerial = ChunkManager.Instance.PositionToChunkSerial(hitInfoPoint);
                //Debug.Log($"{hitInfoPoint} {hightBlock.transform.position} {ChunkManager.Instance.GetNoiseValue( hitInfoPoint.x,hitInfoPoint.y,hitInfoPoint.z)}");
                Chunk chunk = ChunkManager.Instance.GetChunk(chunkSerial);
                oldBlock = chunk.SetBlock(hitInfoPoint - new Vector3(chunkSerial.x * chunkManager.length,0,chunkSerial.z * chunkManager.width),null);
            }

            if (oldBlock != null)
            {
                ItemPrototype itemPrototype = ItemManager.Instance.GetItemPrototype(oldBlock.blockID);
                if (itemPrototype != null)
                {
                    ItemManager.Instance.CreateDropEntity(itemPrototype.id, hitInfoPoint + new Vector3(-0.5f,0.5f,0.5f));
                }
            }
            //Debug.Log($"{hitInfo.point} {hitInfo.normal}");
            Debug.DrawLine(mainCam.position, hitInfo.point);
        }
        else
        {
            hightBlock.SetActive(false);
            Debug.DrawRay(mainCam.position, mainCam.forward * 100 , Color.red);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int curSelectIndex = UIManager.Instance.toolBarPanel.CurSelectIndex;
            ItemEntity curSelEntity =
                playerInventory.toolBarItems.GetItemFromSlot(curSelectIndex);
            if (curSelEntity != null)
            {
                ItemManager.Instance.RemoveItemFromPlayerToolbar(curSelectIndex);
                ItemManager.Instance.CreateDropEntity(curSelEntity.itemPrototypeID, player.position,mainCam.forward.normalized * dropForce);
            }
        }
    }

    private void TestController()
    {
        // if (Input.GetKeyDown(KeyCode.C))
        // {
        //     ChunkManager.Instance.ClearAllChunk();
        // }
        //
        // if (Input.GetKeyDown(KeyCode.L))
        // {
        //     WorldManager.Instance.SaveAllData();
        // }
        
    }
}
