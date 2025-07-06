using System;

namespace Yueby.QuickActions
{
    /// <summary>
    /// Quick action attribute, used to mark and configure quick actions (MenuItem style)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class QuickActionAttribute : Attribute
    {
        /// <summary>
        /// Action path (similar to Unity's MenuItem path)
        /// For example: "Tools/My Action" or "GameObject/Create Empty"
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Action description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Priority, lower numbers have higher priority
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Validation function name, used to check if enabled
        /// Validation function must be a static method returning bool
        /// </summary>
        public string ValidateFunction { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Action path</param>
        /// <param name="description">Action description</param>
        public QuickActionAttribute(string path, string description = null)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Description = description ?? GetActionName(path);
        }

        /// <summary>
        /// Extract action name from path
        /// </summary>
        private static string GetActionName(string path)
        {
            var parts = path.Split('/');
            return parts[parts.Length - 1];
        }
    }
}