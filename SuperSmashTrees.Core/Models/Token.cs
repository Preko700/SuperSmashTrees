namespace SuperSmashTrees.Core.Models
{
    public class Token
    {
        public int Value { get; private set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public bool IsCollected { get; private set; }

        public Token(int value, float positionX, float positionY)
        {
            Value = value;
            PositionX = positionX;
            PositionY = positionY;
            IsCollected = false;
        }

        public void Collect()
        {
            IsCollected = true;
        }
    }
}