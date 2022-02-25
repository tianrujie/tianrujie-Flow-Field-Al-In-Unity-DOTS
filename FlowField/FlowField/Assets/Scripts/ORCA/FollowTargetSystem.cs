using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[AlwaysUpdateSystem]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(RVO3System))]
public class FollowTargetSystem : ComponentSystem
{
    private EntityQuery _query;
    protected override void OnCreate()
    {
        _query = EntityManager.CreateEntityQuery(typeof(MovementData),typeof(AgentData), typeof(Translation), typeof(Rotation),typeof(ORCATag));
    }

    protected override void OnUpdate()
    {
        if (Director1.Ins == null || Director1.Ins.tarTransform == null)
            return;
        float3 tarPos = Director1.Ins.tarTransform.position;
        Entities.WithAll<ORCATag>().ForEach<MovementData,Translation,AgentData>((ref MovementData movement,ref Translation translation, ref AgentData agentData) =>
        {
            var offset = tarPos - translation.Value;
            var dir = math.normalizesafe(offset);
            var l = math.lengthsq(offset);
            if (l > (agentData.Radius * agentData.Radius * 9))
            {
                movement.curSpeed = (dir * movement.moveSpeed).xz;
                movement.destinationReached = false;
            }
            else
            {
                movement.destinationReached = true;
                movement.curSpeed = float2.zero;
            }
        });
    }
}
