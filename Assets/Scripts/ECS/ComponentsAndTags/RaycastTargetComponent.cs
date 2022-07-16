using Unity.Entities;
using Unity.Mathematics;

namespace TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags
{
    [GenerateAuthoringComponent]
    public struct RaycastTargetComponent : IComponentData
    {
        public float3 HitPoint;
    }
}