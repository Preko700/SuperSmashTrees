using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSmashTrees.Core.DataStructures
{
    public class BinarySearchTree<T> where T : IComparable<T>
    {
        public TreeNode<T> Root { get; private set; }

        public BinarySearchTree()
        {
            Root = null;
        }

        public void Insert(T value)
        {
            Root = InsertRecursive(Root, value);
        }

        private TreeNode<T> InsertRecursive(TreeNode<T> node, T value)
        {
            // Si el nodo es nulo, crear un nuevo nodo
            if (node == null)
            {
                return new TreeNode<T>(value);
            }

            // Si el valor es menor, insertar a la izquierda
            if (value.CompareTo(node.Value) < 0)
            {
                node.Left = InsertRecursive(node.Left, value);
            }
            // Si el valor es mayor o igual, insertar a la derecha
            else
            {
                node.Right = InsertRecursive(node.Right, value);
            }

            return node;
        }

        public int GetHeight()
        {
            return CalculateHeight(Root);
        }

        private int CalculateHeight(TreeNode<T> node)
        {
            if (node == null)
            {
                return 0;
            }

            int leftHeight = CalculateHeight(node.Left);
            int rightHeight = CalculateHeight(node.Right);

            return Math.Max(leftHeight, rightHeight) + 1;
        }

        // Método para verificar si el árbol cumple cierto reto
        public bool MeetsChallengeRequirements(string challengeType)
        {
            switch (challengeType)
            {
                case "HeightChallenge":
                    int targetHeight = 6; // Ejemplo para "construir un BST de profundidad 6"
                    return GetHeight() >= targetHeight;
                case "NodeCountChallenge":
                    // Implementar lógica para contar nodos
                    return true;
                default:
                    return false;
            }
        }
    }
}
