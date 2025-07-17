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
        Action
    }

    /// <summary>
    /// Action tree node
    /// </summary>
    public class ActionNode
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ActionNodeType Type { get; set; }
        public QuickAction.ActionInfo ActionInfo { get; set; }
        public List<ActionNode> Children { get; set; } = new List<ActionNode>();
        public ActionNode Parent { get; set; }
        public Texture2D Icon { get; set; }
        public int Priority { get; set; }
        public int CurrentPageIndex { get; set; } = 0; // Page index state for this node
        public bool IsChecked { get; set; } = false; // 选中状态
        public bool ShowCheckmark { get; set; } = false; // 是否显示checkmark

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
            var allActions = QuickAction.GetEnabledActions();

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

            // Calculate start index for current page
            var startIndex = pageIndex * MaxButtonsPerPage;

            // Return empty list if start index is out of range
            if (startIndex >= totalActions)
            {
                return buttons;
            }

            // Get current page actions and update their checked status
            var actionsToShow = Mathf.Min(MaxButtonsPerPage, totalActions - startIndex);
            var availableActions = _currentNode.Children.Skip(startIndex).Take(actionsToShow).ToList();

            // 更新每个动作的选中状态
            foreach (var action in availableActions)
            {
                if (action.Type == ActionNodeType.Action && action.ActionInfo != null)
                {
                    var state = QuickAction.GetActionState(action.ActionInfo.Path);
                    action.IsChecked = state.IsChecked;
                    action.ShowCheckmark = state.ShowCheckmark;
                }
            }

            buttons.AddRange(availableActions);

            return buttons;
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

            var result = QuickAction.ExecuteAction(node.ActionInfo.Path);

            // Refresh all validation states after executing action
            if (result)
            {
                QuickAction.RefreshValidationStates();
            }

            return result;
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
            var nextPageStartIndex = (currentPageIndex + 1) * MaxButtonsPerPage;

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

        /// <summary>
        /// Check if can navigate back (to parent or previous page)
        /// </summary>
        public bool CanNavigateBack()
        {
            EnsureTreeBuilt();

            // Can navigate back if not on first page
            if (_currentNode.CurrentPageIndex > 0)
            {
                return true;
            }

            // Can navigate back if not at root node
            if (_currentNode.Parent != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if can go to next page
        /// </summary>
        public bool CanNextPage()
        {
            EnsureTreeBuilt();
            var totalActions = _currentNode.Children.Count;
            if (totalActions == 0) return false;

            var currentPageIndex = _currentNode.CurrentPageIndex;
            var nextPageStartIndex = (currentPageIndex + 1) * MaxButtonsPerPage;

            // Check if there's a next page
            return nextPageStartIndex < totalActions;
        }

        /// <summary>
        /// Check if current page has next page button
        /// </summary>
        public bool HasNextPage
        {
            get
            {
                EnsureTreeBuilt();
                var totalActions = _currentNode.Children.Count;
                if (totalActions == 0) return false;

                var currentPageIndex = _currentNode.CurrentPageIndex;
                var currentPageStartIndex = currentPageIndex * MaxButtonsPerPage;
                var remainingActions = totalActions - currentPageStartIndex;

                // Check if current page has more actions than can fit
                return remainingActions > MaxButtonsPerPage;
            }
        }

        /// <summary>
        /// Get navigation info for current state
        /// </summary>
        public (bool canNavigateBack, bool hasNextPage) GetNavigationInfo()
        {
            return (CanNavigateBack(), HasNextPage);
        }
    }
}