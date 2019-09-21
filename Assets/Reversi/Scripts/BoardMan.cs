using Unity.Entities;
using Unity.Collections;
using Unity.Tiny.Core;
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
        /*ECS�ɂ����āA�N�G���̍쐬��OnCreate�ōs���̂���΂ƂȂ��Ă��܂�*/

        GirdEntityDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer),typeof(PointerInteraction) ,typeof(Button),typeof(GridComp)},
        };

        /*GetEntityQuery�Ŏ擾�������ʂ͎����I�ɊJ������邽�߁AFree���s�������������Ȃ��Ă����ł��B*/
        //�쐬�����N�G���̌��ʂ��擾���܂��B
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

                var GameStats = World.TinyEnvironment();
                var Config = GameStats.GetConfigData<GameState>();
                SetGridData(GridData.GridNum,Config.NowTurn,ref GridDatas);

                Config.NowTurn = Config.NowTurn == 1 ? 2 : 1;
            }
        });

        GridDatas.Dispose();
    }

    //Entity���e���W�ɑΉ����������ԂɊi�[���Ȃ����܂�
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


    //�����Ă������W�Ƀf�[�^�����������܂��B
    //�ǂ̂��炢���̊֐��ŋ@�\���������邩�킩��܂��񂪁A
    //�����܂ł��������݂����ŁA��̔��]�����͕ʊ֐��ōs����������܂���B
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

    //���łɃf�[�^���Z�b�g����Ă���̂��m�F���܂�
    //True�̏ꍇ�͒u����Ă���
    //False�̏ꍇ�͒u����Ă��Ȃ�
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

    //�Ֆʂ̃f�[�^��ǂݎ��A�F���ĕ`�悵�܂��B
    public bool RefreshBoardColor()
    {
        //Board���̂̐F
        Color TableColor_1 = new Color(0.4366531f, 0.853f, 0.423941f);
        Color TableColor_2 = new Color(0.3252917f, 0.6886792f, 0.3151032f);

        //��̐F
        Color White = new Color(0.9705882f, 0.9705882f, 0.9705882f);
        Color Black = new Color(0.1960784f, 0.1960784f, 0.1960784f);

        Entities.With(GridEntity).ForEach((Entity EntityData, ref Sprite2DRenderer Sprite2D, ref GridComp GridData) =>
        {
            //�Ֆʂɋ�����Ȃ���΂��̃O���b�h�ɓK����Board�̐F��ݒ�
            if (GridData.GridState==0)
            {
                int ColBaseNum = GridData.GridNum.y % 2 == 0 ? 0 : 1;

                Sprite2D.color = (ColBaseNum + GridData.GridNum.x) % 2 > 0 ? TableColor_1 : TableColor_2;
                return;
            }
            //�����łȂ��ꍇ�A��̐F��`��

            Sprite2D.color = GridData.GridState == 1 ? Black : White;
        });

        return true;
    }
}
