using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public static class SharedDataContainer 
{
    public static NativeHashMap<byte,CellData> Cells;
    public static int TargetCell;
    
    public static void Init()
    {
        Cells = new NativeHashMap<byte, CellData>(Const1.MapRange.x * Const1.MapRange.y, Allocator.Persistent);
    }
    
    public static void InitMapGrid()
    {
        for (byte col = 0; col < Const1.MapCells.x; col++)
        {
            for (byte row = 0; row < Const1.MapCells.y; row++)
            {
                var cell = new CellData();
                var index = Index2To1(col, row);
                cell.Index = index;
                cell.WorldPos = WorldPos(col,row).xz;
                Cells.TryAdd(index,cell);
            }
        }
    }

    public static byte Index2To1(byte col, byte row)
    {
        return (byte)(row * Const1.MapCells.x + col);
    }

    public static float3 WorldPos(byte col, byte row)
    {
        return new float3((col + 0.5f) * Const1.MapCellSize - Const1.MapRange.x / 2, 0, (row + 0.5f) * Const1.MapCellSize - Const1.MapRange.y / 2);
    }
    
    public static void Clear()
    {
        if (Cells.IsCreated)
            Cells.Dispose();
    }
}
