using Unity.Entities;

namespace TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers
{
    [InternalBufferCapacity(250)]
    public struct FlowFieldCellCostBufferElement : IBufferElementData
    {
        public FlowFieldCellCost Value;

        public static implicit operator FlowFieldCellCostBufferElement(FlowFieldCellCost value)
        {
            return new FlowFieldCellCostBufferElement {Value = value};
        }

        public static implicit operator FlowFieldCellCost(FlowFieldCellCostBufferElement element)
        {
            return element.Value;
        }
    }
}