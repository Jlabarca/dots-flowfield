using ECSFlowField.Cell;
using Unity.Entities;

namespace ECSFlowField
{
    [UpdateInGroup(typeof(FlowFieldSystemGroup)), DisableAutoCreation]
    public partial class FFCellBufferInitializationSystem : SystemBase
    {
        private EndInitializationEntityCommandBufferSystem _endInitializationEntityCommandBufferSystem;

        protected override void OnStartRunning()
        {
            _endInitializationEntityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _endInitializationEntityCommandBufferSystem.CreateCommandBuffer();

            Entities
                .WithAll<FFGrid_InitCellBuffersTag>()
                .ForEach((ref Entity e, ref FlowFieldComponent flowFieldComponent) =>
                {
                    ecb.RemoveComponent<FFGrid_InitCellBuffersTag>(e);
                    ecb.AddComponent<FFGrid_CellBuffersInitializedTag>(e);

                    var flowFieldSize = flowFieldComponent.FieldSize;
                    var cellCount = flowFieldSize.x * flowFieldSize.y * flowFieldSize.z;

                    var flowFieldCellPositionBuffer = ecb.AddBuffer<FlowFieldCellPositionBufferElement>(e);
                    var flowFieldCellCostBuffer = ecb.AddBuffer<FlowFieldCellCostBufferElement>(e);
                    var flowFieldCellDirectionBuffer = ecb.AddBuffer<FlowFieldCellDirectionBufferElement>(e);

                    for (var i = 0; i < cellCount; i++)
                    {
                        var cellPosition = new FFPosition();
                        var cellCost = new FFCost();
                        var cellDirection = new FFDirection();

                        flowFieldCellPositionBuffer.Add(cellPosition);
                        flowFieldCellCostBuffer.Add(cellCost);
                        flowFieldCellDirectionBuffer.Add(cellDirection);
                    }
                })
                .WithoutBurst()
                .Run();
        }
    }
}