using Unity.Entities;

namespace TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers
{
    [GenerateAuthoringComponent]
    public struct FlowFieldCellCost : IComponentData
    {
        public byte Cost;
        public ushort BestCost;        
    }
}