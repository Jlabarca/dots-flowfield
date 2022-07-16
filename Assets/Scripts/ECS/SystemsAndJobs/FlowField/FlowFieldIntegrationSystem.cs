using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.Tags;
using TopDownCharacterController.Project.Scripts.Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.FlowField
{
    [UpdateAfter(typeof(FlowFieldCellCostCalculationSystem))]
    public partial class FlowFieldIntegrationSystem : SystemBase
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
                .WithAll<RecalculateFlowFieldIntegrationFieldTag>()
                .ForEach((ref Entity e, ref FlowFieldComponent flowFieldComponent, ref DynamicBuffer<FlowFieldCellPositionBufferElement> cellPositionBufferElement, ref DynamicBuffer<FlowFieldCellCostBufferElement> cellCostBufferElement) =>
            {
                ecb.RemoveComponent<RecalculateFlowFieldIntegrationFieldTag>(e);

                var flowTargetCellIndex = flowFieldComponent.FlowTargetPoint;

                var cellPositionBuffer = cellPositionBufferElement.Reinterpret<FlowFieldCellPosition>();
                var cellCostBuffer = cellCostBufferElement.Reinterpret<FlowFieldCellCost>();

                var targetCellBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, flowTargetCellIndex);
                if (targetCellBufferIndex > -1)
                {
                    var flowFieldCellCost = cellCostBuffer[targetCellBufferIndex];
                    if (flowFieldCellCost.Cost == byte.MaxValue)
                    {
                        return;
                    }
                    flowFieldCellCost.Cost = 0;
                    flowFieldCellCost.BestCost = 0;
                    cellCostBuffer[targetCellBufferIndex] = flowFieldCellCost;



                    var flowFieldSize = flowFieldComponent.FieldSize;
                    var cellIndicesToCheckQueue = new NativeQueue<int3>( Allocator.Temp);
                    var neighborCellIndices = new NativeList<int3>(Allocator.TempJob);

                    cellIndicesToCheckQueue.Enqueue(flowTargetCellIndex);

                    while (!cellIndicesToCheckQueue.IsEmpty())
                    {
                        var cellIndexToCheck = cellIndicesToCheckQueue.Dequeue();

                        neighborCellIndices.Clear();
                        var calculateOrthogonalNeighborIndicesJobHandle = new CalculateOrthogonalNeighborIndicesJob
                        {
                            FlowFieldSize = flowFieldSize,
                            TargetFlowFieldCellIndex = cellIndexToCheck,
                            NeighborCellIndices = neighborCellIndices
                        }.Schedule();
                        calculateOrthogonalNeighborIndicesJobHandle.Complete();

                        var cellToCheckBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, cellIndexToCheck);
                        var cellToCheckCost = cellCostBuffer[cellToCheckBufferIndex];
                        // var neighborCellIndicesLenght = neighborCellIndices.Length;
                        // var neighborCellIndicesToUpdate = new NativeArray<int3>(neighborCellIndicesLenght, Allocator.TempJob);
                        // var processNeighborCellBatchJobHandle = new ProcessNeighborCellBatchJob
                        // {
                        //     targetCellCost = cellToCheckCost,
                        //     CellPositionBuffer = cellPositionBuffer,
                        //     CellCostBuffer = cellCostBuffer,
                        //     NeighborCellIndices = neighborCellIndices,
                        //     NeighborCellIndicesToUpdate = neighborCellIndicesToUpdate
                        // }.Schedule(calculateOrthogonalNeighborIndicesJobHandle);
                        // processNeighborCellBatchJobHandle.Complete();


                        foreach (var neighbor in neighborCellIndices)
                        {
                            var neighborBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, neighbor);
                            var neighborCellCost = cellCostBuffer[neighborBufferIndex];

                            if (neighborCellCost.Cost == byte.MaxValue)
                            {
                                continue;
                            }

                            if (neighborCellCost.Cost + cellToCheckCost.BestCost < neighborCellCost.BestCost)
                            {
                                var neighborCellBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, neighbor);
                                var neighborCostInBuffer = cellCostBuffer[neighborCellBufferIndex];
                                neighborCostInBuffer.BestCost = (ushort) (neighborCostInBuffer.Cost + cellToCheckCost.BestCost);
                                cellCostBuffer[neighborCellBufferIndex] = neighborCostInBuffer;
                                cellIndicesToCheckQueue.Enqueue(neighbor);
                            }
                        }

                        // for (var i = 0; i < neighborCellIndicesToUpdate.Length; i++)
                        // {
                        //     var neighborCellIndex = neighborCellIndices[i];
                        //     var neighborCellBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, neighborCellIndex);
                        //     var neighborCostInBuffer = cellCostBuffer[neighborCellBufferIndex];
                        //     neighborCostInBuffer.BestCost = (ushort) (neighborCostInBuffer.Cost + cellToCheckCost.BestCost);
                        //     cellCostBuffer[neighborCellBufferIndex] = neighborCostInBuffer;
                        //     cellIndicesToCheckQueue.Enqueue(neighborCellIndex);
                        // }

                        // neighborCellIndicesToUpdate.Dispose();
                    }
                    cellIndicesToCheckQueue.Dispose();
                    neighborCellIndices.Dispose();
                }

            }).WithoutBurst().Run();
        }

        [BurstCompile]
        private struct CalculateOrthogonalNeighborIndicesJob : IJob
        {
            [ReadOnly] public int3 FlowFieldSize;
            [ReadOnly] public int3 TargetFlowFieldCellIndex;

            public NativeList<int3> NeighborCellIndices;

            public void Execute()
            {
                var x = TargetFlowFieldCellIndex.x;
                var y = TargetFlowFieldCellIndex.y;
                var z = TargetFlowFieldCellIndex.z;

                // Bottom
                FlowFieldHelper.TryAddCell(new int3(x, y-1, z), FlowFieldSize, ref NeighborCellIndices );

                // Middle
                FlowFieldHelper.TryAddCell(new int3(x, y, z-1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x-1, y, z), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x, y, z+1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x+1, y, z), FlowFieldSize, ref NeighborCellIndices );

                // Top
                FlowFieldHelper.TryAddCell(new int3(x, y+1, z), FlowFieldSize, ref NeighborCellIndices );
            }
        }



        // [BurstCompile]
        // private struct ProcessNeighborCellBatchJob : IJob
        // {
        //     [Unity.Collections.ReadOnly] public FlowFieldCellCost targetCellCost;
        //     [Unity.Collections.ReadOnly] public DynamicBuffer<FlowFieldCellPosition> CellPositionBuffer;
        //     [Unity.Collections.ReadOnly] public DynamicBuffer<FlowFieldCellCost> CellCostBuffer;
        //     [Unity.Collections.ReadOnly] public NativeList<int3> NeighborCellIndices;
        //
        //     public NativeArray<int3> NeighborCellIndicesToUpdate;
        //
        //     public void Execute()
        //     {
        //         for (var i = 0; i < NeighborCellIndices.Length; i++)
        //         {
        //             var neighbor = NeighborCellIndices[i];
        //             var neighborBufferIndex = FlowFieldHelper.FindCellBufferIndex(CellPositionBuffer, neighbor);
        //             var neighborCellCost = CellCostBuffer[neighborBufferIndex];
        //             if (neighborCellCost.Cost == byte.MaxValue)
        //             {
        //                 return;
        //             }
        //             if (neighborCellCost.Cost + targetCellCost.BestCost < neighborCellCost.BestCost)
        //             {
        //                 NeighborCellIndicesToUpdate[i] = neighbor;
        //             }
        //         }
        //     }
        // }

    }
}