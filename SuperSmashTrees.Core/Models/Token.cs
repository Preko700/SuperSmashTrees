using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSmashTrees.Core.Models
{
    public class Token
    {
        public int Value { get; set; }
        public Position Position { get; set; }
        public bool IsCollected { get; set; }
        public string TreeType { get; set; }  // "BST", "AVL", "BTree"

        public Token(int value, double x, double y, string treeType)
        {
            Value = value;
            Position = new Position(x, y);
            IsCollected = false;
            TreeType = treeType;
        }

        public bool Intersects(Player player)
        {
            // Distancia entre el centro del token y el jugador
            double dx = player.Position.X - Position.X;
            double dy = player.Position.Y - Position.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            return distance < 30; // Radio de colisión
        }
    }
}
