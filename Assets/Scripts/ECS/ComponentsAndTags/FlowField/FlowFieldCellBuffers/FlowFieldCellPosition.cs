using Unity.Entities;
using Unity.Mathematics;

namespace TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers
{
    public struct FlowFieldCellPosition : IComponentData
    {
        public float3 WorldPos;
        public int3 FlowFieldCellIndex;        
    }
}