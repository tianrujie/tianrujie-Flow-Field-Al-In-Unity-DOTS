using System;
using System.Collections.Generic;
using TMG.ECSFlowField;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[AlwaysUpdateSystem]
[UpdateAfter(typeof(MovementSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class RVO3System : JobComponentSystem
{
    public NativeArray<float2> _newVelocity;
    
    EntityQuery _query;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        _query = GetEntityQuery(typeof(Translation), typeof(MovementData), typeof(AgentData));
    }
    
    protected override void OnStopRunning()
    {
        _newVelocity.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var updateBlockJob = new UpdateBlocksJob();
        //there need to be ScheduleSingle, otherwise write SharedDataContainer.Blocks may cause Unity Crash 
        var blockJobHandle = updateBlockJob.ScheduleSingle(this, inputDeps);
        blockJobHandle.Complete();
        
        if (Time.DeltaTime < 0.0001f)
        {
            return inputDeps;
        }
        if (_newVelocity.IsCreated)
        {
            _newVelocity.Dispose();
        }

        var count = _query.CalculateEntityCountWithoutFiltering();
        _newVelocity = new NativeArray<float2>(count, Allocator.TempJob);

        var steerJob = new Steering();
        steerJob.newVelocity_ = _newVelocity;
        steerJob._deltaTime = Time.DeltaTime;
        steerJob.frameCount = UnityEngine.Time.frameCount;
        steerJob.posDataFromEntity = GetComponentDataFromEntity<Translation>();
        steerJob.agentDataFromEntity = GetComponentDataFromEntity<AgentData>();
        steerJob.movementDataFromEntity = GetComponentDataFromEntity<MovementData>();

        var steerJobHandle = steerJob.Schedule(this, blockJobHandle);

        var updateJob1 = new UpdatePosition();
        updateJob1.newVelocity_ = _newVelocity;
        updateJob1._deltaTime = Time.DeltaTime;
        var h = updateJob1.Schedule(this, steerJobHandle);
        h.Complete();
        return h;
    }


    

    [Serializable]
    public struct Line
    {
        public float2 direction;
        public float2 point;
    }
    
    struct UpdateBlocksJob : IJobForEachWithEntity<Translation, AgentData>
    {
        public void Execute(Entity entity, int index,
            [ReadOnly] ref Translation tr,
            [ReadOnly] ref AgentData agent)
        {
            var pos = tr.Value.xz;
            var idx = Util.GetBlockIndex(pos);
            var blocks = SharedDataContainer.Blocks;
            switch (agent.Camp)
            {
                case Const1.CAMP_A:
                case Const1.CAMP_B:
                    blocks[agent.Camp][idx].Add(new BlockItem(entity, pos));
                    break;
                default:
                    throw new Exception("unknown camp: " + agent.Camp);
            }
        }
    }

    struct UpdatePosition : IJobForEachWithEntity<Translation,MovementData>
    {
        [ReadOnly] public NativeArray<float2> newVelocity_;
        public float _deltaTime;

        public void Execute(Entity entity, int index,
            ref Translation translation, 
            ref MovementData movement)
        {
            var v = newVelocity_[index];
             if (v.x > 0.01f || v.x < -0.01f || v.y > 0.01f || v.y < -0.01f)
             {
                var d = v * _deltaTime;
                var dist = math.length(d);
                //Debug.Log($"{entity} moved {dist} ");
                //if (dist > 0.1f)
                    translation.Value = clampPosition(new float2(translation.Value.x+d.x, translation.Value.z+d.y));
            }

            if (v.x != movement.curSpeed.x || v.y != movement.curSpeed.y)
            {
                movement.curSpeed = v;
            }
        }

        static float3 clampPosition(float2 pos)
        {
            var x = math.clamp(pos.x, Const1.LevelMinX, Const1.LevelMaxX);
            var y = math.clamp(pos.y, Const1.LevelMinY, Const1.LevelMaxY);
            return new float3(x, 0, y);
        }
    }

    struct Steering : IJobForEachWithEntity<Translation, AgentData, MovementData>
    {
        [ReadOnly] public ComponentDataFromEntity<Translation> posDataFromEntity;
        [ReadOnly] public ComponentDataFromEntity<AgentData> agentDataFromEntity;
        [ReadOnly] public ComponentDataFromEntity<MovementData> movementDataFromEntity;
        
        public NativeArray<float2> newVelocity_;
        
        public float _deltaTime;
        public int frameCount;

        public static float det(float2 a, float2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        void findNearestNeighbors(Entity entity, float2 pos, int blockIdx, ref FixedSizeList_10<KeyValuePair<float, Entity>> arr)
        {
            if (blockIdx < 0 || blockIdx >= Const1.NumBlocks)
            {
                return;
            }
            if (arr.Length == arr.Capacity)
            {
                return;
            }

            var blocks = SharedDataContainer.Blocks;
            for (var i = 0; i < blocks.Length; i++)
            {
                var ettPosList = blocks[i][blockIdx];
                for (var j = 0; j < ettPosList.Length; j++)
                {
                    var ettPos = ettPosList[j];
                    if (ettPos.Entity == entity)
                    {
                        continue;
                    }

                    if (!agentDataFromEntity.Exists(entity) || !agentDataFromEntity.Exists(ettPos.Entity))
                        continue;
                    var r = agentDataFromEntity[entity].Radius + agentDataFromEntity[ettPos.Entity].Radius;
                    var dsq = math.lengthsq(ettPos.Pos - pos);
                    if (dsq < r * r)
                    {
                        arr.Add(new KeyValuePair<float, Entity>(dsq, ettPos.Entity));
                        if (arr.Length == arr.Capacity)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public void Execute(
            Entity entity,
            int index,
            [ReadOnly] ref Translation translation,
            [ReadOnly] ref AgentData agentData,
            [ReadOnly] ref MovementData movementData
            )
        {
            if (movementData.moveSpeed <= 0)
            {
                newVelocity_[index] = float2.zero;
                return;
            }

            var pos = translation.Value.xz;
            var selfBlockCenter = Util.GetBlockCenter(pos);
            var arr = new FixedSizeList_10<KeyValuePair<float, Entity>>();
            for (var i = 0; i < Const1.BlockCloseNeighbors.Length; i++)
            {
                var blockCenter = selfBlockCenter + Const1.BlockCloseNeighbors[i];
                var blockIdx = Util.GetBlockIndex(blockCenter);
                findNearestNeighbors(entity, pos, blockIdx, ref arr);
            }

            var orcaLines = new FixedSizeList_20<Line>();
            float invTimeHorizonObst = 1.0f / Const1.TimeHorizonObstacle;
            int numObstLines = orcaLines.Length;
            float invTimeHorizon = 1.0f / Const1.TimeHorizon;

            /* Create agent ORCA lines. */
            for (int i = 0; i < arr.Length; ++i)
            {
                var otherIndex = arr[i].Value;

                float2 relativePosition = (posDataFromEntity[otherIndex].Value - translation.Value).xz;
                var otherMvtData = movementDataFromEntity[otherIndex];

                float2 otherVel = otherMvtData.curSpeed;
                float2 selfVel = movementData.curSpeed;
                float radio = 0.01f;
                
                if (!movementData.destinationReached && !otherMvtData.destinationReached)
                    radio = 0.1f;
                
                if (!movementData.destinationReached && otherMvtData.destinationReached)
                    radio = 0.2f;
                
                if (movementData.destinationReached && !otherMvtData.destinationReached)
                    radio = 0.0f;
                
                if (movementData.destinationReached && otherMvtData.destinationReached)
                    radio = 0.1f;
                
                float2 relativeVelocity = selfVel - otherVel;
                float distSq = math.lengthsq(relativePosition);
                float combinedRadius = agentData.Radius + agentDataFromEntity[otherIndex].Radius;
                float combinedRadiusSq = combinedRadius * combinedRadius;

                Line line;
                float2 u;

                if (distSq > combinedRadiusSq)
                {
                    /* No collision. */
                    float2 w = relativeVelocity - invTimeHorizon * relativePosition;

                    /* Vector from cutoff center to relative velocity. */
                    float wLengthSq = math.lengthsq(w);
                    float dotProduct1 = math.dot(w, relativePosition);

                    if (dotProduct1 < 0.0f && (dotProduct1 * dotProduct1) > combinedRadiusSq * wLengthSq)
                    {
                        /* Project on cut-off circle. */
                        float wLength = math.sqrt(wLengthSq);
                        float2 unitW = w / wLength;

                        line.direction = new float2(unitW.y, -unitW.x);
                        u = (combinedRadius * invTimeHorizon - wLength) * unitW;
                    }
                    else
                    {
                        /* Project on legs. */
                        float leg = math.sqrt(distSq - combinedRadiusSq);

                        if (det(relativePosition, w) > 0.0f)
                        {
                            /* Project on left leg. */
                            line.direction = new float2(relativePosition.x * leg - relativePosition.y * combinedRadius,
                                                 relativePosition.x * combinedRadius + relativePosition.y * leg) /
                                             distSq;
                        }
                        else
                        {
                            /* Project on right leg. */
                            line.direction =
                                -new float2(relativePosition.x * leg + relativePosition.y * combinedRadius,
                                    -relativePosition.x * combinedRadius + relativePosition.y * leg) / distSq;
                        }

                        float dotProduct2 = math.dot(relativeVelocity, line.direction);
                        u = dotProduct2 * line.direction - relativeVelocity;
                    }
                }
                else
                {
                    /* Collision. Project on cut-off circle of time timeStep. */
                    float invTimeStep = 1.0f / _deltaTime;

                    /* Vector from cutoff center to relative velocity. */
                    float2 w = relativeVelocity - invTimeStep * relativePosition;

                    float wLength = math.length(w);
                    float2 unitW = w / wLength;

                    line.direction = new float2(unitW.y, -unitW.x);
                    u = (combinedRadius * invTimeStep - wLength) * unitW;
                }

                line.point = selfVel + radio * u;
                orcaLines.Add(line);
            }

            float2 newVelocity = float2.zero;
            int lineFail = linearProgram2(ref orcaLines, movementData.moveSpeed, movementData.curSpeed,
                false, ref newVelocity, index);

            if (lineFail < orcaLines.Length)
            {
                linearProgram3(ref orcaLines, numObstLines, lineFail, movementData.moveSpeed, ref newVelocity,
                    index);
            }

            newVelocity_[index] = newVelocity;
        }

        private bool linearProgram1
        (ref FixedSizeList_20<Line> lines,
            int lineNo,
            float radius,
            ref float2 optVelocity,
            bool directionOpt,
            ref float2 result,
            int myIndex)
        {
            float dotProduct = math.dot(lines[lineNo].point, lines[lineNo].direction);
            float discriminant = (dotProduct * dotProduct) + (radius * radius) - math.lengthsq(lines[lineNo].point);

            if (discriminant < 0.0f)
            {
                /* Max speed circle fully invalidates line lineNo. */
                return false;
            }

            float sqrtDiscriminant = math.sqrt(discriminant);
            float tLeft = -dotProduct - sqrtDiscriminant;
            float tRight = -dotProduct + sqrtDiscriminant;

            for (int i = 0; i < lineNo; ++i)
            {
                float denominator = det(lines[lineNo].direction, lines[i].direction);
                float numerator = det(lines[i].direction, lines[lineNo].point - lines[i].point);

                if (math.abs(denominator) <= 0.00001f)
                {
                    /* Lines lineNo and i are (almost) parallel. */
                    if (numerator < 0.0f)
                    {
                        return false;
                    }

                    continue;
                }

                float t = numerator / denominator;

                if (denominator >= 0.0f)
                {
                    /* Line i bounds line lineNo on the right. */
                    tRight = math.min(tRight, t);
                }
                else
                {
                    /* Line i bounds line lineNo on the left. */
                    tLeft = math.max(tLeft, t);
                }

                if (tLeft > tRight)
                {
                    return false;
                }
            }

            if (directionOpt)
            {
                /* Optimize direction. */
                if (math.dot(optVelocity, lines[lineNo].direction) > 0.0f)
                {
                    /* Take right extreme. */
                    result = lines[lineNo].point + tRight * lines[lineNo].direction;
                }
                else
                {
                    /* Take left extreme. */
                    result = lines[lineNo].point + tLeft * lines[lineNo].direction;
                }
            }
            else
            {
                /* Optimize closest point. */
                float t = math.dot(lines[lineNo].direction, (optVelocity - lines[lineNo].point));

                if (t < tLeft)
                {
                    result = lines[lineNo].point + tLeft * lines[lineNo].direction;
                }
                else if (t > tRight)
                {
                    result = lines[lineNo].point + tRight * lines[lineNo].direction;
                }
                else
                {
                    result = lines[lineNo].point + t * lines[lineNo].direction;
                }
            }
            
            return true;
        }

        private int linearProgram2
        (ref FixedSizeList_20<Line> lines,
            float radius,
            float2 optVelocity,
            bool directionOpt,
            ref float2 result,
            int myIndex)
        {
            if (directionOpt)
            {
                /*
                 * Optimize direction. Note that the optimization velocity is of
                 * unit length in this case.
                 */
                result = optVelocity * radius;
            }
            else if (math.lengthsq(optVelocity) > (radius * radius))
            {
                /* Optimize closest point and outside circle. */
                result = math.normalizesafe(optVelocity) * radius;
            }
            else
            {
                /* Optimize closest point and inside circle. */
                result = optVelocity;
            }

            for (int i = 0; i < lines.Length; ++i)
            {
                if (det(lines[i].direction, lines[i].point - result) > 0.0f)
                {
                    /* Result does not satisfy constraint i. Compute new optimal result. */
                    Vector2 tempResult = result;
                    if (!linearProgram1(ref lines, i, radius, ref optVelocity, directionOpt, ref result, myIndex))
                    {
                        result = tempResult;

                        return i;
                    }
                }
            }

            return lines.Length;
        }

        /**
         * <summary>Solves a two-dimensional linear program subject to linear
         * constraints defined by lines and a circular constraint.</summary>
         *
         * <param name="lines">Lines defining the linear constraints.</param>
         * <param name="numObstLines">Count of obstacle lines.</param>
         * <param name="beginLine">The line on which the 2-d linear program
         * failed.</param>
         * <param name="radius">The radius of the circular constraint.</param>
         * <param name="result">A reference to the result of the linear program.
         * </param>
         */
        private void linearProgram3
        (ref FixedSizeList_20<Line> lines,
            int numObstLines,
            int beginLine,
            float radius,
            ref float2 result,
            int myIndex)
        {
            float distance = 0.0f;

            for (int i = beginLine; i < lines.Length; ++i)
            {
                if (det(lines[i].direction, lines[i].point - result) > distance)
                {
                    /* Result does not satisfy constraint of line i. */
                    var projLines = new FixedSizeList_20<Line>();
                    for (int ii = 0; ii < numObstLines; ++ii)
                    {
                        projLines.Add(lines[ii]);
                    }

                    for (int j = numObstLines; j < i; ++j)
                    {
                        Line line;

                        float determinant = det(lines[i].direction, lines[j].direction);

                        if (math.abs(determinant) < 0.00001f)
                        {
                            /* Line i and line j are parallel. */
                            if (math.dot(lines[i].direction, lines[j].direction) > 0.0f)
                            {
                                /* Line i and line j point in the same direction. */
                                continue;
                            }
                            else
                            {
                                /* Line i and line j point in opposite direction. */
                                line.point = 0.5f * (lines[i].point + lines[j].point);
                            }
                        }
                        else
                        {
                            line.point = lines[i].point +
                                         (det(lines[j].direction, lines[i].point - lines[j].point) / determinant) *
                                         lines[i].direction;
                        }

                        line.direction = math.normalizesafe(lines[j].direction - lines[i].direction);
                        projLines.Add(line);
                    }

                    Vector2 tempResult = result;

                    var dir = new Vector2(-lines[i].direction.y, lines[i].direction.x);
                    if (linearProgram2(ref projLines, radius, dir, true, ref result, myIndex) < projLines.Length)
                    {
                        /*
                         * This should in principle not happen. The result is by
                         * definition already in the feasible region of this
                         * linear program. If it fails, it is due to small
                         * floating point error, and the current result is kept.
                         */
                        result = tempResult;
                    }

                    distance = det(lines[i].direction, lines[i].point - result);
                }
            }
        }
    }
}
