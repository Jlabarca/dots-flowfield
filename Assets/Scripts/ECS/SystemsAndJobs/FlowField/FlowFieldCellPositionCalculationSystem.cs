using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.FlowField
{
    [UpdateAfter(typeof(FlowFieldCellBufferInitializationSystem))]
    public partial class FlowFieldCellPositionCalculationSystem : SystemBase
    {
        private EndInitializationEntityCommandBufferSystem _endInitializationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            _endInitializationEntityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _endInitializationEntityCommandBufferSystem.CreateCommandBuffer();

            Entities
                .WithAll<RecalculateFlowFieldPositionsRequestTag>()
                .ForEach((Entity e, ref FlowFieldComponent flowFieldComponent, ref DynamicBuffer<FlowFieldCellPositionBufferElement> flowFieldCellPositionBuffer) =>
                {
                    ecb.RemoveComponent<RecalculateFlowFieldPositionsRequestTag>(e);

                    var cellCount = flowFieldComponent.CellCount;
                    var cellWorldPositionsResult = new NativeArray<float3>(cellCount, Allocator.TempJob);
                    var cellFlowFieldIndicesResult = new NativeArray<int3>(cellCount, Allocator.TempJob);
                    var flowFieldCellPositionsCalculationJobHandle =
                        ScheduleFlowFieldCellPositionsCalculationJob(flowFieldComponent, ref cellFlowFieldIndicesResult, ref cellWorldPositionsResult);
                    flowFieldCellPositionsCalculationJobHandle.Complete();

                    var cellEntityBuffer = flowFieldCellPositionBuffer.Reinterpret<FlowFieldCellPosition>();
                    for (var i = 0; i < flowFieldCellPositionBuffer.Length; i++)
                    {
                        var cellWorldPosition = cellWorldPositionsResult[i];
                        var cellFlowFieldIndex = cellFlowFieldIndicesResult[i];

                        var flowFieldCellPosition = cellEntityBuffer[i];
                        flowFieldCellPosition.WorldPos = cellWorldPosition;
                        flowFieldCellPosition.FlowFieldCellIndex = cellFlowFieldIndex;
                        cellEntityBuffer[i] = flowFieldCellPosition;
                    }

                    cellFlowFieldIndicesResult.Dispose();
                    cellWorldPositionsResult.Dispose();
                }).Run();
        }

        [BurstCompile]
        private struct FlowFieldCellPositionsCalculationJob : IJobBurstSchedulable
        {
            [ReadOnly] public int3 FlowFieldSize;
            [ReadOnly] public float CellDiameter;
            [ReadOnly] public float CellRadius;

            public NativeArray<int3> CellFlowFieldIndicesResult;
            public NativeArray<float3> CellWorldPositionsResult;

            public void Execute()
            {
                var fieldSizeX = FlowFieldSize.x;
                var fieldSizeY = FlowFieldSize.y;
                var fieldSizeZ = FlowFieldSize.z;

                var index = 0;

                for (var x = 0; x < fieldSizeX; x++)
                {
                    for (var z = 0; z < fieldSizeZ; z++)
                    {
                        for (var y = 0; y < fieldSizeY; y++)
                        {
                            var cellWorldPos = new float3
                            {
                                x = CellDiameter * x + CellRadius,
                                y = CellDiameter * y,
                                z = CellDiameter * z + CellRadius,
                            };

                            CellFlowFieldIndicesResult[index] = new int3(x,y,z);
                            CellWorldPositionsResult[index] = cellWorldPos;

                            index++;
                        }
                    }
                }

            }
        }

        private static JobHandle ScheduleFlowFieldCellPositionsCalculationJob(FlowFieldComponent flowFieldComponent,
            ref NativeArray<int3> cellFlowFieldIndicesResult,
            ref NativeArray<float3> cellWorldPositionsResult)
        {
            var jobHandle = new FlowFieldCellPositionsCalculationJob
            {
                FlowFieldSize = flowFieldComponent.FieldSize,
                CellDiameter = flowFieldComponent.CellDiameter,
                CellRadius = flowFieldComponent.CellRadius,
                CellFlowFieldIndicesResult = cellFlowFieldIndicesResult,
                CellWorldPositionsResult = cellWorldPositionsResult
            }.Schedule();

            return jobHandle;
        }

    }
}