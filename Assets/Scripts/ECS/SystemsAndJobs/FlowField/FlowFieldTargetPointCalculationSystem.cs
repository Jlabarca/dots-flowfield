using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.Tags;
using Unity.Entities;
using Unity.Mathematics;

namespace TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.FlowField
{
    [UpdateBefore(typeof(FlowFieldCellBufferInitializationSystem))]
    public partial class FlowFieldTargetPointCalculationSystem : SystemBase
    {
        private EndInitializationEntityCommandBufferSystem _endInitializationEntityCommandBufferSystem;

        private PlayerInputComponent _playerInputSingleton;

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
            var ecb = _endInitializationEntityCommandBufferSystem.CreateCommandBuffer();
            _playerInputSingleton = GetSingleton<PlayerInputComponent>();

            Entities
                .WithAll<RecalculateTargetFlowPointTag>()
                .ForEach((ref Entity e, ref FlowFieldComponent flowFieldComponent) =>
                {
                    ecb.RemoveComponent<RecalculateTargetFlowPointTag>(e);

                    var flowFieldSize = flowFieldComponent.FieldSize;
                    var cellDiameter = flowFieldComponent.CellDiameter;
                    var worldPos = _playerInputSingleton.PointerWorldPos;

                    var percentX = worldPos.x / (flowFieldSize.x * cellDiameter);
                    var percentY = worldPos.y / (flowFieldSize.y * cellDiameter);
                    var percentZ = worldPos.z / (flowFieldSize.z * cellDiameter);

                    percentX = math.clamp(percentX, 0, 1f);
                    percentY = math.clamp(percentY, 0, 1f);
                    percentZ = math.clamp(percentZ, 0, 1f);

                    var cellIndex = new int3
                    {
                        x = math.clamp((int)math.floor(flowFieldSize.x * percentX), 0, flowFieldSize.x - 1),
                        y = math.clamp((int)math.floor(flowFieldSize.y * percentY), 0, flowFieldSize.y - 1),
                        z = math.clamp((int)math.floor(flowFieldSize.z * percentZ), 0, flowFieldSize.z - 1),
                    };

                    flowFieldComponent.FlowTargetPoint = cellIndex;
                }).WithoutBurst().Run();
        }
    }
}