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

        IReadOnlySet<ResourceName> GetAndClearModifiedResources()
        {
            GetAndClearModifiedResourcesDelegate GetDelegate(int i)
            {
                return () =>
                {
                    if (i >= _resourceProviders.Count)
                        return ImmutableHashSet<ResourceName>.Empty;
                    return _resourceProviders[i].GetAndClearModifiedResources(GetDelegate(i + 1));
                };
            }

            return GetDelegate(0)();
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
            return CustomResource<T>.Load(name, this);
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