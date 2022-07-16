using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.PlayerInputDataPointer
{
    public partial class PlayerInputDataPointerStateSystem : SystemBase
    {
        private const string _MainInputName = "Fire1";

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<PlayerInputComponent>();
        }

        protected override void OnUpdate()
        {
            var mouse = Mouse.current;

            var playerInputSingleton = GetSingleton<PlayerInputComponent>();
            var fire1IsDown = mouse.leftButton.wasPressedThisFrame;
            var fire1IsHold =  mouse.leftButton.isPressed;
            var fire1IsUp =  mouse.leftButton.wasReleasedThisFrame;

            playerInputSingleton.PointerIsClick = fire1IsDown;
            playerInputSingleton.PointerIsHold = fire1IsHold;
            playerInputSingleton.PointerIsRelease = fire1IsUp;

            SetSingleton(playerInputSingleton);
        }
    }
}