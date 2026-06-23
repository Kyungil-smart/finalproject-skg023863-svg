namespace MyGame
{
    public enum InputDefine
    {
        Noen = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Attack = 1 << 2
    }
    // 입력에 따른 캐릭터의 입력정보를 담음
    public struct InputData
    {
        public int Input;
        
        public float MoveX; // 좌우 입력
        public bool Attack; // 공격 입력
    }
}