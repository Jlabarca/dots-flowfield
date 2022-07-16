using Unity.Entities;

namespace TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers
{
    [InternalBufferCapacity(250)]
    public struct FlowFieldCellDirectionBufferElement : IBufferElementData
    {
        public FlowFieldCellDirection Value;

        public static implicit operator FlowFieldCellDirectionBufferElement(FlowFieldCellDirection value)
        {
            return new FlowFieldCellDirectionBufferElement {Value = value};
        }

        public static implicit operator FlowFieldCellDirection(FlowFieldCellDirectionBufferElement element)
        {
            return element.Value;
        }
    }
}