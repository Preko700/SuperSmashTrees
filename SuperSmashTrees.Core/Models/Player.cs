using System;
using System.Collections.Generic;
using SuperSmashTrees.Core.DataStructures;

namespace SuperSmashTrees.Core.Models
{
    public class Player
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public int Score { get; private set; }
        public List<Power> AvailablePowers { get; private set; }
        public bool IsShielded { get; set; }
        public bool CanAirJump { get; set; }
        public bool IsFalling { get; set; }

        // Árboles del jugador (uno para cada tipo)
        public BinarySearchTree<int> BSTree { get; private set; }
        public AVLTree<int> AVLTree { get; private set; }
        public BTree<int> BTree { get; private set; }

        // Referencia al árbol actualmente en uso según el reto
        public object CurrentTree { get; private set; }
        public TreeType CurrentTreeType { get; private set; }

        public Player(int id, string name)
        {
            Id = id;
            Name = name;
            Score = 0;
            AvailablePowers = new List<Power>();
            IsShielded = false;
            CanAirJump = false;
            IsFalling = false;

            // Inicializar árboles
            BSTree = new BinarySearchTree<int>();
            AVLTree = new AVLTree<int>();
            BTree = new BTree<int>(2); // Grado mínimo de 2

            // Por defecto, empezamos con BST
            CurrentTree = BSTree;
            CurrentTreeType = TreeType.BST;
        }

        public void AddScore(int points)
        {
            Score += points;
        }

        public void AddPower(Power power)
        {
            AvailablePowers.Add(power);
        }

        public Power GetPower(PowerType type)
        {
            return AvailablePowers.Find(p => p.Type == type && !p.IsActive);
        }

        public void RemovePower(Power power)
        {
            AvailablePowers.Remove(power);
        }

        public void SetCurrentTreeType(TreeType treeType)
        {
            CurrentTreeType = treeType;
            switch (treeType)
            {
                case TreeType.BST:
                    CurrentTree = BSTree;
                    break;
                case TreeType.AVL:
                    CurrentTree = AVLTree;
                    break;
                case TreeType.BTree:
                    CurrentTree = BTree;
                    break;
            }
        }

        public void AddToken(int value)
        {
            switch (CurrentTreeType)
            {
                case TreeType.BST:
                    BSTree.Insert(value);
                    break;
                case TreeType.AVL:
                    AVLTree.Insert(value);
                    break;
                case TreeType.BTree:
                    BTree.Insert(value);
                    break;
            }
        }

        public void ApplyForceFrom(float sourceX, float sourceY, float force)
        {
            if (IsShielded)
                return;

            // Vector de dirección desde la fuente al jugador
            float dirX = PositionX - sourceX;
            float dirY = PositionY - sourceY;

            // Normalizar
            float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
            if (length > 0)
            {
                dirX /= length;
                dirY /= length;
            }

            // Aplicar fuerza
            PositionX += dirX * force;
            PositionY += dirY * force;
        }
    }
}