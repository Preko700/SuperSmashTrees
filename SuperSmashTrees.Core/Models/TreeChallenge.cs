using System;
using SuperSmashTrees.Core.DataStructures;

namespace SuperSmashTrees.Core.Models
{
    public enum TreeType
    {
        BST,
        AVL,
        BTree
    }

    public enum ChallengeType
    {
        Height,
        NodeCount,
        Balance,
        MinDegree,
        SpecificStructure
    }

    public class TreeChallenge
    {
        public TreeType TreeType { get; private set; }
        public ChallengeType ChallengeType { get; private set; }
        public string Description { get; private set; }
        public int TargetValue { get; private set; }
        public string ChallengeId { get; private set; }

        public TreeChallenge(TreeType treeType, ChallengeType challengeType, int targetValue)
        {
            TreeType = treeType;
            ChallengeType = challengeType;
            TargetValue = targetValue;
            ChallengeId = GenerateChallengeId();
            Description = GenerateDescription();
        }

        private string GenerateChallengeId()
        {
            return $"{TreeType}_{ChallengeType}Challenge";
        }

        private string GenerateDescription()
        {
            switch (TreeType)
            {
                case TreeType.BST:
                    switch (ChallengeType)
                    {
                        case ChallengeType.Height:
                            return $"Construye un BST con altura de {TargetValue}";
                        case ChallengeType.NodeCount:
                            return $"Construye un BST con {TargetValue} nodos";
                        default:
                            return "Reto BST genérico";
                    }
                case TreeType.AVL:
                    switch (ChallengeType)
                    {
                        case ChallengeType.Balance:
                            return "Construye un árbol AVL perfectamente balanceado";
                        case ChallengeType.Height:
                            return $"Construye un AVL con altura de {TargetValue}";
                        default:
                            return "Reto AVL genérico";
                    }
                case TreeType.BTree:
                    switch (ChallengeType)
                    {
                        case ChallengeType.MinDegree:
                            return $"Construye un B-Tree con grado mínimo de {TargetValue}";
                        case ChallengeType.NodeCount:
                            return $"Construye un B-Tree con {TargetValue} nodos";
                        default:
                            return "Reto B-Tree genérico";
                    }
                default:
                    return "Reto desconocido";
            }
        }

        public bool CheckCompletion<T>(object tree) where T : IComparable<T>
        {
            switch (TreeType)
            {
                case TreeType.BST:
                    if (tree is BinarySearchTree<T> bst)
                    {
                        return bst.MeetsChallengeRequirements(ChallengeId);
                    }
                    break;
                case TreeType.AVL:
                    if (tree is AVLTree<T> avl)
                    {
                        return avl.MeetsChallengeRequirements(ChallengeId);
                    }
                    break;
                case TreeType.BTree:
                    if (tree is BTree<T> bTree)
                    {
                        return bTree.MeetsChallengeRequirements(ChallengeId);
                    }
                    break;
            }
            return false;
        }

        public static TreeChallenge GenerateRandomChallenge()
        {
            Random random = new Random();
            TreeType treeType = (TreeType)random.Next(Enum.GetValues(typeof(TreeType)).Length);
            ChallengeType challengeType;

            switch (treeType)
            {
                case TreeType.BST:
                    challengeType = random.Next(2) == 0 ? ChallengeType.Height : ChallengeType.NodeCount;
                    break;
                case TreeType.AVL:
                    challengeType = random.Next(2) == 0 ? ChallengeType.Balance : ChallengeType.Height;
                    break;
                case TreeType.BTree:
                    challengeType = random.Next(2) == 0 ? ChallengeType.MinDegree : ChallengeType.NodeCount;
                    break;
                default:
                    challengeType = ChallengeType.Height;
                    break;
            }

            int targetValue = challengeType switch
            {
                ChallengeType.Height => random.Next(3, 8),
                ChallengeType.NodeCount => random.Next(5, 20),
                ChallengeType.MinDegree => random.Next(2, 5),
                _ => 0
            };

            return new TreeChallenge(treeType, challengeType, targetValue);
        }
    }
}