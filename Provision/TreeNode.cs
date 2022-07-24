using System;
using System.Collections.Generic;
using System.Linq;

namespace Provision
{
    public class TreeNode<TData>
    {
        public TData Data { get; set; }
        public List<TreeNode<TData>> Children { get; private set; } = new();

        /// <summary>
        /// Gets list of ancestor nodes leading to the first child meeting the specified condition.
        /// </summary>
        /// <param name="predicate">Condition to be met by searched child</param>
        /// <returns>List of ancestor in order or empty list if specified child was not found.</returns>
        public List<TreeNode<TData>> GetPathToChild(Predicate<TreeNode<TData>> predicate)
        {
            static bool GetPathToChild(TreeNode<TData> node, Predicate<TreeNode<TData>> predicate, List<TreeNode<TData>> path, int index = 0)
            {
                TreeNode<TData> result = node.Children.FirstOrDefault(child => predicate(child));

                if (result is null)
                {
                    foreach (TreeNode<TData> child in node.Children)
                    {
                        if (GetPathToChild(child, predicate, path, index + 1))
                        {
                            path.Insert(index, node);
                            return true;
                        }
                    }
                }
                else
                {
                    path.Add(node);
                    return true;
                }

                return false;
            }

            List<TreeNode<TData>> path = new();

            GetPathToChild(this, predicate, path);
            
            return path;
        }

        /// <summary>
        /// Get list of tuples containing relative depth of the current node and it's children in relation to current node and nodes themselves.
        /// </summary>
        /// <returns>List of tuples of depth and nodes</returns>
        public List<(int, TreeNode<TData>)> GetNodesDepth()
        {
            static void GetDepth(TreeNode<TData> node, List<(int, TreeNode<TData>)> collection, int depth = 0)
            {
                collection.Add((depth, node));
                node.Children.ForEach(child => GetDepth(child, collection, depth + 1));
            }

            List<(int, TreeNode<TData>)> result = new();

            GetDepth(this, result);

            return result;
        }

        /// <summary>
        /// Flattens the tree to a list, starting form current node.
        /// </summary>
        /// <returns>List containing current node and all the nodes below it.</returns>
        public List<TreeNode<TData>> Flatten()
        {
            static void GetChildren(TreeNode<TData> node, List<TreeNode<TData>> collection)
            {
                collection.AddRange(node.Children);
                node.Children.ForEach(child => GetChildren(child, collection));
            }

            List<TreeNode<TData>> result = new() { this };

            GetChildren(this, result);

            return result;
        }
    }
}
