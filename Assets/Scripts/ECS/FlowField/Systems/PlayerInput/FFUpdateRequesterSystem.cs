using ECSFlowField.PlayerInput;
using Unity.Collections;
using Unity.Entities;

namespace ECSFlowField
{
    /// <summary>
    ///  System que al click agrega todos los tags components a las entity con FlowFieldComponent
    /// </summary>
    [UpdateBefore(typeof(FFCellBufferInitializationSystem))]
    [UpdateBefore(typeof(FFCostFieldSystem))]
    [UpdateInGroup(typeof(FlowFieldSystemGroup)), DisableAutoCreation]
    public partial class FFUpdateRequesterSystem : SystemBase
    {
        private EndInitializationEntityCommandBufferSystem entityCommandBuffer;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<PlayerInputComponent>();
        }

        protected override void OnStartRunning()
        {
            entityCommandBuffer = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var playerInput = GetSingleton<PlayerInputComponent>();

            if (!playerInput.PointerIsClick) return; // Should update when ground / obstacles change, this is for debug purposes


            //Create ECB
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            Entities
                .WithAll<FlowFieldComponent>()
                .WithNone<FFNewTargetTag>()
                .ForEach((ref Entity e) => { ecb.AddComponent<FFNewTargetTag>(e); }).WithoutBurst().Schedule();

            Entities
                .WithAll<FlowFieldComponent>()
                .WithNone<FFGrid_CellBuffersInitializedTag>()
                .ForEach((ref Entity e) => { ecb.AddComponent<FFGrid_InitCellBuffersTag>(e); }).WithoutBurst().Schedule();

            Entities
                .WithAll<FlowFieldComponent>()
                .WithNone<FFGrid_CalculateCellPositionsTag>()
                .ForEach((ref Entity e) => { ecb.AddComponent<FFGrid_CalculateCellPositionsTag>(e); }).WithoutBurst().Schedule();

            Entities
                .WithAll<FlowFieldComponent>()
                .WithNone<FFGetCostFieldTag>()
                .ForEach((ref Entity e) => { ecb.AddComponent<FFGetCostFieldTag>(e); }).WithoutBurst().Schedule();

            Entities
                .WithAll<FlowFieldComponent>()
                .WithNone<FFGetIntegrationFieldTag>()
                .ForEach((ref Entity e) => { ecb.AddComponent<FFGetIntegrationFieldTag>(e); }).WithoutBurst().Schedule();

            Entities
                .WithAll<FlowFieldComponent>()
                .WithNone<FFGetFlowFieldTag>()
                .ForEach((ref Entity e) => { ecb.AddComponent<FFGetFlowFieldTag>(e); }).WithoutBurst().Schedule();

            Dependency.Complete();

// Now that the job is completed, you can enact the changes.
// Note that Playback can only be called on the main thread.
            ecb.Playback(EntityManager);

// You are responsible for disposing of any ECB you create.
            ecb.Dispose();

        }
    }
}