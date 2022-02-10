using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Const1
{
    public static int2 MapRange = new int2(200,100);
    public static int MapCellSize = 5;
    public static int2 MapCells = new int2(MapRange.x /MapCellSize, MapRange.y / MapCellSize);
    public static int2[] Neighbours4Dir = new int2[4]
    {
        new int2(-1,0),
        new int2(1,0),
        new int2(0,1),
        new int2(0,-1),
    };
    
    public static int2[] Neighbours8Dir = new int2[8]
    {
        new int2(-1,0),
        new int2(1,0),
        new int2(0,1),
        new int2(0,-1),
        new int2(-1,1),
        new int2(-1,-1),
        new int2(1,-1),
        new int2(1,1),
    };


    public static int BlockCost = 256;

}
