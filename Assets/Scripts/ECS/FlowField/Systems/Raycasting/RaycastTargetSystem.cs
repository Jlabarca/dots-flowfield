using ECSFlowField.PlayerInput;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ECSFlowField
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [DisableAutoCreation]
    public partial class RaycastTargetSystem : SystemBase
    {
        private Plane _rayCastPlane;
        private Mouse mouse;

        protected override void OnCreate()
        {
            _rayCastPlane = new Plane(Vector3.up, 0);
            mouse = Mouse.current;
        }

        protected override void OnUpdate()
        {
            mouse ??= Mouse.current;
            if (!mouse.leftButton.isPressed) return;

            var mousePixelCoords =  new Vector2(
                mouse.position.x.ReadValue(),
                mouse.position.y.ReadValue()
            );

            var ray = Camera.main.ScreenPointToRay(mousePixelCoords);

            // if (Physics.Raycast(ray.origin, ray.direction, out var hit, math.INFINITY ))
            // {
            //     var hitPoint = hit.point;
            //     Debug.DrawLine(ray.origin, hitPoint, Color.red, 0.001f);
            //
            //     Entities.ForEach((ref RaycastTargetComponent raycastTargetComponent) =>
            //     {
            //         raycastTargetComponent.HitPoint = hitPoint;
            //     }).Run();
            // }

            if (!_rayCastPlane.Raycast(ray, out var enter))
            {
                return;
            }

            var hitPoint = ray.GetPoint(enter);
            hitPoint.y = 0;

            Debug.DrawLine(ray.origin, hitPoint, Color.red, 1);

            Entities.ForEach((ref RaycastTargetComponent raycastTargetComponent) =>
            {
                raycastTargetComponent.HitPoint = hitPoint;
            }).WithoutBurst().Run();

            var playerInputSingleton = GetSingleton<PlayerInputComponent>();
            var fire1IsDown = mouse.leftButton.wasPressedThisFrame;
            var fire1IsHold =  mouse.leftButton.isPressed;
            var fire1IsUp =  mouse.leftButton.wasReleasedThisFrame;

            playerInputSingleton.PointerIsClick = fire1IsDown;
            playerInputSingleton.PointerIsHold = fire1IsHold;
            playerInputSingleton.PointerIsRelease = fire1IsUp;
            playerInputSingleton.PointerWorldPos = hitPoint;
            SetSingleton(playerInputSingleton);
        }
    }
}