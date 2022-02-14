using System.Collections.Generic;
using TMG.ECSFlowField;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[AlwaysUpdateSystem]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public class SweepSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job1 = new ClearJob4();
        
        var handles = new NativeList<JobHandle>(1, Allocator.TempJob);
        handles.Add(job1.Schedule(inputDeps));
        var jobHandle = JobHandle.CombineDependencies(handles);
        jobHandle.Complete();
        handles.Dispose();
        return jobHandle;
    }

    protected override void OnStopRunning()
    {
        
    }

    protected override void OnDestroy()
    {
        DestroyAllEntitys();
    }

    public void DestroyAllEntitys()
    {
        EntityQuery AgentQuery = GetEntityQuery(typeof(EntityMovementData));
        World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(AgentQuery);
    }
    
    public struct ClearJob4 : IJob
    {
        public void Execute()
        {
            var blocks = SharedDataContainer.Blocks;
            if (blocks != null)
            {
                for (var i = 0; i < blocks.Length; i++)
                {
                    for (var j = 0; j < Const1.NumBlocks; j++)
                    {
                        blocks[i][j].Clear();
                    }
                }
            }
        }
    }
}
