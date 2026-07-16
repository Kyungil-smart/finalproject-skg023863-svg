namespace MyGame
{
    public enum InputDefine
    {
        Noen = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Down = 1 << 2,
        Up = 1 << 3,
        Attack = 1 << 4
    }
    // 입력에 따른 캐릭터의 입력정보를 담음
    public struct InputData
    {
        public int Input;
    }
}