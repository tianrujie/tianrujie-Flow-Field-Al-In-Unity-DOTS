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
}