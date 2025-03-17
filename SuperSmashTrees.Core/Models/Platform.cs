using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSmashTrees.Core.Models
{
    public class Platform
    {
        public Position Position { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public Platform(double x, double y, double width, double height)
        {
            Position = new Position(x, y);
            Width = width;
            Height = height;
        }

        public bool Intersects(Player player)
        {
            // Colisión simple rectangular (mejorar si es necesario)
            return player.Position.X + 20 > Position.X &&
                   player.Position.X - 20 < Position.X + Width &&
                   player.Position.Y + 30 > Position.Y &&
                   player.Position.Y - 30 < Position.Y + Height;
        }
    }
}
