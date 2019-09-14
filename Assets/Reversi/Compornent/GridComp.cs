using Unity.Entities;
using Unity.Mathematics;

public struct GridComp : IComponentData
{
    public int2 GridNum;

    //0‹ó”’@1”’@2•
    public int GridState;
}
