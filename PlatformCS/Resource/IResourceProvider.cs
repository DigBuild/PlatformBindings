using System.Collections.Generic;

namespace DigBuild.Platform.Resource
{
    public delegate IResource? GetResourceDelegate(ResourceName name);

    /// <summary>
    /// A resource provider.
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// Adds any modified resources to the set, and clears the local list of changed resources.
        /// </summary>
        /// <param name="resources">The set that the modified resources must be added to</param>
        void AddAndClearModifiedResources(ISet<ResourceName> resources);
        /// <summary>
        /// Tries to get a resource by name, optionally delegating on the parent if not found.
        /// </summary>
        /// <param name="name">The resource name</param>
        /// <param name="parent">The parent</param>
        /// <returns>The resource, or null</returns>
        IResource? GetResource(ResourceName name, GetResourceDelegate parent);
    }
}