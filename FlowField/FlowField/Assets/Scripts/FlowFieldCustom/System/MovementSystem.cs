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
            _query = GetEntityQuery(typeof(MovementData), typeof(Translation), typeof(Rotation),typeof(FlowFieldTag));
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var destiPos2 = SharedDataContainer.Cells[SharedDataContainer.TargetCell].WorldPos;
            var destPos = new float3(destiPos2.x,0,destiPos2.y);
            Entities.WithAll<FlowFieldTag>().ForEach((Entity entity, ref MovementData entityMovementData, ref Translation translation, ref Rotation rotation) =>
            {
                //Reset the target
                if (entityMovementData.destinationVersion != SharedDataContainer.TargetCellVersion)
                    entityMovementData.destinationReached = false;
                
                if (entityMovementData.destinationReached)
                {
                    entityMovementData.curSpeed = ((Vector2) entityMovementData.curSpeed).normalized *
                                                  entityMovementData.destinationMoveSpeed;
                    return;
                }
                
                SharedDataContainer.WorldPos2Idx(translation.Value,out var idx1, out var idx2);

                if (!SharedDataContainer.IdxLegal(idx1))
                    return;
                
                //if (math.distancesq(translation.Value, destPos) <= 2.5f)
                if(idx1 == SharedDataContainer.TargetCell)
                {
                    entityMovementData.destinationReached = true;
                    entityMovementData.destinationVersion = SharedDataContainer.TargetCellVersion;
                    return;
                }
               
                var cellData = SharedDataContainer.Cells[idx1];
                var bDir = ((Vector2)cellData.BestDir).normalized;
                var dir3T = new Vector3(bDir.x,0,-bDir.y).normalized;
                //dir3T = Vector3.Lerp(new Vector3(entityMovementData.curSpeed.x,0,entityMovementData.curSpeed.y), dir3T, 0.8f);
                var velocity = dir3T * entityMovementData.moveSpeed;
                var expectPos = Util.ClampPos(translation.Value + (float3)velocity * deltaTime);
                //translation.Value = expectPos;
                entityMovementData.curSpeed = new float2(velocity.x,velocity.z);
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