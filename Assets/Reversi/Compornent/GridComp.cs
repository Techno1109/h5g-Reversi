using Unity.Entities;
using Unity.Mathematics;

public struct GridComp : IComponentData
{
    public int2 GridNum;

    //0���󔒁@1�����@2����
    public int GridState;
}
