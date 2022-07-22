using ECSFlowField.Helpers;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace ECSFlowField
{
    [UpdateAfter(typeof(FFDirectionSystem))]
    [UpdateInGroup(typeof(FlowFieldSystemGroup)), DisableAutoCreation]
    public partial class FFPhysicsBodyMovementSystem : SystemBase
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
            var cellPositionBuffer = GetBuffer<FlowFieldCellPositionBufferElement>(flowFieldEntity);
            var cellDirectionBuffer = GetBuffer<FlowFieldCellDirectionBufferElement>(flowFieldEntity);
            var cellCostBuffer= GetBuffer<FlowFieldCellCostBufferElement>(flowFieldEntity);
            var destinationCellBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, flowFieldComponent.FlowTargetPoint);


            if (destinationCellBufferIndex == -1)
            {
                return;
            }

            Entities
                .ForEach((ref Entity e, ref Translation translation, ref Rotation rotation, ref PhysicsVelocity velocity, in MovementData movementData) =>
                {
                    var yPos = 0.5f;
                    rotation.Value = quaternion.identity;

                    if (cellCostBuffer[destinationCellBufferIndex].Value.Value == byte.MaxValue)
                    {
                        velocity.Linear.xz = float2.zero;
                        translation.Value.y = yPos;

                        return;
                    }

                    var currentCellBufferIndex = -1;
                    var distance = math.INFINITY;
                    for (var i = 0; i < cellPositionBuffer.Length; i++)
                    {
                        var tmpDistance = math.distance(cellPositionBuffer[i].Value.WorldPos, translation.Value);

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

                    var moveDirection = cellDirectionBuffer[currentCellBufferIndex].Value.Value.xz.Equals(float2.zero) ? float2.zero : math.normalize(cellDirectionBuffer[currentCellBufferIndex].Value.Value.xz);
                    velocity.Linear.xz = moveDirection * movementData.Speed;
                    translation.Value.y = yPos;
                })
                //.WithoutBurst()
                .Schedule();
        }
        //
        // [BurstCompile]
        // private struct FindCellByWorldPosJob : IJob
        // {
        //     [Unity.Collections.ReadOnly] public DynamicBuffer<FFPosition> CellPositionsBuffer;
        //     [Unity.Collections.ReadOnly] public float3 WorldPos;
        //
        //     public NativeArray<int> CellBufferIndex;
        //
        //     public void Execute()
        //     {
        //         var distance = math.INFINITY;
        //
        //         for (var i = 0; i < CellPositionsBuffer.Length; i++)
        //         {
        //             var tmpDistance = math.distance(CellPositionsBuffer[i].WorldPos, WorldPos);
        //
        //             if (tmpDistance < distance)
        //             {
        //                 distance = tmpDistance;
        //                 CellBufferIndex[0] = i;
        //             }
        //         }
        //     }
        // }
    }
}