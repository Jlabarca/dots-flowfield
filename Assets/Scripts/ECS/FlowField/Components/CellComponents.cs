using Unity.Entities;
using Unity.Mathematics;

namespace ECSFlowField.Cell
{
    [GenerateAuthoringComponent]
    public struct FFCost : IComponentData
    {
        public byte Value;
        public ushort BestCost;
    }

    [GenerateAuthoringComponent]
    public struct FFDirection : IComponentData
    {
        public float3 Value;
    }

    public struct FFPosition : IComponentData
    {
        public float3 WorldPos;
        public int3 FlowFieldCellIndex;
    }
}