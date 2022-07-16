using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.Tags;
using TopDownCharacterController.Project.Scripts.Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.FlowField
{
    [UpdateAfter(typeof(FlowFieldFlowDirectionSystem))]
    public partial class FlowFieldPhysicsBodyMovementSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<FlowFieldComponent>();
        }

        protected override void OnUpdate()
        {
            var flowFieldComponent = GetSingleton<FlowFieldComponent>();

            if (!flowFieldComponent.IsBuilt)
            {
                return;
            }

            var flowFieldEntity = GetSingletonEntity<FlowFieldComponent>();
            var cellPositionBufferElements = GetBuffer<FlowFieldCellPositionBufferElement>(flowFieldEntity);
            var cellPositionBuffer = cellPositionBufferElements.Reinterpret<FlowFieldCellPosition>();
            var cellDirectionBufferElements = GetBuffer<FlowFieldCellDirectionBufferElement>(flowFieldEntity);
            var cellDirectionBuffer = cellDirectionBufferElements.Reinterpret<FlowFieldCellDirection>();
            var cellCostBufferElements = GetBuffer<FlowFieldCellCostBufferElement>(flowFieldEntity);
            var cellCostBuffer = cellCostBufferElements.Reinterpret<FlowFieldCellCost>();
            var destinationCellBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, flowFieldComponent.FlowTargetPoint);
            var yPos = 0.5f;

            if (destinationCellBufferIndex == -1)
            {
                return;
            }

            Entities
                .ForEach((ref Entity e, ref Translation translation, ref Rotation rotation, ref PhysicsVelocity velocity, in MovementData movementData) =>
                {
                    rotation.Value = quaternion.identity;

                    if (cellCostBuffer[destinationCellBufferIndex].Cost == byte.MaxValue)
                    {
                        velocity.Linear.xz = float2.zero;
                        translation.Value.y = yPos;

                        return;
                    }
                    var currentCellBufferIndex = -1;
                    var distance = math.INFINITY;
                    for (var i = 0; i < cellPositionBuffer.Length; i++)
                    {
                        var tmpDistance = math.distance(cellPositionBuffer[i].WorldPos, translation.Value);

                        if (tmpDistance < distance)
                        {
                            distance = tmpDistance;
                            currentCellBufferIndex = i;
                        }
                    }

                    if (currentCellBufferIndex < 0 || currentCellBufferIndex.Equals(destinationCellBufferIndex))
                    {
                        velocity.Linear.xz = float2.zero;
                        translation.Value.y = yPos;
                        return;
                    }

                    var moveDirection = math.normalize(cellDirectionBuffer[currentCellBufferIndex].BestDirection.xz);
                    velocity.Linear.xz = moveDirection * movementData.Speed;
                    translation.Value.y = yPos;
                }).Schedule();


        }

        [BurstCompile]
        private struct FindCellByWorldPosJob : IJob
        {
            [Unity.Collections.ReadOnly] public DynamicBuffer<FlowFieldCellPosition> CellPositionsBuffer;
            [Unity.Collections.ReadOnly] public float3 WorldPos;

            public NativeArray<int> CellBufferIndex;

            public void Execute()
            {
                var distance = math.INFINITY;

                for (var i = 0; i < CellPositionsBuffer.Length; i++)
                {
                    var tmpDistance = math.distance(CellPositionsBuffer[i].WorldPos, WorldPos);

                    if (tmpDistance < distance)
                    {
                        distance = tmpDistance;
                        CellBufferIndex[0] = i;
                    }
                }
            }

        }

    }
}