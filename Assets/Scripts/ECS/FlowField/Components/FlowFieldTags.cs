using Unity.Entities;

namespace ECSFlowField
{
    public struct FFGrid_InitCellBuffersTag : IComponentData {}
    public struct FFGrid_CellBuffersInitializedTag : IComponentData {} // not used?
    public struct FFGrid_CalculateCellPositionsTag : IComponentData {}
    public struct FFNewTargetTag : IComponentData {}


    public struct FFGetCostFieldTag : IComponentData {}
    public struct FFGetIntegrationFieldTag : IComponentData {}
    public struct FFGetFlowFieldTag : IComponentData {}

}