using ECSFlowField.Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace ECSFlowField
{
    [UpdateAfter(typeof(FFIntegrationFieldSystem))]
    [UpdateInGroup(typeof(FlowFieldSystemGroup)), DisableAutoCreation]
    public partial class FFDirectionSystem : SystemBase
    {
        private BeginSimulationEntityCommandBufferSystem _beginSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            _beginSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _beginSimulationEntityCommandBufferSystem.CreateCommandBuffer();

            Entities
                .WithAll<FFGetFlowFieldTag>()
                .ForEach((Entity e, ref FlowFieldComponent flowFieldComponent,
                    ref DynamicBuffer<FlowFieldCellPositionBufferElement> cellPositionBuffer,
                    ref DynamicBuffer<FlowFieldCellCostBufferElement> cellCostBuffer,
                    ref DynamicBuffer<FlowFieldCellDirectionBufferElement> cellDirectionBuffer) =>
                {
                    ecb.RemoveComponent<FFGetFlowFieldTag>(e);

                    var flowFieldSize = flowFieldComponent.FieldSize;
                    var neighborCellIndices = new NativeList<int3>(Allocator.TempJob);

                    for (var i = 0; i < flowFieldComponent.CellCount; i++)
                    {
                        var cellIndex = cellPositionBuffer[i].Value.FlowFieldCellIndex;
                        neighborCellIndices.Clear();
                        var calculateCardinalNeighborIndicesJobHandle = new CalculateCardinalNeighborIndicesJob
                        {
                            TargetFlowFieldCellIndex = cellIndex,
                            FlowFieldSize = flowFieldSize,
                            NeighborCellIndices = neighborCellIndices
                        }.Schedule();

                        calculateCardinalNeighborIndicesJobHandle.Complete();

                        var cellCost = cellCostBuffer[i].Value;
                        var bestCost = cellCost.BestCost;
                        var bestDirection = int3.zero;

                        foreach (var neighbor in neighborCellIndices)
                        {
                            var neighborBufferIndex = FindCellBufferIndex(cellPositionBuffer, neighbor);
                            var neighborCellCost = cellCostBuffer[neighborBufferIndex].Value;

                            if (neighborCellCost.Value == byte.MaxValue) continue;
                            if (neighborCellCost.BestCost >= bestCost) continue;

                            bestCost = neighborCellCost.BestCost;
                            bestDirection = cellPositionBuffer[neighborBufferIndex].Value.FlowFieldCellIndex - cellIndex;
                        }

                        var tmpFlowFieldCellDirection = cellDirectionBuffer[i].Value;
                        tmpFlowFieldCellDirection.Value = bestDirection;
                        cellDirectionBuffer[i] = tmpFlowFieldCellDirection;
                    }

                    flowFieldComponent.IsBuilt = true;

                    neighborCellIndices.Dispose();
                })
                .WithoutBurst()
                .Run();
        }

        private int FindCellBufferIndex(DynamicBuffer<FlowFieldCellPositionBufferElement> flowFieldCellPositionBuffer, int3 index)
        {
            for (var i = 0; i < flowFieldCellPositionBuffer.Length; i++)
            {
                if (flowFieldCellPositionBuffer[i].Value.FlowFieldCellIndex.Equals(index))
                {
                    return i;
                }
            }

            return -1;
        }

        [BurstCompile]
        private struct CalculateCardinalNeighborIndicesJob : IJob
        {
            [ReadOnly] public int3 TargetFlowFieldCellIndex;
            [ReadOnly] public int3 FlowFieldSize;

            public NativeList<int3> NeighborCellIndices;

            public void Execute()
            {
                var x = TargetFlowFieldCellIndex.x;
                var y = TargetFlowFieldCellIndex.y;
                var z = TargetFlowFieldCellIndex.z;

                // Bottom
                FlowFieldHelper.TryAddCell(new int3(x - 1, y - 1, z + 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x, y - 1, z + 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x + 1, y - 1, z + 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x - 1, y - 1, z), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x, y - 1, z), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x + 1, y - 1, z), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x - 1, y - 1, z - 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x, y - 1, z - 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x + 1, y - 1, z - 1), FlowFieldSize, ref NeighborCellIndices);
                // Middle
                FlowFieldHelper.TryAddCell(new int3(x - 1, y, z + 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x, y, z + 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x + 1, y, z + 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x - 1, y, z), FlowFieldSize, ref NeighborCellIndices);
                // FlowFieldHelper.TryAddCell(new int3(x, y, z), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x + 1, y, z), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x - 1, y, z - 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x, y, z - 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x + 1, y, z - 1), FlowFieldSize, ref NeighborCellIndices);

                // Top
                FlowFieldHelper.TryAddCell(new int3(x - 1, y + 1, z + 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x, y + 1, z + 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x + 1, y + 1, z + 1), FlowFieldSize, ref NeighborCellIndices);

                FlowFieldHelper.TryAddCell(new int3(x - 1, y + 1, z), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x, y + 1, z), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x + 1, y + 1, z), FlowFieldSize, ref NeighborCellIndices);

                FlowFieldHelper.TryAddCell(new int3(x - 1, y + 1, z - 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x, y + 1, z - 1), FlowFieldSize, ref NeighborCellIndices);
                FlowFieldHelper.TryAddCell(new int3(x + 1, y + 1, z - 1), FlowFieldSize, ref NeighborCellIndices);
            }
        }
    }
}