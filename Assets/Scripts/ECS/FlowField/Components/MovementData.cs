using Unity.Entities;

namespace TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField
{
    [GenerateAuthoringComponent]
    public struct MovementData : IComponentData
    {
        public float Speed;
    }
}