using System;
using System.Collections;
using System.Collections.Generic;
using TMG.ECSFlowField;
using UnityEngine;

public class Director : MonoBehaviour
{
    private static Director _director;
    [SerializeField] private GameObject _blockPrefab;

    public static Director Instance
    {
        get { return _director; }
    }
    public void Awake()
    {
        _director = this;
        SharedDataContainer.Init();
        SharedDataContainer.InitMapGrid();
        ArchyType.Initalize();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int a = 8;
            int b = 1 << 3;
            Debug.Log(a&b);
            
            var worldPos = SharedDataContainer.TouchPos2WorldPointByLayer(Input.mousePosition,"Default");
            Debug.Log($"Click World Pos: {worldPos}");
            if (!SharedDataContainer.PosLegal(worldPos))
                return;
            
            if (SharedDataContainer.WorldPos2Idx(worldPos, out var idx, out var idx2))
            {
                if(SharedDataContainer.Cells[idx].IsBlock)
                    return;
                
                Debug.Log($"Clicked [{idx},{idx2}]");
                SharedDataContainer.TargetCell = idx;
                SharedDataContainer.TargetCellVersion++;
            }
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            var worldPos = SharedDataContainer.TouchPos2WorldPointByLayer(Input.mousePosition,"Default");
            Debug.Log($"Right Click World Pos: {worldPos}");
            if (SharedDataContainer.WorldPos2Idx(worldPos, out var idx, out var idx2))
            {
                var cellData = SharedDataContainer.Cells[idx];
                if(cellData.IsBlock)
                {
                    Debug.Log($"Clicked [{idx},{idx2}] and Clear Block");
                    cellData.IsBlock = false;
                    cellData.BestCost = int.MaxValue;
                    if (SharedDataContainer.BlocksShow.TryGetValue(idx, out var b))
                    {
                        Destroy(b.gameObject);
                        SharedDataContainer.BlocksShow.Remove(idx);
                    }
                }
                else
                {
                    Debug.Log($"Clicked [{idx},{idx2}] and Set Block");
                    cellData.IsBlock = true;
                    cellData.BestCost = Const1.BlockCost;
                    var blockObj = GameObject.Instantiate(_blockPrefab);
                    blockObj.name = $"Block[{idx},{idx2}]";
                    blockObj.transform.position = new Vector3(cellData.WorldPos.x,0,cellData.WorldPos.y);
                    blockObj.transform.localScale = new Vector3(Const1.MapCellSize,0.01f,Const1.MapCellSize);
                    SharedDataContainer.BlocksShow.Add(idx,blockObj.transform);
                }
                    
                SharedDataContainer.Cells[idx] = cellData;
                SharedDataContainer.TargetCellVersion++;
            }
        }
        
        //clear blocks
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SharedDataContainer.ClearBlock();
            SharedDataContainer.TargetCellVersion++;
        }
    }

    private void OnDestroy()
    {
        SharedDataContainer.Clear();
        SharedDataContainer.ClearBlock();
    }
}
