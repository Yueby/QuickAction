using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yueby.QuickActions
{
    /// <summary>
    /// Action tree node type
    /// </summary>
    public enum ActionNodeType
    {
        /// <summary>
        /// Collection node (folder)
        /// </summary>
        Collection,

        /// <summary>
        /// Action node (executable command)
        /// </summary>
        Action,

        /// <summary>
        /// Back to parent node
        /// </summary>
        Back,

        /// <summary>
        /// Next page node
        /// </summary>
        NextPage
    }

    /// <summary>
    /// Action tree node
    /// </summary>
    public class ActionNode
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ActionNodeType Type { get; set; }
        public ActionRegistry.ActionInfo ActionInfo { get; set; }
        public List<ActionNode> Children { get; set; } = new List<ActionNode>();
        public ActionNode Parent { get; set; }
        public Texture2D Icon { get; set; }
        public int Priority { get; set; }
        public int CurrentPageIndex { get; set; } = 0; // Page index state for this node

        public bool IsLeaf => Children.Count == 0;
        public bool IsRoot => Parent == null;
    }

    /// <summary>
    /// Action tree, handles collection and pagination logic
    /// </summary>
    public class ActionTree
    {
        private ActionNode _root;
        private ActionNode _currentNode;
        private const int MaxButtonsPerPage = 8; // Maximum 8 buttons per page

        /// <summary>
        /// Current node
        /// </summary>
        public ActionNode CurrentNode => _currentNode;

        /// <summary>
        /// Whether at root node
        /// </summary>
        public bool IsAtRoot
        {
            get
            {
                EnsureTreeBuilt();
                return _currentNode == _root;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionTree()
        {
            // Don't build tree immediately in constructor, build when needed
        }

        /// <summary>
        /// Ensure tree is built
        /// </summary>
        private void EnsureTreeBuilt()
        {
            if (_root == null)
            {
                BuildTree();
            }
        }

        /// <summary>
        /// Build action tree
        /// </summary>
        public void BuildTree()
        {
            _root = new ActionNode
            {
                Name = "Root",
                Path = "",
                Type = ActionNodeType.Collection
            };

            // Only get enabled actions
            var allActions = ActionRegistry.GetEnabledActions();

            // Build tree structure by path
            foreach (var kvp in allActions)
            {
                var actionInfo = kvp.Value;
                var pathParts = actionInfo.Path.Split('/');

                var currentNode = _root;
                var currentPath = "";

                // Traverse each part of the path
                for (int i = 0; i < pathParts.Length; i++)
                {
                    var part = pathParts[i];
                    currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                    // Check if node already exists
                    var existingNode = currentNode.Children.FirstOrDefault(n => n.Name == part);

                    if (existingNode == null)
                    {
                        // Create new node
                        var newNode = new ActionNode
                        {
                            Name = part,
                            Path = currentPath,
                            Parent = currentNode,
                            Priority = actionInfo.Priority
                        };

                        // Determine if this is a leaf node (last path part)
                        if (i == pathParts.Length - 1)
                        {
                            // This is an action node
                            newNode.Type = ActionNodeType.Action;
                            newNode.ActionInfo = actionInfo;
                        }
                        else
                        {
                            // This is a collection node
                            newNode.Type = ActionNodeType.Collection;
                        }

                        currentNode.Children.Add(newNode);
                        currentNode = newNode;
                    }
                    else
                    {
                        currentNode = existingNode;
                    }
                }
            }

            // Sort all nodes' children by priority
            SortChildren(_root);

            _currentNode = _root;
        }

        /// <summary>
        /// Recursively sort children
        /// </summary>
        private void SortChildren(ActionNode node)
        {
            node.Children = node.Children.OrderBy(n => n.Priority).ThenBy(n => n.Name).ToList();

            foreach (var child in node.Children)
            {
                SortChildren(child);
            }
        }

        /// <summary>
        /// Get current page button list
        /// </summary>
        /// <param name="pageIndex">Page index (starting from 0, use node's own page index if -1)</param>
        /// <returns>Button list</returns>
        public List<ActionNode> GetCurrentPageButtons(int pageIndex = -1)
        {
            // Ensure tree is built
            EnsureTreeBuilt();

            var buttons = new List<ActionNode>();
            var totalActions = _currentNode.Children.Count;

            // Return empty list if no actions
            if (totalActions == 0)
            {
                return buttons;
            }

            // Use node's own page index if pageIndex is -1
            if (pageIndex == -1)
            {
                pageIndex = _currentNode.CurrentPageIndex;
            }

            // Dynamically calculate current page button distribution
            bool needBackButton = !IsAtRoot || pageIndex > 0;
            int availableSlots = MaxButtonsPerPage;
            
            // Reserve space for back button
            if (needBackButton)
            {
                availableSlots--;
            }

            // Calculate action start index and remaining count
            var startIndex = CalculateStartIndex(pageIndex, needBackButton);
            
            // Return empty list if start index is out of range
            if (startIndex >= totalActions)
            {
                return buttons;
            }

            var remainingActions = totalActions - startIndex;
            
            // Check if next page button is needed
            bool needNextPageButton = remainingActions > availableSlots;
            
            // Reduce one more slot if next page button is needed
            if (needNextPageButton)
            {
                availableSlots--;
            }

            // Add back button
            if (needBackButton)
            {
                buttons.Add(new ActionNode
                {
                    Name = "↑",
                    Type = ActionNodeType.Back,
                    Priority = -1000 // Ensure back button is at the front
                });
            }

            // Get current page actions
            var actionsToShow = Mathf.Min(availableSlots, remainingActions);
            var availableActions = _currentNode.Children.Skip(startIndex).Take(actionsToShow).ToList();

            buttons.AddRange(availableActions);

            // Add next page button
            if (needNextPageButton)
            {
                buttons.Add(new ActionNode
                {
                    Name = "←",
                    Type = ActionNodeType.NextPage,
                    Priority = 1000 // Ensure next page button is at the end
                });
            }

            return buttons;
        }

        /// <summary>
        /// Calculate start index for specified page
        /// </summary>
        private int CalculateStartIndex(int targetPageIndex, bool needBackButton)
        {
            if (targetPageIndex == 0)
            {
                return 0;
            }

            int startIndex = 0;
            
            // Calculate page by page, considering dynamic slot allocation for each page
            for (int page = 0; page < targetPageIndex; page++)
            {
                int availableSlots = MaxButtonsPerPage;
                
                // Reserve space for back button
                if (needBackButton)
                {
                    availableSlots--;
                }
                
                // Calculate remaining actions for this page
                var remainingActions = _currentNode.Children.Count - startIndex;
                
                // Check if next page button is needed
                bool needNextPageButton = remainingActions > availableSlots;
                
                // Reduce one more slot if next page button is needed
                if (needNextPageButton)
                {
                    availableSlots--;
                }
                
                // Actual number of actions displayed on this page
                var actionsInThisPage = Mathf.Min(availableSlots, remainingActions);
                startIndex += actionsInThisPage;
            }
            
            return startIndex;
        }

        /// <summary>
        /// Navigate to specified node
        /// </summary>
        public bool NavigateTo(ActionNode node)
        {
            EnsureTreeBuilt();
            if (node == null) return false;

            switch (node.Type)
            {
                case ActionNodeType.Collection:
                    _currentNode = node;
                    return true;

                case ActionNodeType.Action:
                    // ActionTree only handles navigation, not execution
                    // Execution logic should be handled by caller
                    return true;

                case ActionNodeType.Back:
                    return NavigateBack();

                case ActionNodeType.NextPage:
                    // Next page logic is handled by UI
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Execute specified node's action
        /// </summary>
        public bool ExecuteAction(ActionNode node)
        {
            if (node?.Type != ActionNodeType.Action || node.ActionInfo == null)
                return false;

            return ActionRegistry.ExecuteAction(node.ActionInfo.Path);
        }

        /// <summary>
        /// Navigate back to parent or first page
        /// </summary>
        public bool NavigateBack()
        {
            EnsureTreeBuilt();

            // If not on first page, return to first page
            if (_currentNode.CurrentPageIndex > 0)
            {
                _currentNode.CurrentPageIndex = 0;
                return true;
            }

            // If already on first page, return to parent node
            if (_currentNode.Parent != null)
            {
                _currentNode = _currentNode.Parent;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Navigate to root node
        /// </summary>
        public void NavigateToRoot()
        {
            _currentNode = _root;
        }

        /// <summary>
        /// Get current path
        /// </summary>
        public string GetCurrentPath()
        {
            if (_currentNode == _root) return "Root";
            return _currentNode.Path;
        }

        /// <summary>
        /// Refresh action tree (re-get enabled status)
        /// </summary>
        public void Refresh()
        {
            // In lazy build mode, rebuild on next access
            _root = null;
            _currentNode = null;
        }

        /// <summary>
        /// Rebuild action tree
        /// </summary>
        public void RebuildTree()
        {
            Refresh();
        }

        /// <summary>
        /// Get current node's page index
        /// </summary>
        public int GetCurrentPageIndex()
        {
            EnsureTreeBuilt();
            return _currentNode.CurrentPageIndex;
        }

        /// <summary>
        /// Set current node's page index
        /// </summary>
        public void SetCurrentPageIndex(int pageIndex)
        {
            EnsureTreeBuilt();
            _currentNode.CurrentPageIndex = Mathf.Max(0, pageIndex);
        }

        /// <summary>
        /// Go to next page
        /// </summary>
        public bool NextPage()
        {
            EnsureTreeBuilt();
            var totalActions = _currentNode.Children.Count;
            if (totalActions == 0) return false;

            var currentPageIndex = _currentNode.CurrentPageIndex;
            bool needBackButton = !IsAtRoot || currentPageIndex > 0;
            
            // Calculate start index for next page
            var nextPageStartIndex = CalculateStartIndex(currentPageIndex + 1, needBackButton);
            
            // Check if there's a next page
            if (nextPageStartIndex < totalActions)
            {
                _currentNode.CurrentPageIndex++;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Go to previous page
        /// </summary>
        public bool PreviousPage()
        {
            EnsureTreeBuilt();
            if (_currentNode.CurrentPageIndex > 0)
            {
                _currentNode.CurrentPageIndex--;
                return true;
            }
            return false;
        }
    }
}