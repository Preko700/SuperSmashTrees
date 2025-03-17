using System;
using System.Collections.Generic;

namespace SuperSmashTrees.Core.DataStructures
{
    public class BTree<T> where T : IComparable<T>
    {
        private class BTreeNode
        {
            public List<T> Keys { get; set; }
            public List<BTreeNode> Children { get; set; }
            public bool IsLeaf { get; set; }
            public int MinDegree { get; set; }

            public BTreeNode(int minDegree, bool isLeaf)
            {
                this.MinDegree = minDegree;
                this.IsLeaf = isLeaf;
                this.Keys = new List<T>(2 * minDegree - 1);
                this.Children = new List<BTreeNode>(2 * minDegree);
            }

            // Devuelve true si este nodo está lleno
            public bool IsFull()
            {
                return Keys.Count == 2 * MinDegree - 1;
            }
        }

        private BTreeNode root;
        private readonly int minDegree;
        private int nodeCount;

        public BTree(int minDegree)
        {
            this.root = null;
            this.minDegree = minDegree;
            this.nodeCount = 0;
        }

        // Método público para insertar una clave en el árbol
        public void Insert(T key)
        {
            // Si el árbol está vacío
            if (root == null)
            {
                root = new BTreeNode(minDegree, true);
                root.Keys.Add(key);
                nodeCount++;
                return;
            }

            // Si la raíz está llena, el árbol crece en altura
            if (root.IsFull())
            {
                // Crear nueva raíz
                BTreeNode newRoot = new BTreeNode(minDegree, false);

                // Hacer que la raíz antigua sea hija de la nueva raíz
                newRoot.Children.Add(root);

                // Dividir la antigua raíz y mover una clave a la nueva raíz
                SplitChild(newRoot, 0);

                // La nueva raíz tiene dos hijos. Decidir cuál de los dos hijos va a tener la nueva clave
                int i = 0;
                if (newRoot.Keys[0].CompareTo(key) < 0)
                {
                    i++;
                }
                InsertNonFull(newRoot.Children[i], key);

                // Cambiar la raíz
                root = newRoot;
            }
            else // Si la raíz no está llena, llamar a insertNonFull para la raíz
            {
                InsertNonFull(root, key);
            }
        }

        // Función para insertar una clave en un nodo que no está lleno
        private void InsertNonFull(BTreeNode node, T key)
        {
            // Inicializar índice como el índice del elemento más a la derecha
            int i = node.Keys.Count - 1;

            // Si es una hoja
            if (node.IsLeaf)
            {
                // Buscar la posición correcta para la nueva clave
                while (i >= 0 && node.Keys[i].CompareTo(key) > 0)
                {
                    i--;
                }

                // Insertar la nueva clave en la posición encontrada
                node.Keys.Insert(i + 1, key);
                nodeCount++;
            }
            else // Si no es hoja
            {
                // Encontrar el hijo que va a tener la nueva clave
                while (i >= 0 && node.Keys[i].CompareTo(key) > 0)
                {
                    i--;
                }

                // Ver si el hijo encontrado está lleno
                if (node.Children[i + 1].IsFull())
                {
                    // Si el hijo está lleno, dividirlo
                    SplitChild(node, i + 1);

                    // Después de la división, la clave media del hijo[i] sube
                    // El hijo[i] se divide en dos. Ver cuál de los dos va a tener la clave
                    if (node.Keys[i + 1].CompareTo(key) < 0)
                    {
                        i++;
                    }
                }
                InsertNonFull(node.Children[i + 1], key);
            }
        }

        // Función para dividir el hijo de un nodo
        private void SplitChild(BTreeNode parent, int childIndex)
        {
            // Obtener el hijo a dividir
            BTreeNode child = parent.Children[childIndex];

            // Crear un nuevo nodo que será el hermano derecho
            BTreeNode newChild = new BTreeNode(child.MinDegree, child.IsLeaf);

            // Copiar las últimas (MinDegree-1) claves del hijo al nuevo hijo
            for (int j = 0; j < minDegree - 1; j++)
            {
                newChild.Keys.Add(child.Keys[j + minDegree]);
            }

            // Copiar los últimos MinDegree hijos del hijo al nuevo hijo
            if (!child.IsLeaf)
            {
                for (int j = 0; j < minDegree; j++)
                {
                    newChild.Children.Add(child.Children[j + minDegree]);
                }
            }

            // Reducir el número de claves en child
            for (int j = 0; j < minDegree; j++)
            {
                if (child.Keys.Count > minDegree)
                    child.Keys.RemoveAt(child.Keys.Count - 1);
            }

            // Reducir el número de hijos en child
            if (!child.IsLeaf)
            {
                for (int j = 0; j < minDegree; j++)
                {
                    if (child.Children.Count > minDegree)
                        child.Children.RemoveAt(child.Children.Count - 1);
                }
            }

            // Insertar un nuevo hijo en parent
            parent.Children.Insert(childIndex + 1, newChild);

            // Una clave de child se mueve a parent
            parent.Keys.Insert(childIndex, child.Keys[minDegree - 1]);
            child.Keys.RemoveAt(minDegree - 1);
        }

        // Método para verificar si el árbol cumple cierto reto
        public bool MeetsChallengeRequirements(string challengeType)
        {
            switch (challengeType)
            {
                case "MinDegreeChallenge":
                    return minDegree >= 3;
                case "NodeCountChallenge":
                    int targetCount = 10; // Ejemplo
                    return nodeCount >= targetCount;
                default:
                    return false;
            }
        }

        // Método para obtener la altura del árbol
        public int GetHeight()
        {
            return CalculateHeight(root);
        }

        private int CalculateHeight(BTreeNode node)
        {
            if (node == null)
                return 0;

            if (node.IsLeaf)
                return 1;

            int maxHeight = 0;
            foreach (var child in node.Children)
            {
                int height = CalculateHeight(child);
                if (height > maxHeight)
                    maxHeight = height;
            }

            return maxHeight + 1;
        }
    }
}