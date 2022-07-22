using Unity.Entities;

namespace ECSFlowField
{
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    public class FlowFieldSystemGroup : ComponentSystemGroup
    {
        private FFCellBufferInitializationSystem ffCellBufferInitializationSystem;
        private FFCalculateCellPositionsSystem ffCalculateCellPositionsSystem;
        private FFNewTargetSystem ffNewTargetSystem;

        private FFCostFieldSystem ffCostFieldSystem;
        private FFIntegrationFieldSystem ffIntegrationFieldSystem;
        private FFDirectionSystem ffDirectionSystem;

        private FFPhysicsBodyMovementSystem ffPhysicsBodyMovementSystem;
        private RaycastTargetSystem raycastTargetSystem;
        private FFUpdateRequesterSystem ffUpdateRequesterSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            var world = World.DefaultGameObjectInjectionWorld;

            ffCellBufferInitializationSystem = world.GetOrCreateSystem<FFCellBufferInitializationSystem>();
            ffCalculateCellPositionsSystem = world.GetOrCreateSystem<FFCalculateCellPositionsSystem>();
            ffNewTargetSystem = world.GetOrCreateSystem<FFNewTargetSystem>();

            ffCostFieldSystem = world.GetOrCreateSystem<FFCostFieldSystem>();
            ffIntegrationFieldSystem = world.GetOrCreateSystem<FFIntegrationFieldSystem>();
            ffDirectionSystem = world.GetOrCreateSystem<FFDirectionSystem>();

            ffPhysicsBodyMovementSystem = world.GetOrCreateSystem<FFPhysicsBodyMovementSystem>();
            raycastTargetSystem = world.GetOrCreateSystem<RaycastTargetSystem>();
            ffUpdateRequesterSystem = world.GetOrCreateSystem<FFUpdateRequesterSystem>();

            var initialize = world.GetOrCreateSystem<FlowFieldSystemGroup>();

            initialize.AddSystemToUpdateList(ffCellBufferInitializationSystem);
            initialize.AddSystemToUpdateList(ffCalculateCellPositionsSystem);
            initialize.AddSystemToUpdateList(ffNewTargetSystem);

            initialize.AddSystemToUpdateList(ffCostFieldSystem);
            initialize.AddSystemToUpdateList(ffIntegrationFieldSystem);
            initialize.AddSystemToUpdateList(ffDirectionSystem);

            initialize.AddSystemToUpdateList(ffPhysicsBodyMovementSystem);
            initialize.AddSystemToUpdateList(raycastTargetSystem);
            initialize.AddSystemToUpdateList(ffUpdateRequesterSystem);
        }

        protected override void OnUpdate()
        {
            raycastTargetSystem.Update();
            ffUpdateRequesterSystem.Update();

            ffCellBufferInitializationSystem.Update();
            ffCalculateCellPositionsSystem.Update();
            ffNewTargetSystem.Update();

            ffCostFieldSystem.Update();
            ffIntegrationFieldSystem.Update();
            ffDirectionSystem.Update();

            ffPhysicsBodyMovementSystem.Update();

        }
    }
}