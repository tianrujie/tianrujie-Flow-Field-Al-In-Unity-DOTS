using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Const1
{
    /*
     * about rvo
     */
    public static int2 MapRange = new int2(200,100);
    public const float LevelMinX = -100f;
    public const float LevelMaxX = LevelMinX + 200 ;
    public const float LevelMinY = -50f;
    public const float LevelMaxY = LevelMinY + 100;
    public const float LevelCx = LevelMaxX - LevelMinX;
    public const float LevelCy = LevelMaxY - LevelMinY;
    public const float BorderError = 0.0001f;
    public const int BlockSize = 5;
    public const int _mapRangeX = 200;
    public const int _mapRangeY = 100;
    public const int NumBlockColumns = _mapRangeX / BlockSize;
    public const int NumBlockRows = _mapRangeY / BlockSize;
    public const int NumBlocks = NumBlockRows * NumBlockColumns;
    
    public static readonly int2[] BlockCloseNeighbors =
    {
        new int2(-BlockSize,BlockSize),    new int2(0,BlockSize),    new int2(BlockSize,BlockSize), 
        new int2(-BlockSize,0),            new int2(0,0),            new int2(BlockSize,0),
        new int2(-BlockSize,-BlockSize),   new int2(0,-BlockSize),   new int2(BlockSize,-BlockSize), 
    };
    
    public const float TimeHorizon = 3.0f;
    public const float TimeHorizonBattlingUnit = 3.0f;
    public const float TimeHorizonHero = 3.0f;
    public const float TimeHorizonObstacle = 3.0f;
    
    
    /*
     * about FlowField
     */
    
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


    public const int BlockCost = 256;

    public const int BL = 1;
    public const int BR = 1<<1;
    public const int BT = 1<<2;
    public const int BB = 1<<3;
    
    /*
     * about AI
     */

    public const byte CAMP_A = 0;
    public const byte CAMP_B = 1;
}
