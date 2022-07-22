using Unity.Entities;
using Unity.Mathematics;

namespace ECSFlowField
{
    [GenerateAuthoringComponent]
    public struct FlowFieldComponent : IComponentData
    {
        public int3 FieldSize;
        public float CellRadius;
        public int3 FlowTargetPoint;
        public bool IsBuilt;
        public int CellCount => FieldSize.x * FieldSize.y * FieldSize.z;
        public float CellDiameter => CellRadius * 2;

    }
}