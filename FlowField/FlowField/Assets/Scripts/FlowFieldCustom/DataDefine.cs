using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct CellData
{
    public int Index;
    public int2 I_2T;     //(x:col,y:row)
    public float2 WorldPos;
    public int Cost;
    public int BestCost;
    public int TargetCellVersion;
    public float2 BestDir;
}
