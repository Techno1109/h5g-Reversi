using Unity.Entities;

public class GameState : IComponentData
{
    //�Q�[�����i�s���Ȃ̂��A�I�����Ă���̂��i�[���܂�
    public bool IsActive;

    //  �ǂ���̃^�[���Ȃ̂����i�[���܂�
    public int NowTurn;
}
