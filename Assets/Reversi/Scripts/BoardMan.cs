using Unity.Entities;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.UILayout;
using Unity.Tiny.UIControls;
using Unity.Mathematics;

[UpdateAfter(typeof(IniBoard))]
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
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer),typeof(PointerInteraction) ,typeof(Button),typeof(GridComp)},
        };

        /*GetEntityQueryで取得した結果は自動的に開放されるため、Freeを行う処理を書かなくていいです。*/
        //作成したクエリの結果を取得します。
        GridEntity = GetEntityQuery(GirdEntityDesc);
    }

    protected override void OnUpdate()
    {
        if( ! (GridEntity.CalculateLength() > 0) )
        {
            return;
        }

        NativeArray<Entity> GridDatas = new NativeArray<Entity>(0, Allocator.Temp);

        GetGirdArray(ref GridDatas);

        Entities.With(GridEntity).ForEach((Entity EntityData, ref PointerInteraction GridClickData,ref GridComp GridData) =>
        {
            if (GridClickData.clicked == true)
            {
                if (CheckGridData(GridData.GridNum, ref GridDatas))
                {
                    return;
                }

                var Config = GetSingleton<GameState>();

                if (CheckCanPut(GridData.GridNum,Config.NowTurn,ref GridDatas))
                {
                    SetGridData(GridData.GridNum, Config.NowTurn, ref GridDatas);

                    Reverse(GridData.GridNum, Config.NowTurn, ref GridDatas);

                    Config.NowTurn = Config.NowTurn == 1 ? 2 : 1;

                    SetSingleton<GameState>(Config);
                }
            }
        });

        GridDatas.Dispose();
    }

    //Entityを各座標に対応させた順番に格納しなおします
    public bool  GetGirdArray(ref NativeArray<Entity> ReturnEntities)
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

        ReturnEntities = EntitiesArray;

        EntitiesArray.Dispose();
        return true;
    }


    //送られてきた座標にデータを書き換えます。
    //どのくらいこの関数で機能を持たせるかわかりませんが、
    //あくまでも書き込みだけで、駒の反転処理は別関数で行うかもしれません。
    public void SetGridData(int2 SetPos,int SetStatus,ref NativeArray<Entity> Entities)
    {
        if (SetPos.x < BoardSize && SetPos.y < BoardSize)
        {
            GridComp GridData = EntityManager.GetComponentData<GridComp>(Entities[SetPos.x + (SetPos.y * BoardSize)]);
            GridData.GridState = SetStatus;
            EntityManager.SetComponentData(Entities[SetPos.x + (SetPos.y * BoardSize)], GridData);

            RefreshBoardColor();
        }
    }

    //すでにデータがセットされているのか確認します
    //Trueの場合は置かれている
    //Falseの場合は置かれていない
    public bool CheckGridData(int2 SetPos,ref NativeArray<Entity> Entities)
    {
        if (SetPos.x < BoardSize && SetPos.y < BoardSize)
        {
            GridComp GridData = EntityManager.GetComponentData<GridComp>(Entities[SetPos.x + (SetPos.y * BoardSize)]);
            if(GridData.GridState==0)
            {
                return false;
            }
        }

        return true;
    }

    //クリックされた場所に駒を設置できるかどうか返します
    public bool CheckCanPut(int2 SetPos,int SetState ,ref NativeArray<Entity> Entities)
    {
        //１方向でも設置できればOK
        //上
        if(CheckPinch(SetPos,new int2(0,1), SetState, 0, ref Entities))
        {
            return true;
        }
        //下
        if (CheckPinch(SetPos, new int2(0, -1), SetState, 0, ref Entities))
        {
            return true;
        }
        //左
        if (CheckPinch(SetPos, new int2(-1, 0), SetState, 0, ref Entities))
        {
            return true;
        }
        //右
        if (CheckPinch(SetPos, new int2(1, 0), SetState, 0, ref Entities))
        {
            return true;
        }
        //右上
        if (CheckPinch(SetPos, new int2(1, 1), SetState, 0, ref Entities))
        {
            return true;
        }
        //右下
        if (CheckPinch(SetPos, new int2(1, -1), SetState, 0, ref Entities))
        {
            return true;
        }
        //左上
        if (CheckPinch(SetPos, new int2(-1, 1), SetState, 0, ref Entities))
        {
            return true;
        }
        //左下
        if (CheckPinch(SetPos, new int2(-1, -1), SetState, 0, ref Entities))
        {
            return true;
        }

        return false;
    }

    //挟んだ駒を反転します
    public void Reverse(int2 SetPos, int SetState, ref NativeArray<Entity> Entities)
    {
        //上
        CheckReverseState(SetPos, new int2(0, 1), SetState, 0, ref Entities);

        //下
        CheckReverseState(SetPos, new int2(0, -1), SetState, 0, ref Entities);

        //左
        CheckReverseState(SetPos, new int2(-1, 0), SetState, 0, ref Entities);

        //右
        CheckReverseState(SetPos, new int2(1, 0), SetState, 0, ref Entities);

        //右上
        CheckReverseState(SetPos, new int2(1, 1), SetState, 0, ref Entities);

        //右下
        CheckReverseState(SetPos, new int2(1, -1), SetState, 0, ref Entities);

        //左上
        CheckReverseState(SetPos, new int2(-1, 1), SetState, 0, ref Entities);

        //左下
        CheckReverseState(SetPos, new int2(-1, -1), SetState, 0, ref Entities);
    }

    //指定したグリッドのデータを取得します
    public int GetGridData(int2 CheckPos, ref NativeArray<Entity> Entities)
    {
        if (CheckPos.x < BoardSize && CheckPos.y < BoardSize)
        {
            GridComp GridData = EntityManager.GetComponentData<GridComp>(Entities[CheckPos.x + (CheckPos.y * BoardSize)]);
            return GridData.GridState;
        }

        return -1;
    }

    //指定ベクトル方向に挟めているのかチェックする
    public bool CheckPinch(int2 CheckPos,int2 CheckVector,int BaseState , int Count,ref NativeArray<Entity> Entities)
    {
        if (!(CheckPos.x < BoardSize && CheckPos.y < BoardSize))
        {
            return false;
        }

        if(GetGridData(CheckPos+CheckVector,ref Entities) == BaseState)
        {
            if(Count>0)
            {
                return true;
            }

            return false;
        }

        if (GetGridData(CheckPos + CheckVector, ref Entities) == 0 || GetGridData(CheckPos + CheckVector, ref Entities) == -1)
        {
            return false;
        }

        return CheckPinch(CheckPos+CheckVector, CheckVector,BaseState,++Count,ref Entities);
    }

    //駒が挟めているかチェックして、挟めていたら反転させる
    public bool CheckReverseState(int2 CheckPos, int2 CheckVector, int BaseState, int Count, ref NativeArray<Entity> Entities)
    {
        if (!(CheckPos.x < BoardSize && CheckPos.y < BoardSize))
        {
            return false;
        }

        if (GetGridData(CheckPos + CheckVector, ref Entities) == BaseState)
        {
            if (Count > 0)
            {
                SetGridData(CheckPos, BaseState, ref Entities);
                return true;
            }

            return false;
        }

        if (GetGridData(CheckPos + CheckVector, ref Entities) == 0 || GetGridData(CheckPos + CheckVector, ref Entities) == -1)
        {
            return false;
        }

        if (CheckReverseState(CheckPos + CheckVector, CheckVector, BaseState, ++Count, ref Entities))
        {
            SetGridData(CheckPos,BaseState,ref Entities);
            return true;
        }

        return false;
    }

    //盤面のデータを読み取り、色を再描画します。
    public bool RefreshBoardColor()
    {
        //Board自体の色
        Color TableColor_1 = new Color(0.4366531f, 0.853f, 0.423941f);
        Color TableColor_2 = new Color(0.3252917f, 0.6886792f, 0.3151032f);

        //駒の色
        Color White = new Color(0.9705882f, 0.9705882f, 0.9705882f);
        Color Black = new Color(0.1960784f, 0.1960784f, 0.1960784f);

        Entities.With(GridEntity).ForEach((Entity EntityData, ref Sprite2DRenderer Sprite2D, ref GridComp GridData) =>
        {
            //盤面に駒が何もなければそのグリッドに適したBoardの色を設定
            if (GridData.GridState==0)
            {
                int ColBaseNum = GridData.GridNum.y % 2 == 0 ? 0 : 1;

                Sprite2D.color = (ColBaseNum + GridData.GridNum.x) % 2 > 0 ? TableColor_1 : TableColor_2;
                return;
            }
            //そうでない場合、駒の色を描画

            Sprite2D.color = GridData.GridState == 1 ? Black : White;
        });

        return true;
    }
}
