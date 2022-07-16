using Unity.Entities;
using Unity.Mathematics;

namespace TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers
{
    [GenerateAuthoringComponent]
    public struct FlowFieldCellDirection : IComponentData
    {
        public float3 BestDirection;
    }
}