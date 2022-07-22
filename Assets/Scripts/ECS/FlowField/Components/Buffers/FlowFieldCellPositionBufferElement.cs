using ECSFlowField.Cell;
using Unity.Entities;

namespace ECSFlowField
{
    [InternalBufferCapacity(250)]
    public struct FlowFieldCellPositionBufferElement : IBufferElementData
    {
        public FFPosition Value;

        public static implicit operator FlowFieldCellPositionBufferElement(FFPosition value)
        {
            return new FlowFieldCellPositionBufferElement {Value = value};
        }

        public static implicit operator FFPosition(FlowFieldCellPositionBufferElement element)
        {
            return element.Value;
        }
    }
}