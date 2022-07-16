using Unity.Entities;
using Unity.Mathematics;

namespace TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField
{
    [GenerateAuthoringComponent]
    public struct FlowFieldComponent : IComponentData
    {
        public int3 FieldSize;
        public float CellRadius;
        public float CellDiameter => CellRadius * 2;
        public int3 FlowTargetPoint;
        public int CellCount => FieldSize.x * FieldSize.y * FieldSize.z;
        public bool IsBuilt;
    }
}