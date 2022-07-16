using Unity.Entities;

namespace TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers
{
    [InternalBufferCapacity(250)]
    public struct FlowFieldCellPositionBufferElement : IBufferElementData
    {
        public FlowFieldCellPosition Value;

        public static implicit operator FlowFieldCellPositionBufferElement(FlowFieldCellPosition value)
        {
            return new FlowFieldCellPositionBufferElement {Value = value};
        }

        public static implicit operator FlowFieldCellPosition(FlowFieldCellPositionBufferElement element)
        {
            return element.Value;
        }
    }
}