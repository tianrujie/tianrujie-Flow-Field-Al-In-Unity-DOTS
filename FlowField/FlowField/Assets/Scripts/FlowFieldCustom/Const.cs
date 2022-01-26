using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Const1
{
    public static int2 MapRange = new int2(200,100);
    public static int MapCellSize = 10;
    public static int2 MapCells = new int2(MapRange.x /MapCellSize, MapRange.y / MapCellSize);
}
