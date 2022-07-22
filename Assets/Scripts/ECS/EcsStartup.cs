using ECSFlowField;
using Unity.Entities;
using Unity.Transforms;

public static class GameInitializer
{
    public static void InitializeSystemWorkflow()
    {
        InitializeSystems();
    }

    private static void InitializeSystems()
    {
        var world = World.DefaultGameObjectInjectionWorld;

        //System Group Handles (From Unity)
        var initialization = world.GetOrCreateSystem<InitializationSystemGroup>();
        var simulation = world.GetOrCreateSystem<SimulationSystemGroup>();
        var transform = world.GetOrCreateSystem<TransformSystemGroup>();
        var lateSimulation = world.GetOrCreateSystem<LateSimulationSystemGroup>();
        var presentation = world.GetOrCreateSystem<PresentationSystemGroup>();

        var flowFieldSystemGroup = world.GetOrCreateSystem<FlowFieldSystemGroup>();

        //Adding Managers as systems
        initialization.AddSystemToUpdateList(flowFieldSystemGroup);


        //Sorting
        initialization.SortSystems();
        simulation.SortSystems();
        transform.SortSystems();
        lateSimulation.SortSystems();
        presentation.SortSystems();
    }
}
