using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags;
using TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.Raycasting;
using Unity.Entities;

namespace TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.PlayerInputDataPointer
{
    [UpdateAfter(typeof(RaycastTargetSystem))]
    public partial class PlayerInputDataPointerWorldPosSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<RaycastTargetComponent>();
            RequireSingletonForUpdate<PlayerInputComponent>();
        }

        protected override void OnUpdate()
        {
            var playerInputSingleton = GetSingleton<PlayerInputComponent>();
            var hitPoint = GetSingleton<RaycastTargetComponent>().HitPoint;
            
            playerInputSingleton.PointerWorldPos = hitPoint;
            
            SetSingleton(playerInputSingleton);
        }
    }
}