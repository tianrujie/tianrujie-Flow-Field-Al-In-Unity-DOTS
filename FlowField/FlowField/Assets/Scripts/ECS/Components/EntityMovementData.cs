using Unity.Entities;
using Unity.Mathematics;

namespace TMG.ECSFlowField
{
    public struct EntityMovementData : IComponentData
    {
        public float moveSpeed;
        public float destinationMoveSpeed;
        public bool destinationReached;
        public float3 curSpeed;
    }
}