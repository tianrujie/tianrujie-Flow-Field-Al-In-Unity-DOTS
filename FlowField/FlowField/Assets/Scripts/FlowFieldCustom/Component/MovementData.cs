using Unity.Entities;
using Unity.Mathematics;

public struct MovementData : IComponentData
{
    public float moveSpeed;
    public float destinationMoveSpeed;
    public bool destinationReached;
    public int destinationVersion;
    public float2 curSpeed;
}