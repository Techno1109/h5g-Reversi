using Unity.Entities;

public struct TestButton : IComponentData
{
    //テスト用のボタンタグです。
    //テスト段階ではこのタグのButtonNameでボタンを判断させます
    public string ButtonName;
}
