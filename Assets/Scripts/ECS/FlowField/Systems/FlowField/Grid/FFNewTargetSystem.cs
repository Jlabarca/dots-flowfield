using ECSFlowField.PlayerInput;
using Unity.Entities;
using Unity.Mathematics;

namespace ECSFlowField
{
    /// <summary>
    ///  Handle RecalculateTargetFlowPointTag
    ///  Calculates the new FlowTargetPoint
    /// </summary>
    [UpdateBefore(typeof(FFCellBufferInitializationSystem))]
    [UpdateInGroup(typeof(FlowFieldSystemGroup)), DisableAutoCreation]
    public partial class FFNewTargetSystem : SystemBase
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
            _playerInputSingleton = GetSingleton<PlayerInputComponent>();
            if (!_playerInputSingleton.PointerIsClick) return;
            _playerInputSingleton.PointerIsClick = false;
            SetSingleton(_playerInputSingleton);
            var ecb = _endInitializationEntityCommandBufferSystem.CreateCommandBuffer();

            Entities
                .WithAll<FFNewTargetTag>()
                .ForEach((ref Entity e, ref FlowFieldComponent flowFieldComponent) =>
                {
                    ecb.RemoveComponent<FFNewTargetTag>(e);

                    var flowFieldSize = flowFieldComponent.FieldSize;
                    var cellDiameter = flowFieldComponent.CellDiameter;
                    var worldPos = _playerInputSingleton.PointerWorldPos;

                    var cellIndex = GetCellIndex(worldPos, flowFieldSize, cellDiameter);

                    flowFieldComponent.FlowTargetPoint = cellIndex;

                }).WithoutBurst().Run();
        }


        private static int3 GetCellIndex(float3 worldPos, int3 flowFieldSize, float cellDiameter)
        {
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
            return cellIndex;
        }
    }
}