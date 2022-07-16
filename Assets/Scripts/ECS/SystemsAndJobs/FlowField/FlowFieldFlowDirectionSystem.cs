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
    [UpdateAfter(typeof(FlowFieldIntegrationSystem))]
    public partial class FlowFieldFlowDirectionSystem : SystemBase
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
                .WithAll<RecalculateFlowFieldFlowDirectionTag>()
                .ForEach((Entity e, ref FlowFieldComponent flowFieldComponent,
                    ref DynamicBuffer<FlowFieldCellPositionBufferElement> cellPositionBufferElement, 
                    ref DynamicBuffer<FlowFieldCellCostBufferElement> cellCostBufferElement, 
                    ref DynamicBuffer<FlowFieldCellDirectionBufferElement> cellDirectionBufferElement) =>
            {
                ecb.RemoveComponent<RecalculateFlowFieldFlowDirectionTag>(e);

                var flowFieldSize = flowFieldComponent.FieldSize;
                var cellPositionBuffer = cellPositionBufferElement.Reinterpret<FlowFieldCellPosition>();
                var cellCostBuffer = cellCostBufferElement.Reinterpret<FlowFieldCellCost>();
                var cellDirectionBuffer = cellDirectionBufferElement.Reinterpret<FlowFieldCellDirection>();
                var neighborCellIndices = new NativeList<int3>(Allocator.TempJob);

                for (var i = 0; i < flowFieldComponent.CellCount; i++)
                {
                    var cellIndex = cellPositionBuffer[i].FlowFieldCellIndex;
                    
                    neighborCellIndices.Clear();
                    var calculateCardinalNeighborIndicesJobHandle = new CalculateCardinalNeighborIndicesJob
                    {
                        TargetFlowFieldCellIndex = cellIndex,
                        FlowFieldSize = flowFieldSize,
                        NeighborCellIndices = neighborCellIndices
                    }.Schedule();
                    calculateCardinalNeighborIndicesJobHandle.Complete();

                    var cellCost = cellCostBuffer[i];
                    var bestCost = cellCost.BestCost;
                    var bestDirection = int3.zero;
                    
                    foreach (var neighbor in neighborCellIndices)
                    {
                        var neighborBufferIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, neighbor);
                        var neighborCellCost = cellCostBuffer[neighborBufferIndex];
                        
                        if (neighborCellCost.Cost == byte.MaxValue) continue;
                        if (neighborCellCost.BestCost >= bestCost) continue;
                        
                        bestCost = neighborCellCost.BestCost;
                        bestDirection = cellPositionBuffer[neighborBufferIndex].FlowFieldCellIndex - cellIndex;
                    }
                    
                    var tmpFlowFieldCellDirection = cellDirectionBuffer[i];
                    tmpFlowFieldCellDirection.BestDirection = bestDirection;
                    cellDirectionBuffer[i] = tmpFlowFieldCellDirection;
                }

                flowFieldComponent.IsBuilt = true;

                neighborCellIndices.Dispose();
            }).WithoutBurst().Run();
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
                FlowFieldHelper.TryAddCell(new int3(x-1, y-1, z+1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x, y-1, z+1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x+1, y-1, z+1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x-1, y-1, z), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x, y-1, z), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x+1, y-1, z), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x-1, y-1, z-1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x, y-1, z-1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x+1, y-1, z-1), FlowFieldSize, ref NeighborCellIndices );
                // Middle
                FlowFieldHelper.TryAddCell(new int3(x-1, y, z+1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x, y, z+1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x+1, y, z+1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x-1, y, z), FlowFieldSize, ref NeighborCellIndices );
                // FlowFieldHelper.TryAddCell(new int3(x, y, z), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x+1, y, z), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x-1, y, z-1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x, y, z-1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x+1, y, z-1), FlowFieldSize, ref NeighborCellIndices );

                // Top
                FlowFieldHelper.TryAddCell(new int3(x-1, y+1, z+1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x, y+1, z+1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x+1, y+1, z+1), FlowFieldSize, ref NeighborCellIndices );
                
                FlowFieldHelper.TryAddCell(new int3(x-1, y+1, z), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x, y+1, z), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x+1, y+1, z), FlowFieldSize, ref NeighborCellIndices );
                
                FlowFieldHelper.TryAddCell(new int3(x-1, y+1, z-1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x, y+1, z-1), FlowFieldSize, ref NeighborCellIndices );
                FlowFieldHelper.TryAddCell(new int3(x+1, y+1, z-1), FlowFieldSize, ref NeighborCellIndices );
            }
        }
        
    }
}