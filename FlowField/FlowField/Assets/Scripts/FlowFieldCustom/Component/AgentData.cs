using Unity.Entities;

public enum RvoType
{
    AgentMoving,
    AgentReached,
}

public struct AgentData : IComponentData
{
    public byte Camp;
    public float Radius;
    public RvoType PriorityType;
}