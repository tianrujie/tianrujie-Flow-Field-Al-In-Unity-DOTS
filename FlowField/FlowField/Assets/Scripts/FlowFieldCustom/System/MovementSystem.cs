using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TMG.ECSFlowField
{
    [AlwaysUpdateSystem]
    public class MovementSystem : SystemBase
    {
        private EntityCommandBufferSystem _ecbSystem;
        private EntityQuery _query;
        protected override void OnCreate()
        {
            _ecbSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
            _query = GetEntityQuery(typeof(EntityMovementData), typeof(Translation), typeof(Rotation));
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var destiPos2 = SharedDataContainer.Cells[SharedDataContainer.TargetCell].WorldPos;
            var destPos = new float3(destiPos2.x,0,destiPos2.y);
            Entities.ForEach((Entity entity, ref EntityMovementData entityMovementData, ref Translation translation, ref Rotation rotation) =>
            {
                if (entityMovementData.destinationReached)
                    return;
                
                if (math.distancesq(translation.Value, destPos) <= 0.01f)
                {
                    entityMovementData.destinationReached = true;
                    return;
                }
                
                SharedDataContainer.WorldPos2Idx(translation.Value,out var idx1, out var idx2);
                if (!SharedDataContainer.IdxLegal(idx1))
                    return;
                var cellData = SharedDataContainer.Cells[idx1];
                var bDir = ((Vector2)cellData.BestDir).normalized;
                var dir3T = new Vector3(bDir.x,0,-bDir.y).normalized;
                dir3T = Vector3.Lerp(entityMovementData.curSpeed,dir3T, 0.2f);
                var expectPos = Util.ClampPos(translation.Value + (float3) dir3T * entityMovementData.moveSpeed * deltaTime);
                translation.Value = expectPos;
                entityMovementData.curSpeed = dir3T;
            }).ScheduleParallel();

            //EntityCommandBuffer commandBuffer = _ecbSystem.CreateCommandBuffer();
            // Entities.ForEach((Entity entity) =>
            // {
            //     GridDebug.instance.AddToList(cellData);
            //     commandBuffer.RemoveComponent<AddToDebugTag>(entity);
            // }).Run();
        }

        protected override void OnDestroy()
        {
        }
    }
}