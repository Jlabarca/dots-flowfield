// using Unity.Burst;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Collections;
// using Unity.Physics;
// using Unity.Physics.Systems;
// using UnityEngine;
//
// [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
// public class RaycastUnderPointerSystem : JobComponentSystem
// {
//     private BuildPhysicsWorld _buildPhysicsWorldSystem;
//     private EndFramePhysicsSystem _endFramePhysicsSystem;
//     private NativeQueue<UnderPointerEvent> _eventQueue;
//
//     protected override void OnStartRunning()
//     {
//         _buildPhysicsWorldSystem = World.GetExistingSystem<BuildPhysicsWorld>();
//         _endFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();
//         _eventQueue = World.GetExistingSystem<EntityEventSystem>().GetOrCreateEventQueue<UnderPointerEvent>();
//     }
//
//     protected override JobHandle OnUpdate(JobHandle inputDeps)
//     {
//         if (Input.GetMouseButton(0))
//         {
//             inputDeps = JobHandle.CombineDependencies(inputDeps, _buildPhysicsWorldSystem.FinalJobHandle);
//
//             var screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
//             var selectionSettings = GetSingleton<SelectionSettings>();
//
//             var handle = new FindClosestInteractable
//             {
//                 Input = new RaycastInput
//                 {
//                     Start = screenRay.origin,
//                     End = screenRay.GetPoint(100),
//                     Filter = new CollisionFilter
//                     {
//                         BelongsTo = selectionSettings.SelectionCategory,
//                         CollidesWith = ~0u,
//                         GroupIndex = 0
//                     }
//                 },
//                 EventQueue = _eventQueue.ToConcurrent(),
//                 World = _buildPhysicsWorldSystem.PhysicsWorld
//
//             }.Schedule(inputDeps);
//
//             _endFramePhysicsSystem.HandlesToWaitFor.Add(handle);
//
//             return handle;
//         }
//
//         return inputDeps;
//     }
//
//     [BurstCompile]
//     public struct FindClosestInteractable : IJob
//     {
//         [ReadOnly] public PhysicsWorld World;
//         [ReadOnly] public RaycastInput Input;
//
//         public NativeQueue<UnderPointerEvent>.Concurrent EventQueue;
//
//         public unsafe void Execute()
//         {
//             if (World.CollisionWorld.CastRay(Input, out var hit))
//             {
//                 Entity entity = World.Bodies[hit.RigidBodyIndex].Entity;
//                 EventQueue.Enqueue(new UnderPointerEvent
//                 {
//                     Entity = entity,
//                     Hit = hit,
//                 });
//             }
//         }
//     }
//
// }