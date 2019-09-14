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
        /*ECS�ɂ����āA�N�G���̍쐬��OnCreate�ōs���̂���΂ƂȂ��Ă��܂�*/

        GirdEntityDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer),typeof(PointerInteraction) ,typeof(Button)},
        };

        /*GetEntityQuery�Ŏ擾�������ʂ͎����I�ɊJ������邽�߁AFree���s�������������Ȃ��Ă����ł��B*/
        //�쐬�����N�G���̌��ʂ��擾���܂��B
        GridEntity = GetEntityQuery(GirdEntityDesc);
    }

    protected override void OnUpdate()
    {
        Entities.With(GridEntity).ForEach((Entity EntityData, ref PointerInteraction GridClickData,ref GridComp GridData) =>
        {
            if (GridClickData.clicked == true)
            {
                //�N���b�N���ꂽ�Ƃ����GridComp��Num�����W�ł��B
            }
        });
    }

    //Entity���e���W�ɑΉ����������ԂɊi�[���Ȃ����܂�
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


    //�����Ă������W�Ƀf�[�^�����������܂��B
    //�ǂ̂��炢���̊֐��ŋ@�\���������邩�킩��܂��񂪁A
    //�����܂ł��������݂����ŁA��̔��]�����͕ʊ֐��ōs����������܂���B
    public void SetGridData(int2 SetPos,int SetStatus,ref NativeArray<Entity> Entities)
    {
        if (SetPos.x < BoardSize && SetPos.y < BoardSize)
        {
            //Entities[SetPos.x + (SetPos.y * BoardSize)];
        }
    }
}
