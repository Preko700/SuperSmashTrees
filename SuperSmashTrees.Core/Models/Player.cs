using SuperSmashTrees.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSmashTrees.Core.Models
{
    public class Player
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public Position Position { get; set; }
        public Velocity Velocity { get; set; }
        public List<Power> Powers { get; set; }
        public Dictionary<string, object> Trees { get; set; }
        public bool IsJumping { get; set; }
        public bool IsFalling { get; set; }
        public int Health { get; set; }

        public Player(int id, string name)
        {
            Id = id;
            Name = name;
            Score = 0;
            Position = new Position(100, 100);  // Posición inicial por defecto
            Velocity = new Velocity(0, 0);
            Powers = new List<Power>();
            Trees = new Dictionary<string, object>();
            IsJumping = false;
            IsFalling = false;
            Health = 100;
        }

        public void Move(Direction direction, double speed)
        {
            switch (direction)
            {
                case Direction.Left:
                    Velocity.X = -speed;
                    break;
                case Direction.Right:
                    Velocity.X = speed;
                    break;
            }
        }

        public void Jump(double jumpForce)
        {
            if (!IsJumping && !IsFalling)
            {
                IsJumping = true;
                Velocity.Y = -jumpForce;
            }
        }

        public void Update(double deltaTime)
        {
            // Aplicar gravedad
            Velocity.Y += 9.8 * deltaTime;

            // Actualizar posición
            Position.X += Velocity.X * deltaTime;
            Position.Y += Velocity.Y * deltaTime;

            // Amortiguamiento horizontal
            Velocity.X *= 0.9;
        }

        public void UsePower(PowerType powerType, GameState gameState)
        {
            var power = Powers.FirstOrDefault(p => p.Type == powerType);
            if (power != null)
            {
                power.Use(this, gameState);
                Powers.Remove(power);
            }
        }

        public void AddToken(int token, string treeType)
        {
            // Agregar el token al árbol correspondiente
            switch (treeType)
            {
                case "BST":
                    if (!Trees.ContainsKey("BST"))
                    {
                        Trees["BST"] = new BinarySearchTree<int>();
                    }
                    ((BinarySearchTree<int>)Trees["BST"]).Insert(token);
                    break;
                case "AVL":
                    if (!Trees.ContainsKey("AVL"))
                    {
                        Trees["AVL"] = new AVLTree<int>();
                    }
                    ((AVLTree<int>)Trees["AVL"]).Insert(token);
                    break;
                case "BTree":
                    if (!Trees.ContainsKey("BTree"))
                    {
                        Trees["BTree"] = new BTree<int>(3); // Grado mínimo 3
                    }
                    ((BTree<int>)Trees["BTree"]).Insert(token);
                    break;
            }
        }
    }
}
