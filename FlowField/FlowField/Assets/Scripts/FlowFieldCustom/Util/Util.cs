using Unity.Mathematics;

public static class Util
{
    public static float3 ClampPos(float3 oriPos)
    {
        oriPos.x = math.clamp(oriPos.x, -Const1.MapRange.x / 2, Const1.MapRange.x / 2);
        oriPos.y = 0f;
        oriPos.z = math.clamp(oriPos.z, -Const1.MapRange.y / 2, Const1.MapRange.y / 2);
        return oriPos;
    }
    
    public static int GetBlockIndex(float2 pos)
    {
        var x = (int)math.clamp(pos.x - Const1.LevelMinX, 0, Const1.LevelCx - Const1.BorderError);
        var y = (int)math.clamp(pos.y - Const1.LevelMinY, 0, Const1.LevelCy - Const1.BorderError);
        return x / Const1.BlockSize * Const1.NumBlockRows + y / Const1.BlockSize;
    }
    
    public static float2 GetBlockCenter(float2 pos)
    {
        var x = (int)math.clamp(pos.x - Const1.LevelMinX, 0, Const1.LevelCx - 0.0001f);
        var y = (int)math.clamp(pos.y - Const1.LevelMinY, 0, Const1.LevelCy- 0.0001f);
        
        return new float2((x / Const1.BlockSize + 0.5f) * Const1.BlockSize + Const1.LevelMinX,
            (y / Const1.BlockSize + 0.5f) * Const1.BlockSize + Const1.LevelMinY);
    }
}