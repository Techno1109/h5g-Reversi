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

    //�N���b�N���ꂽ�ꏊ�ɋ��ݒu�ł��邩�ǂ����Ԃ��܂�
    public bool CheckCanPut(int2 SetPos,int SetState ,ref NativeArray<Entity> Entities)
    {
        //�P�����ł��ݒu�ł����OK
        //��
        if(CheckPinch(SetPos,new int2(0,1), SetState, 0, ref Entities))
        {
            return true;
        }
        //��
        if (CheckPinch(SetPos, new int2(0, -1), SetState, 0, ref Entities))
        {
            return true;
        }
        //��
        if (CheckPinch(SetPos, new int2(-1, 0), SetState, 0, ref Entities))
        {
            return true;
        }
        //�E
        if (CheckPinch(SetPos, new int2(1, 0), SetState, 0, ref Entities))
        {
            return true;
        }
        //�E��
        if (CheckPinch(SetPos, new int2(1, 1), SetState, 0, ref Entities))
        {
            return true;
        }
        //�E��
        if (CheckPinch(SetPos, new int2(1, -1), SetState, 0, ref Entities))
        {
            return true;
        }
        //����
        if (CheckPinch(SetPos, new int2(-1, 1), SetState, 0, ref Entities))
        {
            return true;
        }
        //����
        if (CheckPinch(SetPos, new int2(-1, -1), SetState, 0, ref Entities))
        {
            return true;
        }

        return false;
    }

    //���񂾋�𔽓]���܂�
    public void Reverse(int2 SetPos, int SetState, ref NativeArray<Entity> Entities)
    {
        //��
        CheckReverseState(SetPos, new int2(0, 1), SetState, 0, ref Entities);

        //��
        CheckReverseState(SetPos, new int2(0, -1), SetState, 0, ref Entities);

        //��
        CheckReverseState(SetPos, new int2(-1, 0), SetState, 0, ref Entities);

        //�E
        CheckReverseState(SetPos, new int2(1, 0), SetState, 0, ref Entities);

        //�E��
        CheckReverseState(SetPos, new int2(1, 1), SetState, 0, ref Entities);

        //�E��
        CheckReverseState(SetPos, new int2(1, -1), SetState, 0, ref Entities);

        //����
        CheckReverseState(SetPos, new int2(-1, 1), SetState, 0, ref Entities);

        //����
        CheckReverseState(SetPos, new int2(-1, -1), SetState, 0, ref Entities);
    }

    //�w�肵���O���b�h�̃f�[�^���擾���܂�
    public int GetGridData(int2 CheckPos, ref NativeArray<Entity> Entities)
    {
        if (CheckPos.x < BoardSize && CheckPos.y < BoardSize)
        {
            GridComp GridData = EntityManager.GetComponentData<GridComp>(Entities[CheckPos.x + (CheckPos.y * BoardSize)]);
            return GridData.GridState;
        }

        return -1;
    }

    //�w��x�N�g�������ɋ��߂Ă���̂��`�F�b�N����
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

    //����߂Ă��邩�`�F�b�N���āA���߂Ă����甽�]������
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
