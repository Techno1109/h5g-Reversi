using Unity.Entities;

public struct BoardState : IComponentData
{
    //このコンポーネントで盤面の状態を格納できるようにします

    public bool EmitBoard;
}
