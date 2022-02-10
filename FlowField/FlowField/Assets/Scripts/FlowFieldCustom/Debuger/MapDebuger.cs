using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;

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

        GUIStyle style = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter};
        
        switch (debugTarget)
        {
            case DebugTarget.Cost:
                
                break;
            case DebugTarget.BestCost:
                for (int idx = 0; idx < Const1.MapCells.x * Const1.MapCells.y; idx++)
                {
                    var cellData = SharedDataContainer.Cells[idx];
                    if (cellData.IsBlock)
                        continue;
                    Handles.Label( new Vector3(cellData.WorldPos.x,0,cellData.WorldPos.y), cellData.BestCost.ToString(), style);
                }
               
                break;
            case DebugTarget.BestDirection:
                for (int idx = 0; idx < Const1.MapCells.x * Const1.MapCells.y; idx++)
                {
                    var cellData = SharedDataContainer.Cells[idx];
                    if (cellData.IsBlock)
                        continue;
                    var center3T = new Vector3(cellData.WorldPos.x,0,cellData.WorldPos.y);
                    var bDir = ((Vector2)cellData.BestDir).normalized;
                    Handles.Label( center3T, cellData.BestCost.ToString(), style);
                    
                    var dir3T = new Vector3(bDir.x,0,-bDir.y);
                    var offset = dir3T.normalized * Const1.MapCellSize / 2;
                    
                    var pos1 = center3T + offset;
                    var pos2 = center3T; //- offset / 2
                    Handles.DrawLine(pos2 ,pos1, 1);
                    //Handles.DrawDottedLine(pos1, pos2,2);
                }
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
        for (byte row = 0; row < drawGridSize.y; row++)
        {
            for (byte col = 0; col < drawGridSize.x; col++)
            {
                Vector3 center = SharedDataContainer.WorldPos(col,row);
                Vector3 size = Vector3.one * Const1.MapCellSize;
                size.y = 0.01f;
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
