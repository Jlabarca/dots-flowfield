using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.Tags;
using Unity.Entities;

namespace TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.FlowField
{
    public partial class FlowFieldCellBufferInitializationSystem : SystemBase
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
                .WithAll<InitializeFlowFieldCellBuffersRequestTag>()
                .ForEach((ref Entity e, ref FlowFieldComponent flowFieldComponent) =>
                {
                    ecb.RemoveComponent<InitializeFlowFieldCellBuffersRequestTag>(e);
                    ecb.AddComponent<FlowFieldCellBuffersAreInitializedTag>(e);
                    
                    var flowFieldSize = flowFieldComponent.FieldSize;
                    var cellCount = flowFieldSize.x * flowFieldSize.y * flowFieldSize.z;
                    
                    var flowFieldCellPositionBuffer = ecb.AddBuffer<FlowFieldCellPositionBufferElement>(e); 
                    var flowFieldCellCostBuffer = ecb.AddBuffer<FlowFieldCellCostBufferElement>(e); 
                    var flowFieldCellDirectionBuffer = ecb.AddBuffer<FlowFieldCellDirectionBufferElement>(e); 

                    for (var i = 0; i < cellCount; i++)
                    {
                        var cellPosition = new FlowFieldCellPosition();
                        var cellCost = new FlowFieldCellCost();
                        var cellDirection = new FlowFieldCellDirection();

                        flowFieldCellPositionBuffer.Add(cellPosition);
                        flowFieldCellCostBuffer.Add(cellCost);
                        flowFieldCellDirectionBuffer.Add(cellDirection);
                    }
                }).WithoutBurst().Run();
        }
    }
}