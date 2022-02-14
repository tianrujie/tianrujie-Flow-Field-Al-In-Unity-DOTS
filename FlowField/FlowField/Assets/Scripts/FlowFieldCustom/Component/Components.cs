using Unity.Entities;
using Unity.Mathematics;

public struct BlockItem
{
    public Entity Entity;
    public float2 Pos;

    public BlockItem(Entity entity, float2 pos)
    {
        Entity = entity;
        Pos = pos;
    }
}