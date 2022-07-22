using Unity.Entities;
using Unity.Mathematics;

namespace ECSFlowField
{
    [GenerateAuthoringComponent]
    public struct RaycastTargetComponent : IComponentData
    {
        public float3 HitPoint;
    }
}