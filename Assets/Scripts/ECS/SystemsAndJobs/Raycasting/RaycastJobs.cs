using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Physics;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace TopDownCharacterController.Project.Scripts.ECS.SystemsAndJobs.Raycasting
{
    public static class RaycastJobs
    {
        [BurstCompile]
        public struct RaycastJobParallel : IJobParallelFor
        {
            [ReadOnly] public CollisionWorld World;
            [ReadOnly] public NativeArray<RaycastInput> Inputs;
            
            public NativeArray<RaycastHit> Results;

            public void Execute(int index)
            {
                World.CastRay(Inputs[index], out var hit);
                Results[index] = hit;
            }
        }
        
        public static JobHandle ScheduleBatchRayCast(
            CollisionWorld world, NativeArray<RaycastInput> inputs, ref NativeArray<RaycastHit> results, JobHandle dependency = default)
        {
            var jobHandle = new RaycastJobParallel
            {
                World = world,
                Inputs = inputs,
                Results = results
            }.Schedule(inputs.Length, 5);
            return jobHandle;
        }
        
        [BurstCompile]
        public struct BoxcastJobParallel : IJobParallelFor
        {
            [ReadOnly] public CollisionWorld World;
            [ReadOnly] public CollisionFilter Filter;
            [ReadOnly] public NativeArray<BoxcastCommand> Inputs;
            
            public NativeArray<ColliderCastHit> Results;

            public void Execute(int index)
            {
                var inputs = Inputs[index];
                var center = inputs.center;
                var orientation = inputs.orientation;
                var halfExtents = inputs.halfExtents;
                var direction = inputs.direction;
                var distance = inputs.distance;

                World.BoxCast(center, orientation, halfExtents, direction, distance, out var hit, Filter);
                
                Results[index] = hit;
            }
        }
        
        public static JobHandle ScheduleBatchBoxCast(
            CollisionWorld world, CollisionFilter filter, NativeArray<BoxcastCommand> inputs, ref NativeArray<ColliderCastHit> results, JobHandle dependency = default)
        {
            var jobHandle = new BoxcastJobParallel
            {
                World = world,
                Filter = filter,
                Inputs = inputs,
                Results = results
            }.Schedule(inputs.Length, 5, dependency);
            return jobHandle;
        }
    }
}