using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField;
using TopDownCharacterController.Project.Scripts.ECS.ComponentsAndTags.FlowField.FlowFieldCellBuffers;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TopDownCharacterController.Project.Scripts.Helpers
{
    public class FlowFieldHelper : MonoBehaviour
    {
        public static FlowFieldHelper Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);

                return;
            }

            Instance = this;
        }

        public static void TryAddCell( int3 newCellIndex, int3 flowFieldSize, ref NativeList<int3> neighbors )
        {
            var isOutOfBounds = newCellIndex.x < 0 || newCellIndex.x > flowFieldSize.x - 1 || newCellIndex.y < 0 || newCellIndex.y > flowFieldSize.y  - 1 || newCellIndex.z < 0 || newCellIndex.z > flowFieldSize.z - 1;
            if (isOutOfBounds) return;

            neighbors.Add(newCellIndex);
        }

        public static int FindCellBufferIndex(DynamicBuffer<FlowFieldCellPosition> flowFieldCellPositionBuffer, int3 index)
        {
            for (var i = 0; i < flowFieldCellPositionBuffer.Length; i++)
            {
                if (flowFieldCellPositionBuffer[i].FlowFieldCellIndex.Equals(index))
                {
                    return i;
                }
            }

            return -1;
        }

        private void OnDrawGizmos()
        {
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                return;
            }

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = em.CreateEntityQuery(typeof(FlowFieldComponent));

            Entity flowFieldEntity = default;
            try
            {
                flowFieldEntity = query.GetSingletonEntity();
            }
            catch
            {
                return;
            }

            if (flowFieldEntity.Equals(Entity.Null)) return;

            var flowFieldComponent = query.GetSingleton<FlowFieldComponent>();

            if (query.IsEmpty)
            {
                return;
            }

            var hasFlowFieldCellPositionBufferElement = em.HasComponent<FlowFieldCellPositionBufferElement>(flowFieldEntity);
            var hasFlowFieldCellCostBufferElement = em.HasComponent<FlowFieldCellCostBufferElement>(flowFieldEntity);
            var hasFlowFieldCellDirectionBufferElement = em.HasComponent<FlowFieldCellDirectionBufferElement>(flowFieldEntity);

            if (!hasFlowFieldCellPositionBufferElement || !hasFlowFieldCellCostBufferElement || !hasFlowFieldCellDirectionBufferElement)
            {
                return;
            }

            var positionsBufferElement = em.GetBuffer<FlowFieldCellPositionBufferElement>(flowFieldEntity, true);
            var costBufferElement = em.GetBuffer<FlowFieldCellCostBufferElement>(flowFieldEntity, true);
            var directionBufferElement = em.GetBuffer<FlowFieldCellDirectionBufferElement>(flowFieldEntity, true);

            var positionBuffer = positionsBufferElement.Reinterpret<FlowFieldCellPosition>();
            var costBuffer = costBufferElement.Reinterpret<FlowFieldCellCost>();
            var directionBuffer = directionBufferElement.Reinterpret<FlowFieldCellDirection>();

            for (var i = 0; i < flowFieldComponent.CellCount; i++)
            {
                var cellPosition = positionBuffer[i].WorldPos;
                var cellCost = costBuffer[i];

                var cost = cellCost.Cost;

                if (cost == byte.MaxValue)
                {
                    continue;
                }

                var color = Color.Lerp(Color.green, Color.red, math.unlerp(1, 255, cost));

                if (cost <= 0)
                {
                    color = Color.white;
                }

                var bestDirection = directionBuffer[i].BestDirection;

                if (bestDirection.Equals(float3.zero))
                {
                    continue;
                }

                DrawArrow.ForGizmo(cellPosition - bestDirection * 0.25f, bestDirection*0.5f, color, 0.25f, 35f);


                // Handles.Label(cellPosition.WorldPos, bestCost.ToString(), new GUIStyle
                // {
                //     alignment = TextAnchor.MiddleCenter,
                //     clipping = TextClipping.Overflow,
                //
                //     normal = new GUIStyleState
                //     {
                //         textColor = color,
                //     }
                // });
            }
        }

    }
}