using Unity.Entities;
using Unity.Mathematics;

namespace ECSFlowField.PlayerInput
{
    [GenerateAuthoringComponent]
    public struct PlayerInputComponent : IComponentData
    {
        public float3 PointerWorldPos;
        public bool PointerIsClick;
        public bool PointerIsHold;
        public bool PointerIsRelease;
    }
}