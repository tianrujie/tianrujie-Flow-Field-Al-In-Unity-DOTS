using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MapDebuger : MonoBehaviour
{
    public enum DebugTarget
    {
        None,
        Cost,
        BestCost,
        BestDirection,
        CostHeatMap,
    }
    
    public DebugTarget debugTarget;
    public bool drawGrid;
    private void OnDrawGizmos()
    {
        if (!SharedDataContainer.Cells.IsCreated)
            return;
        
        if (drawGrid)
        {
            DrawGrid(Const1.MapCells,Color.red);
        }

        switch (debugTarget)
        {
            case DebugTarget.Cost:
                
                break;
            case DebugTarget.BestCost:
                
                break;
            case DebugTarget.BestDirection:
                
                break;
            case DebugTarget.CostHeatMap:
                
                break;
            default:
                
                break;;
        }
    }
    
    private void DrawGrid(int2 drawGridSize, Color drawColor)
    {
        Gizmos.color = drawColor;
        for (byte col = 0; col < drawGridSize.x; col++)
        {
            for (byte row = 0; row < drawGridSize.y; row++)
            {
                Vector3 center = SharedDataContainer.WorldPos(col,row);
                Vector3 size = Vector3.one * Const1.MapCellSize;
                size.y = 0.01f;
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
