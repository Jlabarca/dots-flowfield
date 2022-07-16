using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.Tags;
using Unity.Entities;

namespace TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.FlowField
{
    [UpdateBefore(typeof(FlowFieldCellBufferInitializationSystem))]
    [UpdateBefore(typeof(FlowFieldTargetPointCalculationSystem))]
    [UpdateBefore(typeof(FlowFieldCellPositionCalculationSystem))]
    [UpdateBefore(typeof(FlowFieldCellCostCalculationSystem))]
    public partial class FlowFieldUpdateRequesterSystem : SystemBase
    {
        private EndInitializationEntityCommandBufferSystem _endInitializationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<PlayerInputComponent>();
        }

        protected override void OnStartRunning()
        {
            _endInitializationEntityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var playerInput = GetSingleton<PlayerInputComponent>();

            if (playerInput.PointerIsClick) // Should update when ground / obstacles change, this is for debug purposes
            {
                var ecb = _endInitializationEntityCommandBufferSystem.CreateCommandBuffer();

                Entities
                    .WithAll<FlowFieldComponent>()
                    .WithNone<RecalculateTargetFlowPointTag>()
                    .ForEach((ref Entity e ) =>
                    {
                        ecb.AddComponent<RecalculateTargetFlowPointTag>(e);
                    }).Schedule();

                Entities
                    .WithAll<FlowFieldComponent>()
                    .WithNone<FlowFieldCellBuffersAreInitializedTag>()
                    .ForEach((ref Entity e) =>
                {
                    ecb.AddComponent<InitializeFlowFieldCellBuffersRequestTag>(e);
                }).Schedule();

                Entities
                    .WithAll<FlowFieldComponent>()
                    .WithNone<RecalculateFlowFieldPositionsRequestTag>()
                    .ForEach((ref Entity e ) =>
                    {
                        ecb.AddComponent<RecalculateFlowFieldPositionsRequestTag>(e);
                    }).Schedule();

               Entities
                    .WithAll<FlowFieldComponent>()
                    .WithNone<RecalculateFlowFieldCostRequestTag>()
                    .ForEach((ref Entity e ) =>
                    {
                        ecb.AddComponent<RecalculateFlowFieldCostRequestTag>(e);
                    }).Schedule();

                Entities
                    .WithAll<FlowFieldComponent>()
                    .WithNone<RecalculateFlowFieldIntegrationFieldTag>()
                    .ForEach((ref Entity e ) =>
                    {
                        ecb.AddComponent<RecalculateFlowFieldIntegrationFieldTag>(e);
                    }).Schedule();

                Entities
                    .WithAll<FlowFieldComponent>()
                    .WithNone<RecalculateFlowFieldFlowDirectionTag>()
                    .ForEach((ref Entity e ) =>
                    {
                        ecb.AddComponent<RecalculateFlowFieldFlowDirectionTag>(e);
                    }).Schedule();

            }
        }
    }
}