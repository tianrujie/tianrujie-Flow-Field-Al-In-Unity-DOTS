using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public static class SharedDataContainer 
{
    public static NativeHashMap<int,CellData> Cells;
    public static Dictionary<int, Transform> BlocksShow;
    public static NativeList<BlockItem>[][] Blocks;
    public static int TargetCell;
    public static int TargetCellVersion;
    
    public static void Init()
    {
        //@todo NativeHashMap`s write use memcopy, it`s too expensive,maybe we need try native array?
        Cells = new NativeHashMap<int, CellData>(Const1.MapRange.x * Const1.MapRange.y, Allocator.Persistent);
        BlocksShow = new Dictionary<int, Transform>();
        TargetCell = 0;
        TargetCellVersion = 0;
        
    }
    
    public static void InitMapGrid()
    {
        for (byte row = 0; row < Const1.MapCells.y; row++)
        {
            for (byte col = 0; col < Const1.MapCells.x; col++)
            {
                var cell = new CellData();
                var index = Index2To1(col, row);
                cell.Index = index;
                cell.I_2T = new int2(col,row);
                cell.WorldPos = WorldPos(col,row).xz;
                cell.Cost = 1;
                cell.BestCost = int.MaxValue;
                cell.TargetCellVersion = 0;
                Cells.TryAdd(index,cell);
            }
        }
        
        Blocks = new NativeList<BlockItem>[2][];
        for (var i = 0; i < Blocks.Length; i++)
        {
            Blocks[i] = new NativeList<BlockItem>[Const1.NumBlocks];
            for (var j = 0; j < Const1.NumBlocks; j++)
            {
                Blocks[i][j] = new NativeList<BlockItem>(4, Allocator.Persistent);
            }
        }
    }

    public static int Index2To1(int col, int row)
    {
        if (col < 0 || col >= Const1.MapCells.x || row < 0 || row >= Const1.MapCells.y)
            return -1;
        
        return row * Const1.MapCells.x + col;
    }
    
    public static int2 Index1To2(int idx)
    {
        return new int2(idx % Const1.MapCells.x, idx / Const1.MapCells.x);
    }

    public static float3 WorldPos(int col, int row)
    {
        return new float3((col + 0.5f) * Const1.MapCellSize - Const1.MapRange.x / 2, 0, (row + 0.5f) * Const1.MapCellSize - Const1.MapRange.y / 2);
    }
    
    public static bool WorldPos2Idx(Vector3 worldPos, out int idx, out int2 idx2)
    {
        if (!PosLegal(worldPos))
        {
            idx = -1;
            idx2 = new int2(-1,-1);
            Debug.LogError($"{worldPos} not in map area!");
            return false;
        }
        
        idx2 = new int2((int)(worldPos.x + Const1.MapRange.x / 2) / Const1.MapCellSize, (int)((worldPos.z + Const1.MapRange.y / 2) / Const1.MapCellSize));
        idx = SharedDataContainer.Index2To1(idx2.x,idx2.y);
        return true;
    }

    public static void GetNeighbours(int2 idx2, ref NativeList<int> rt)
    {
        for (int i = 0; i < Const1.Neighbours4Dir.Length; i++)
        {
            var nIdx = idx2 + Const1.Neighbours4Dir[i];
            if (IdxLegal(nIdx))
                rt.Add(Index2To1(nIdx.x,nIdx.y));
        }
    }
    
    public static bool IdxLegal(int2 idx2)
    {
        return idx2.x >= 0 && idx2.x < Const1.MapCells.x &&
               idx2.y >= 0 && idx2.y < Const1.MapCells.y;
    }
    
    public static bool IdxLegal(int idx)
    {
        return idx >= 0 && idx < Const1.MapCells.x *  Const1.MapCells.y;
    }
    
    public static bool PosLegal(Vector3 pos)
    {
        return math.abs(pos.x) <= Const1.MapRange.x / 2 && math.abs(pos.y) <= Const1.MapRange.y / 2 ;
    }
    
    public static void Clear()
    {
        if (Cells.IsCreated)
            Cells.Dispose();
        
        if (Blocks != null)
        {
            for (var i = 0; i < Blocks.Length; i++)
            {
                for (var j = 0; j < Const1.NumBlocks; j++)
                {
                    if( Blocks[i][j].IsCreated)
                        Blocks[i][j].Dispose();
                }
            }
        }
    }

    public static void ClearBlock()
    {
        foreach (var block in BlocksShow)
        {
            var cellData = Cells[block.Key];
            cellData.IsBlock = false;
            Cells[block.Key] = cellData;
            GameObject.Destroy(block.Value.gameObject);
        }
        BlocksShow.Clear();
    }
    
    public static Vector3 TouchPos2WorldPoint(Vector3 mousePos)
    {
        Vector3 worldPos = Vector3.zero;
        var ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out var hit))
        {
            worldPos = hit.point;
        }

        return worldPos;
    }
    
    static string[] layerGroup = new string[1];
    static RaycastHit[] hits = new RaycastHit[1];

    public static Vector3 TouchPos2WorldPointByLayer(Vector3 mousePos, string layerName)
    {
        Vector3 worldPos = Vector3.positiveInfinity;
        layerGroup[0] = layerName;
        var ray = Camera.main.ScreenPointToRay(mousePos);
        int target = LayerMask.GetMask(layerGroup);
        if (Physics.RaycastNonAlloc(ray, hits, 500f, target) > 0)
        {
            if (hits[0].collider != null)
            {
                worldPos = hits[0].point;
            }
        }
        else
        {
            Debug.LogWarning("cant find layer " + layerName);
        }

        return worldPos;
    }
}
