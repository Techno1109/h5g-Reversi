using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.UILayout;
using Unity.Tiny.Core2D;
using Unity.Mathematics;
using Unity.Collections;


public class IniBoard : ComponentSystem
{

    EntityQueryDesc PrefabGirdEntityDesc;
    EntityQuery PrefabGridEntity;

    EntityQueryDesc CanvasDesc;
    EntityQuery CanvasEntity;

    const float GridSize = 42.5f;
    const float GridStartPos_X = 20;
    const float GridStartPos_Y = 40;
    const int BoardSize = 8;

    protected override void OnCreate()
    {
        /*ECSにおいて、クエリの作成はOnCreateで行うのが定石となっています*/

        PrefabGirdEntityDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Prefab), typeof(RectTransform),typeof(Sprite2DRenderer) },
        };

        CanvasDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] {typeof(UICanvas)},
        };
    

        /*GetEntityQueryで取得した結果は自動的に開放されるため、Freeを行う処理を書かなくていいです。*/
        //作成したクエリの結果を取得します。
        PrefabGridEntity = GetEntityQuery(PrefabGirdEntityDesc);
        CanvasEntity = GetEntityQuery(CanvasDesc);
    }


    protected override void OnUpdate()
    {
        var GameStats = World.TinyEnvironment();
        var Config = GameStats.GetConfigData<BoardState>();

        if(Config.EmitBoard==false)
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

        Color TableColor_1=new Color(111,218,30,108);
        Color TableColor_2=new Color(214, 238, 65, 1);

        NativeArray<Entity> Canvas = CanvasEntity.ToEntityArray(Allocator.TempJob);

        if (Config.EmitBoard==false)
        {
            NativeArray<Entity> PrefabEntity = PrefabGridEntity.ToEntityArray(Allocator.TempJob);
            for(int X =0; X<GridSize;X++)
            {
                for(int Y = 0; Y < GridSize; Y++)
                {
                    Entity EmitEntity = EntityManager.Instantiate(PrefabEntity[0]);
                    EntityManager.SetComponentData(EmitEntity,new RectTransform { anchoredPosition={ x=GridStartPos_X+(GridSize*X) , y = GridStartPos_Y + (GridSize*Y) } });

                    GridComp TargetGridComp = EntityManager.GetComponentData<GridComp>(EmitEntity);
                    TargetGridComp.GridNum.x = X;
                    TargetGridComp.GridNum.y = Y;
                    TargetGridComp.GridState = 0;
                    EntityManager.SetComponentData(EmitEntity, TargetGridComp);


                    Sprite2DRenderer TargetRenderer = EntityManager.GetComponentData<Sprite2DRenderer>(EmitEntity);

                    int ColBaseNum = Y % 2 == 0 ? 0:1;

                    TargetRenderer.color = (ColBaseNum + Y) % 2 > 0 ?TableColor_1 :TableColor_2 ;

                    EntityManager.SetComponentData(EmitEntity, TargetRenderer);

                    EntityManager.SetComponentData(EmitEntity, new Parent { Value = Canvas[0] });
                }
            }
        }
    }
}
