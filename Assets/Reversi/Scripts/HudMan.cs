using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Tiny.UILayout;
using Unity.Tiny.UIControls;
using Unity.Mathematics;

[UpdateAfter(typeof(BoardMan))]
public class HudMan : ComponentSystem
{
    EntityQueryDesc GameEndWindowDesc;
    EntityQuery     GameEndWindowEntity;

    EntityQueryDesc WinnerTextDesc;
    EntityQuery WinnerTextEntity;

    EntityQueryDesc NowTurnTextDesc;
    EntityQuery NowTurnTextEntity;

    EntityQueryDesc NowTurnBgDesc;
    EntityQuery NowTurnBgEntity;

    EntityQueryDesc ReplayButtonDesc;
    EntityQuery ReplayButtonEntity;


    protected override void OnCreate()
    {
        /*ECS�ɂ����āA�N�G���̍쐬��OnCreate�ōs���̂���΂ƂȂ��Ă��܂�*/

        GameEndWindowDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(GameEndWindowTag)},
        };

        WinnerTextDesc = new EntityQueryDesc()
        {
            All= new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer),typeof(WinnerTexts), typeof(WinnerTextTag) },
        };
    

        NowTurnTextDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer), typeof(NowTurnTag) ,typeof(TurnTexts)},
        };

        NowTurnBgDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer), typeof(NowTurnBg) },
        };


       ReplayButtonDesc = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(RectTransform), typeof(Sprite2DRenderer), typeof(PointerInteraction), typeof(Button), typeof(ReplayButton) },
        };

        /*GetEntityQuery�Ŏ擾�������ʂ͎����I�ɊJ������邽�߁AFree���s�������������Ȃ��Ă����ł��B*/
        //�쐬�����N�G���̌��ʂ��擾���܂��B
        GameEndWindowEntity = GetEntityQuery(GameEndWindowDesc);
        WinnerTextEntity = GetEntityQuery(WinnerTextDesc);
        NowTurnTextEntity = GetEntityQuery(NowTurnTextDesc);
        NowTurnBgEntity = GetEntityQuery(NowTurnBgDesc);
        ReplayButtonEntity = GetEntityQuery(ReplayButtonDesc);
    }

    protected override void OnUpdate()
    {
        if (HasSingleton<GameState>() == false)
        {
            return;
        }

        if (HasSingleton<BoardState>() == false)
        {
            return;
        }
        var G_State = GetSingleton<GameState>();
        var B_State = GetSingleton<BoardState>();

        //�o�b�N�O���E���h�̐F�ƕ�����ύX
        Entities.With(NowTurnBgEntity).ForEach((ref Sprite2DRenderer Sprite2D) =>
        {
            Color White = new Color(0.9705882f, 0.9705882f, 0.9705882f);
            Color Black = new Color(0.1960784f, 0.1960784f, 0.1960784f);

            Sprite2D.color = G_State.NowTurn == 1 ? Black : White;
        });

        Entities.With(NowTurnTextEntity).ForEach((ref Sprite2DRenderer Sprite2D,ref TurnTexts Texts) =>
        {
            Sprite2D.sprite = G_State.NowTurn == 1 ? Texts.Black : Texts.White;
        });

        if (G_State.GameEnd == true)
        {
            bool PushFlag = false;

            Entities.With(ReplayButtonEntity).ForEach((ref PointerInteraction GridClickData) =>
            {
                PushFlag = GridClickData.clicked;
                EntityManager.World.GetExistingSystem<IniBoard>().InitBoard();
            });

            Entities.With(GameEndWindowEntity).ForEach((ref RectTransform RectT) =>
            {
                //�`��ʒu
                if (PushFlag==false)
                {
                    RectT.anchoredPosition = new float2(0,0);
                }
                else
                {
                    RectT.anchoredPosition = new float2(0,1000);
                }

            });

            Entities.With(WinnerTextEntity).ForEach((ref Sprite2DRenderer Sprite2D, ref WinnerTexts WinnetText) =>
            {
                Sprite2D.sprite = G_State.WinnetNum == 1 ? WinnetText.Black : WinnetText.White;
            });
        }
    }
}
