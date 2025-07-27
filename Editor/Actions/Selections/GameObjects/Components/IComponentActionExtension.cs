using UnityEngine;
using UnityEditor;

namespace Yueby.QuickActions.Actions.Selections
{
    /// <summary>
    /// Interface for extending ComponentAction with custom component-specific actions
    /// </summary>
    public interface IComponentActionExtension
    {
        /// <summary>
        /// Get the component type this extension handles
        /// </summary>
        System.Type ComponentType { get; }
        
        /// <summary>
        /// Register custom actions for the specified component
        /// </summary>
        /// <param name="component">The component instance</param>
        /// <param name="componentName">The component type name</param>
        /// <param name="componentKey">The unique key for this component</param>
        void RegisterCustomActions(Component component, string componentName, string componentKey);
        
        /// <summary>
        /// Get the priority for this extension (lower values appear first)
        /// </summary>
        int Priority { get; }
    }
} 