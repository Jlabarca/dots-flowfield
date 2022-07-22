using ECSFlowField.Cell;
using Unity.Entities;

namespace ECSFlowField
{
    [InternalBufferCapacity(250)]
    public struct FlowFieldCellDirectionBufferElement : IBufferElementData
    {
        public FFDirection Value;

        public static implicit operator FlowFieldCellDirectionBufferElement(FFDirection value)
        {
            return new FlowFieldCellDirectionBufferElement {Value = value};
        }

        public static implicit operator FFDirection(FlowFieldCellDirectionBufferElement element)
        {
            return element.Value;
        }
    }
}