using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DigBuild.Platform.Resource
{
    /// <summary>
    /// A resource manager composed of an ordered set of resource providers.
    /// </summary>
    public sealed class ResourceManager
    {
        private readonly List<IResourceProvider> _resourceProviders;

        public ResourceManager(List<IResourceProvider> resourceProviders)
        {
            _resourceProviders = resourceProviders;
        }

        public ResourceManager(params IResourceProvider[] resourceProviders) :
            this(resourceProviders.ToList())
        {
        }

        /// <summary>
        /// Lists the resources that have been modified since the last query.
        /// </summary>
        /// <returns>The modified resources</returns>
        public IReadOnlySet<ResourceName> GetAndClearModifiedResources()
        {
            var resources = new HashSet<ResourceName>();
            for (var i = _resourceProviders.Count - 1; i >= 0; i--)
                _resourceProviders[i].AddAndClearModifiedResources(resources);
            return resources;
        }

        /// <summary>
        /// Gets a resource by domain and path, or null.
        /// </summary>
        /// <param name="domain">The domain</param>
        /// <param name="path">The path</param>
        /// <returns>The resource, or null</returns>
        public IResource? GetResource(string domain, string path)
            => GetResource(new ResourceName(domain, path));

        /// <summary>
        /// Gets a resource by name, or null.
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The resource, or null</returns>
        public IResource? GetResource(ResourceName name)
        {
            GetResourceDelegate GetDelegate(int i)
            {
                return n =>
                {
                    if (i >= _resourceProviders.Count)
                        return null;
                    return _resourceProviders[i].GetResource(n, GetDelegate(i + 1));
                };
            }

            return GetDelegate(0)(name);
        }

        /// <summary>
        /// Tries to get a resource by domain and path.
        /// </summary>
        /// <param name="domain">The domain</param>
        /// <param name="path">The path</param>
        /// <param name="resource">The resource, if found</param>
        /// <returns>True if found, false otherwise</returns>
        public bool TryGetResource(string domain, string path, [NotNullWhen(true)] out IResource? resource)
            => TryGetResource(new ResourceName(domain, path), out resource);
        
        /// <summary>
        /// Tries to get a resource by name.
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="resource">The resource, if found</param>
        /// <returns>True if found, false otherwise</returns>
        public bool TryGetResource(ResourceName name, [NotNullWhen(true)] out IResource? resource)
        {
            resource = GetResource(name);
            return resource != null;
        }
        
        /// <summary>
        /// Gets a custom resource by domain and path, or null.
        /// </summary>
        /// <typeparam name="T">The custom resource type</typeparam>
        /// <param name="domain">The domain</param>
        /// <param name="path">The path</param>
        /// <returns>The resource, or null</returns>
        public T? Get<T>(string domain, string path) where T : class, ICustomResource
            => Get<T>(new ResourceName(domain, path));
        
        /// <summary>
        /// Gets a custom resource by name, or null.
        /// </summary>
        /// <typeparam name="T">The custom resource type</typeparam>
        /// <param name="name">The name</param>
        /// <returns>The resource, or null</returns>
        public T? Get<T>(ResourceName name) where T : class, ICustomResource
        {
            return CustomResource<T>.Load(this, name);
        }
        
        /// <summary>
        /// Tries to get a custom resource by domain and path.
        /// </summary>
        /// <typeparam name="T">The custom resource type</typeparam>
        /// <param name="domain">The domain</param>
        /// <param name="path">The path</param>
        /// <param name="resource">The resource, if found</param>
        /// <returns>True if found, false otherwise</returns>
        public bool TryGet<T>(string domain, string path, [NotNullWhen(true)] out T? resource) where T : class, ICustomResource
            => TryGet(new ResourceName(domain, path), out resource);
        
        /// <summary>
        /// Tries to get a custom resource by domain and path.
        /// </summary>
        /// <typeparam name="T">The custom resource type</typeparam>
        /// <param name="name">The name</param>
        /// <param name="resource">The resource, if found</param>
        /// <returns>True if found, false otherwise</returns>
        public bool TryGet<T>(ResourceName name, [NotNullWhen(true)] out T? resource) where T : class, ICustomResource
        {
            resource = Get<T>(name);
            return resource != null;
        }
    }
}