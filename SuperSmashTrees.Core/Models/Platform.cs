namespace SuperSmashTrees.Core.Models
{
    public class Platform
    {
        public float PositionX { get; private set; }
        public float PositionY { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }

        public Platform(float posX, float posY, float width, float height)
        {
            PositionX = posX;
            PositionY = posY;
            Width = width;
            Height = height;
        }
    }
}