using ECSFlowField.Cell;
using Unity.Entities;

namespace ECSFlowField
{
    [InternalBufferCapacity(250)]
    public struct FlowFieldCellCostBufferElement : IBufferElementData
    {
        public FFCost Value;

        public static implicit operator FlowFieldCellCostBufferElement(FFCost value)
        {
            return new FlowFieldCellCostBufferElement {Value = value};
        }

        public static implicit operator FFCost(FlowFieldCellCostBufferElement element)
        {
            return element.Value;
        }
    }
}