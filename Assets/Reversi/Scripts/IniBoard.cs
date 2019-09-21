using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.UILayout;
using Unity.Tiny.Core2D;
using Unity.Tiny.UIControls;
using Unity.Mathematics;
using Unity.Collections;


public class IniBoard : ComponentSystem
{

    EntityQueryDesc GridEntityDesc;
    EntityQuery GridEntity;

    const float GridSize = 42.5f;
    const float GridStartPos_X = 20;
    const float GridStartPos_Y = -80;
    const int BoardSize = 8;

    protected override void OnCreate()
    {
        /*ECSにおいて、クエリの作成はOnCreateで行うのが定石となっています*/

        GridEntityDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform),typeof(Sprite2DRenderer),typeof(GridComp) },
        };


        /*GetEntityQueryで取得した結果は自動的に開放されるため、Freeを行う処理を書かなくていいです。*/
        //作成したクエリの結果を取得します。
        GridEntity = GetEntityQuery(GridEntityDesc);
    }


    protected override void OnUpdate()
    {
        var GameStats = World.TinyEnvironment();
        var Config = GameStats.GetConfigData<BoardState>();
        if (Config.EmitBoard==false)
        {
            EmitBoard();
            Config.EmitBoard = true;
            GameStats.SetConfigData(Config);
        }
    }

    //盤面を作成します
    public void EmitBoard()
    {
        var GameStats = World.TinyEnvironment();
        var Config = GameStats.GetConfigData<BoardState>();

        Color TableColor_1=new Color(0.4366531f, 0.853f, 0.423941f);
        Color TableColor_2=new Color(0.3252917f, 0.6886792f, 0.3151032f);

        Entities.With(GridEntity).ForEach((Entity EntityData, ref Sprite2DRenderer Sprite2D, ref GridComp GridData) =>
        {
            int ColBaseNum = GridData.GridNum.y % 2 == 0 ? 0 : 1;

            Sprite2D.color = (ColBaseNum + GridData.GridNum.x) % 2 > 0 ? TableColor_1 : TableColor_2;

            GridData.GridState = 0;
        });
    }
}
