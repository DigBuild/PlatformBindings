using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DigBuild.Platform.Resource
{
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

        public IReadOnlySet<ResourceName> GetAndClearModifiedResources()
        {
            var resources = new HashSet<ResourceName>();
            for (var i = _resourceProviders.Count - 1; i >= 0; i--)
                _resourceProviders[i].AddAndClearModifiedResources(resources);
            return resources;
        }

        public IResource? GetResource(string domain, string path)
            => GetResource(new ResourceName(domain, path));

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

        public bool TryGetResource(string domain, string path, [NotNullWhen(true)] out IResource? resource)
            => TryGetResource(new ResourceName(domain, path), out resource);

        public bool TryGetResource(ResourceName name, [NotNullWhen(true)] out IResource? resource)
        {
            resource = GetResource(name);
            return resource != null;
        }
        
        public T? Get<T>(string domain, string path) where T : class, ICustomResource
            => Get<T>(new ResourceName(domain, path));

        public T? Get<T>(ResourceName name) where T : class, ICustomResource
        {
            return CustomResource<T>.Load(this, name);
        }
        
        public bool TryGet<T>(string domain, string path, [NotNullWhen(true)] out T? resource) where T : class, ICustomResource
            => TryGet(new ResourceName(domain, path), out resource);

        public bool TryGet<T>(ResourceName name, [NotNullWhen(true)] out T? resource) where T : class, ICustomResource
        {
            resource = Get<T>(name);
            return resource != null;
        }
    }
}