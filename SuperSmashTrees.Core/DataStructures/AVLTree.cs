using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSmashTrees.Core.DataStructures
{
    public class AVLTree<T> where T : IComparable<T>
    {
        private class AVLNode
        {
            public T Value { get; set; }
            public AVLNode Left { get; set; }
            public AVLNode Right { get; set; }
            public int Height { get; set; }

            public AVLNode(T value)
            {
                Value = value;
                Left = null;
                Right = null;
                Height = 1; // Hoja nueva tiene altura 1
            }
        }

        private AVLNode Root { get; set; }

        public AVLTree()
        {
            Root = null;
        }

        private int Height(AVLNode node)
        {
            return node == null ? 0 : node.Height;
        }

        private int GetBalanceFactor(AVLNode node)
        {
            return node == null ? 0 : Height(node.Left) - Height(node.Right);
        }

        private AVLNode RightRotate(AVLNode y)
        {
            AVLNode x = y.Left;
            AVLNode T2 = x.Right;

            // Realizar rotación
            x.Right = y;
            y.Left = T2;

            // Actualizar alturas
            y.Height = Math.Max(Height(y.Left), Height(y.Right)) + 1;
            x.Height = Math.Max(Height(x.Left), Height(x.Right)) + 1;

            return x;
        }

        private AVLNode LeftRotate(AVLNode x)
        {
            AVLNode y = x.Right;
            AVLNode T2 = y.Left;

            // Realizar rotación
            y.Left = x;
            x.Right = T2;

            // Actualizar alturas
            x.Height = Math.Max(Height(x.Left), Height(x.Right)) + 1;
            y.Height = Math.Max(Height(y.Left), Height(y.Right)) + 1;

            return y;
        }

        public void Insert(T value)
        {
            Root = InsertRecursive(Root, value);
        }

        private AVLNode InsertRecursive(AVLNode node, T value)
        {
            // Inserción BST normal
            if (node == null)
            {
                return new AVLNode(value);
            }

            if (value.CompareTo(node.Value) < 0)
            {
                node.Left = InsertRecursive(node.Left, value);
            }
            else if (value.CompareTo(node.Value) > 0)
            {
                node.Right = InsertRecursive(node.Right, value);
            }
            else
            {
                // Valor duplicado, no se permite en AVL
                return node;
            }

            // Actualizar altura del nodo actual
            node.Height = Math.Max(Height(node.Left), Height(node.Right)) + 1;

            // Obtener factor de balance
            int balance = GetBalanceFactor(node);

            // Caso Left Left
            if (balance > 1 && value.CompareTo(node.Left.Value) < 0)
            {
                return RightRotate(node);
            }

            // Caso Right Right
            if (balance < -1 && value.CompareTo(node.Right.Value) > 0)
            {
                return LeftRotate(node);
            }

            // Caso Left Right
            if (balance > 1 && value.CompareTo(node.Left.Value) > 0)
            {
                node.Left = LeftRotate(node.Left);
                return RightRotate(node);
            }

            // Caso Right Left
            if (balance < -1 && value.CompareTo(node.Right.Value) < 0)
            {
                node.Right = RightRotate(node.Right);
                return LeftRotate(node);
            }

            return node;
        }

        public int GetHeight()
        {
            return Height(Root);
        }

        // Método para verificar si el árbol cumple cierto reto
        public bool MeetsChallengeRequirements(string challengeType)
        {
            switch (challengeType)
            {
                case "BalancedChallenge":
                    // Verificar que todas las ramas estén balanceadas
                    return IsFullyBalanced(Root);
                case "HeightChallenge":
                    int targetHeight = 4; // Ejemplo
                    return GetHeight() >= targetHeight;
                default:
                    return false;
            }
        }

        private bool IsFullyBalanced(AVLNode node)
        {
            if (node == null)
            {
                return true;
            }

            int balanceFactor = GetBalanceFactor(node);
            return Math.Abs(balanceFactor) <= 1 &&
                   IsFullyBalanced(node.Left) &&
                   IsFullyBalanced(node.Right);
        }
    }
}
