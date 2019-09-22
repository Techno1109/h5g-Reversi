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
        if(HasSingleton<BoardState>()==false)
        {
            return;
        }
        var Config = GetSingleton<BoardState>();
        if (GetSingleton<BoardState>().EmitBoard == false)
        {
            EmitBoard();
            Config.EmitBoard = true;
            SetSingleton<BoardState>(Config);
        }
    }

    //盤面を作成します
    public void EmitBoard()
    {
        Entities.With(GridEntity).ForEach((Entity EntityData, ref Sprite2DRenderer Sprite2D, ref GridComp GridData) =>
        {
            GridData.GridState = 0;
            if ( (GridData.GridNum.y == 3 || GridData.GridNum.y == 4) && (GridData.GridNum.x == 3 || GridData.GridNum.x == 4))
            {
                if (GridData.GridNum.x == 3)
                {
                    GridData.GridState = 2;
                }
                if (GridData.GridNum.x == 4)
                {
                    GridData.GridState = 1;
                }

                if (GridData.GridNum.y == 4)
                {
                    GridData.GridState = GridData.GridState == 1 ? 2 : 1;
                }
            }
        });

        EntityManager.World.GetExistingSystem<BoardMan>().RefreshBoardColor();
    }
}
