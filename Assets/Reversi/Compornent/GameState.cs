using Unity.Entities;
using Unity.Tiny.Core2D;

public struct GameState : IComponentData
{
    //ゲームが進行中なのか、終了しているのか格納します
    public bool IsActive;

    //  どちらのターンなのかを格納します
    public int NowTurn;
}
