using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct CellData
{
    public byte Index;
    public float2 WorldPos;
    public byte Cost;
    public byte BestCost;
    public float2 BestDir;
}
