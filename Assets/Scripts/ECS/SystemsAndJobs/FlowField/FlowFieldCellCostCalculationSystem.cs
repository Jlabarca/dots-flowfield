using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.Tags;
using TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.Raycasting;
using TopDownCharacterController.Project.Scripts.Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;
using Collider = Unity.Physics.Collider;

namespace TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.FlowField
{
    [UpdateAfter(typeof(FlowFieldCellBufferInitializationSystem))]
    [UpdateAfter(typeof(FlowFieldCellPositionCalculationSystem))]
    public partial class FlowFieldCellCostCalculationSystem : SystemBase
    {
        private EndInitializationEntityCommandBufferSystem _endInitializationEntityCommandBufferSystem;
        
        private BuildPhysicsWorld _buildPhysicsWorld;
        private StepPhysicsWorld _stepPhysicsWorld;
        
        protected override void OnCreate()
        {
            _endInitializationEntityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
            _buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
            _stepPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<StepPhysicsWorld>();
        }

        protected override void OnUpdate()
        {
            var ecb = _endInitializationEntityCommandBufferSystem.CreateCommandBuffer();

            Entities
                .WithAll<RecalculateFlowFieldCostRequestTag>()
                .ForEach((Entity e, ref FlowFieldComponent flowFieldComponent,
                    ref DynamicBuffer<FlowFieldCellPositionBufferElement> flowFieldCellPositionBuffer,
                    ref DynamicBuffer<FlowFieldCellCostBufferElement> flowFieldCellCostBuffer) =>
            {
                ecb.RemoveComponent<RecalculateFlowFieldCostRequestTag>(e);

                var cellCount = flowFieldComponent.CellCount;
                var cellRadius = flowFieldComponent.CellRadius;
                
                // Reinterpret Buffers
                var cellPositionBuffer = flowFieldCellPositionBuffer.Reinterpret<FlowFieldCellPosition>();
                var cellCostBuffer = flowFieldCellCostBuffer.Reinterpret<FlowFieldCellCost>();
                
                // FlowFieldBoxcastJob
                // var boxcastInputs = new NativeArray<BoxcastCommand>(cellCount, Allocator.TempJob);
                // var boxcastHitResults = new NativeArray<ColliderCastHit>(cellCount, Allocator.TempJob);
                // _stepPhysicsWorld.FinalSimulationJobHandle.Complete(); // Complete FinalSimulationJob before ray casting on multiple threads
                // var flowFieldRaycastJobHandle = 
                //     ScheduleFlowFieldBoxcastJob(_buildPhysicsWorld.PhysicsWorld.CollisionWorld, cellPositionBuffer, cellRadius, boxcastInputs, ref boxcastHitResults, _stepPhysicsWorld.FinalSimulationJobHandle);
                // flowFieldRaycastJobHandle.Complete();

                
                // FlowFieldRaycastJob
                var raycastInputs = new NativeArray<RaycastInput>(cellCount, Allocator.TempJob);
                var raycastHitResults = new NativeArray<RaycastHit>(cellCount, Allocator.TempJob);
                _stepPhysicsWorld.FinalSimulationJobHandle.Complete(); // Complete FinalSimulationJob before ray casting on multiple threads
                var flowFieldRaycastJobHandle = 
                    ScheduleFlowFieldRaycastJob(_buildPhysicsWorld.PhysicsWorld.CollisionWorld, cellPositionBuffer, cellRadius, raycastInputs, ref raycastHitResults, _stepPhysicsWorld.FinalSimulationJobHandle);
                flowFieldRaycastJobHandle.Complete();
                
                // var colliders = new NativeArray<Collider>(boxcastHitResults.Length, Allocator.TempJob);
                // var raycastHitResultProcessJobHandle = new BoxcastHitResultProcessJob
                // {
                //     CollisionWorld = _buildPhysicsWorld.PhysicsWorld.CollisionWorld,
                //     BoxcastHits = boxcastHitResults,
                //     Colliders = colliders
                // }.Schedule(boxcastHitResults.Length, 10, flowFieldRaycastJobHandle);
                // raycastHitResultProcessJobHandle.Complete();
                
                // RaycastHitProcessJob
                var colliders = new NativeArray<Collider>(raycastInputs.Length, Allocator.TempJob);
                var raycastHitResultProcessJobHandle = new RaycastHitResultProcessJob
                {
                    CollisionWorld = _buildPhysicsWorld.PhysicsWorld.CollisionWorld,
                    RaycastHits = raycastHitResults,
                    Colliders = colliders
                }.Schedule(raycastInputs.Length, 5, flowFieldRaycastJobHandle);
                raycastHitResultProcessJobHandle.Complete();
                
                
                // FlowFieldCellCostEvaluationJob
                var costResults = new NativeArray<(float3,byte)>(cellCount, Allocator.TempJob);
                var flowFieldCellCostEvaluationJobHandle = new FlowFieldCellCostEvaluationJob
                {
                    Colliders = colliders,
                    PositionsBuffer = cellPositionBuffer,
                    CostResults = costResults
                }.Schedule(cellCount, 5, 
                    JobHandle.CombineDependencies(flowFieldRaycastJobHandle, raycastHitResultProcessJobHandle));
                flowFieldCellCostEvaluationJobHandle.Complete();
                
                // Update Buffers
                for (var i = 0; i < cellCount; i++)
                {
                    var cellWorldPos = cellPositionBuffer[i].WorldPos;
                    var cellFlowFieldIndex = cellPositionBuffer[i].FlowFieldCellIndex;
                
                    byte cellCost = 1;
                
                    for (var j = 0; j < costResults.Length; j++)
                    {
                        var (worldPos, cost) = costResults[i];
                
                        if (cellWorldPos.Equals(worldPos))
                        {
                            cellCost = cost;
                            break;
                        }
                    }
                
                    var flatIndex = FlowFieldHelper.FindCellBufferIndex(cellPositionBuffer, cellFlowFieldIndex);
                    var tmpCellCostComponent = cellCostBuffer[flatIndex];
                    tmpCellCostComponent.Cost = cellCost;
                    tmpCellCostComponent.BestCost = ushort.MaxValue;
                    cellCostBuffer[flatIndex] = tmpCellCostComponent;
                }

                // boxcastInputs.Dispose();
                // boxcastHitResults.Dispose();

                raycastInputs.Dispose();
                raycastHitResults.Dispose();
                colliders.Dispose();
                costResults.Dispose();
            }).WithoutBurst().Run();
        }
        
        // private static JobHandle ScheduleFlowFieldBoxcastJob(
        //     CollisionWorld collisionWorld, DynamicBuffer<FlowFieldCellPosition> positionsBuffer, float cellRadius, NativeArray<BoxcastCommand> inputs, ref NativeArray<ColliderCastHit> results, JobHandle cellWorldPositionsCalcJobHandle)
        // {
        //     var collisionFilter = new CollisionFilter
        //     {
        //         BelongsTo = ~0u,
        //         CollidesWith = ~0u,
        //         GroupIndex = 0
        //     };
        //
        //     var cellHalfExtents = Vector3.one * cellRadius * 0.75f;
        //     
        //     for (var i = 0; i < positionsBuffer.Length; i++)
        //     {
        //         var worldPos = positionsBuffer[i].WorldPos;
        //
        //         inputs[i] = new BoxcastCommand
        //         {
        //             center = worldPos,
        //             halfExtents = cellHalfExtents,
        //             orientation = Quaternion.identity,
        //         };
        //     }
        //     
        //     var jobHandle = RaycastJobs.ScheduleBatchBoxCast(collisionWorld, collisionFilter, inputs, ref results, cellWorldPositionsCalcJobHandle);
        //
        //     return jobHandle;
        // }
        
        private static JobHandle ScheduleFlowFieldRaycastJob(
            CollisionWorld collisionWorld, DynamicBuffer<FlowFieldCellPosition> cellPositionsBuffer, float cellRadius, NativeArray<RaycastInput> inputs, ref NativeArray<RaycastHit> results, JobHandle cellWorldPositionsCalcJobHandle)
        {
            var collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0
            };
            
            for (var i = 0; i < cellPositionsBuffer.Length; i++)
            {
                var worldPos = cellPositionsBuffer[i].WorldPos;
                var startPos = worldPos + (float3)Vector3.up * cellRadius;
                var endPos = startPos + (float3)Vector3.down * cellRadius;
        
                inputs[i] = new RaycastInput
                {
                    Filter = collisionFilter,
                    Start = startPos,
                    End = endPos
                };
            }
            
            var jobHandle = RaycastJobs.ScheduleBatchRayCast(collisionWorld, inputs, ref results, cellWorldPositionsCalcJobHandle);
        
            return jobHandle;
        }
        
        
        // [BurstCompile]
        // private struct BoxcastHitResultProcessJob : IJobParallelFor
        // {
        //     [ReadOnly] public CollisionWorld CollisionWorld;
        //     [ReadOnly] public NativeArray<ColliderCastHit> BoxcastHits;
        //     
        //     public NativeArray<Collider> Colliders;
        //
        //     public void Execute(int index)
        //     {
        //         Colliders[index] = CollisionWorld.Bodies[BoxcastHits[index].RigidBodyIndex].Collider.Value;
        //     }
        // }
        
        [BurstCompile]
        private struct RaycastHitResultProcessJob : IJobParallelFor
        {
            [ReadOnly] public CollisionWorld CollisionWorld;
            [ReadOnly] public NativeArray<RaycastHit> RaycastHits;
            
            public NativeArray<Collider> Colliders;
        
            public void Execute(int index)
            {
                Colliders[index] = CollisionWorld.Bodies[RaycastHits[index].RigidBodyIndex].Collider.Value;
            }
        }
        
        [BurstCompile]
        private struct FlowFieldCellCostEvaluationJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Collider> Colliders;
            [ReadOnly] public DynamicBuffer<FlowFieldCellPosition> PositionsBuffer;
            
            public NativeArray<(float3, byte)> CostResults;
        
            public void Execute(int index)
            {
                var worldPos = PositionsBuffer[index].WorldPos;
                CostResults[index] = (worldPos, 1);
                
                var isObstacle = Colliders[index].Filter.BelongsTo == (ulong) CollisionLayer.Obstacle;
                var isGround = Colliders[index].Filter.BelongsTo == (ulong) CollisionLayer.Ground;
                
                if (isObstacle )
                {
                    CostResults[index] = (worldPos, byte.MaxValue);
                }
                else
                {
                    CostResults[index] = (worldPos, 1);
                }
            }
        }
    
    }
}