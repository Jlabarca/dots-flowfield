using ECSFlowField.Cell;
using ECSFlowField.Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace ECSFlowField
{
    /// <summary>
    /// Flowfield
    /// </summary>
    [UpdateAfter(typeof(FFCostFieldSystem))]
    [UpdateInGroup(typeof(FlowFieldSystemGroup)), DisableAutoCreation]
    public partial class FFIntegrationFieldSystem : SystemBase
    {
        private EndInitializationEntityCommandBufferSystem entityCommandBuffer;

        protected override void OnCreate()
        {
            entityCommandBuffer = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = entityCommandBuffer.CreateCommandBuffer();

            Entities
                .WithAll<FFGetIntegrationFieldTag>()
                .ForEach((ref Entity e, ref FlowFieldComponent flowFieldComponent, ref DynamicBuffer<FlowFieldCellPositionBufferElement> cellPositionBuffer,
                    ref DynamicBuffer<FlowFieldCellCostBufferElement> cellCostBuffer) =>
                {
                    ecb.RemoveComponent<FFGetIntegrationFieldTag>(e);

                    var flowTargetCellIndex = flowFieldComponent.FlowTargetPoint;
                    var targetCellBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, flowTargetCellIndex);
                    if (targetCellBufferIndex > -1)
                    {
                        var flowFieldCellCost = cellCostBuffer[targetCellBufferIndex].Value;
                        if (flowFieldCellCost.Value == byte.MaxValue)
                        {
                            return;
                        }

                        flowFieldCellCost.Value = 0;
                        flowFieldCellCost.BestCost = 0;
                        cellCostBuffer[targetCellBufferIndex] = flowFieldCellCost;


                        var flowFieldSize = flowFieldComponent.FieldSize;
                        var cellIndicesToCheckQueue = new NativeQueue<int3>(Allocator.Temp);
                        var neighborCellIndices = new NativeList<int3>(Allocator.TempJob);

                        cellIndicesToCheckQueue.Enqueue(flowTargetCellIndex);

                        while (!cellIndicesToCheckQueue.IsEmpty())
                        {
                            var cellIndexToCheck = cellIndicesToCheckQueue.Dequeue();

                            neighborCellIndices.Clear();
                            var calculateOrthogonalNeighborIndicesJobHandle = new CalculateOrthogonalNeighbourIndicesJob
                            {
                                FlowFieldSize = flowFieldSize,
                                TargetFlowFieldCellIndex = cellIndexToCheck,
                                NeighborCellIndices = neighborCellIndices
                            }.Schedule();
                            calculateOrthogonalNeighborIndicesJobHandle.Complete();

                            var cellToCheckBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, cellIndexToCheck);
                            var cellToCheckCost = cellCostBuffer[cellToCheckBufferIndex].Value;

                            foreach (var neighbor in neighborCellIndices)
                            {
                                var neighborBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, neighbor);
                                var neighborCellCost = cellCostBuffer[neighborBufferIndex].Value;

                                if (neighborCellCost.Value == byte.MaxValue)
                                {
                                    continue;
                                }

                                if (neighborCellCost.Value + cellToCheckCost.BestCost < neighborCellCost.BestCost)
                                {
                                    var neighborCellBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, neighbor);
                                    var neighborCostInBuffer = cellCostBuffer[neighborCellBufferIndex].Value;
                                    neighborCostInBuffer.BestCost = (ushort)(neighborCostInBuffer.Value + cellToCheckCost.BestCost);
                                    cellCostBuffer[neighborCellBufferIndex] = neighborCostInBuffer;
                                    cellIndicesToCheckQueue.Enqueue(neighbor);
                                }
                            }
                        }

                        cellIndicesToCheckQueue.Dispose();
                        neighborCellIndices.Dispose();
                    }
                })
                .WithoutBurst()
                .Run();
        }

        [BurstCompile]
        private struct CalculateOrthogonalNeighbourIndicesJob : IJob
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
                FlowFieldHelper.TryAddCell(new int3(x, y - 1, z), FlowFieldSize, ref NeighborCellIndices);

                // Middle
                FlowFieldHelper.TryAddCell(new int3(x, y, z - 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x - 1, y, z), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x, y, z + 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x + 1, y, z), FlowFieldSize, ref NeighborCellIndices);

                // Top
                FlowFieldHelper.TryAddCell(new int3(x, y + 1, z), FlowFieldSize, ref NeighborCellIndices);
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