using Unity.Entities;
using Unity.Collections;
using Unity.Tiny.Core2D;
using Unity.Tiny.UILayout;
using Unity.Tiny.UIControls;
using Unity.Mathematics;

public class BoardMan : ComponentSystem
{
    const int BoardSize = 8;


    EntityQueryDesc GirdEntityDesc;
    EntityQuery GridEntity;

    EntityQueryDesc CanvasDesc;
    EntityQuery CanvasEntity;

    protected override void OnCreate()
    {
        /*ECSにおいて、クエリの作成はOnCreateで行うのが定石となっています*/

        GirdEntityDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer),typeof(PointerInteraction) ,typeof(Button)},
        };

        /*GetEntityQueryで取得した結果は自動的に開放されるため、Freeを行う処理を書かなくていいです。*/
        //作成したクエリの結果を取得します。
        GridEntity = GetEntityQuery(GirdEntityDesc);
    }

    protected override void OnUpdate()
    {
        Entities.With(GridEntity).ForEach((Entity EntityData, ref PointerInteraction GridClickData,ref GridComp GridData) =>
        {
            if (GridClickData.clicked == true)
            {
                //クリックされたところのGridCompのNumが座標です。
            }
        });
    }

    //Entityを各座標に対応させた順番に格納しなおします
    public NativeArray<Entity>  GetGirdArray()
    {
        NativeArray<Entity> EntitiesArray = new NativeArray<Entity>(BoardSize * BoardSize, Allocator.Temp);

        for (int i = 0; i < BoardSize * BoardSize; ++i)
        {
            EntitiesArray[i] = Entity.Null;
        }


        Entities.With(GridEntity).ForEach((Entity EntityData, ref GridComp GridData) =>
        {
            if (GridData.GridNum.x < BoardSize && GridData.GridNum.y < BoardSize)
            {
                EntitiesArray[GridData.GridNum.x + (GridData.GridNum.y * BoardSize)] = EntityData;
            }
        });

        return EntitiesArray;
    }


    //送られてきた座標にデータを書き換えます。
    //どのくらいこの関数で機能を持たせるかわかりませんが、
    //あくまでも書き込みだけで、駒の反転処理は別関数で行うかもしれません。
    public void SetGridData(int2 SetPos,int SetStatus,ref NativeArray<Entity> Entities)
    {
        if (SetPos.x < BoardSize && SetPos.y < BoardSize)
        {
            //Entities[SetPos.x + (SetPos.y * BoardSize)];
        }
    }
}
