using System.Collections.Generic;

namespace DigBuild.Platform.Resource
{
    public delegate IResource? GetResourceDelegate(ResourceName name);

    public interface IResourceProvider
    {
        void AddAndClearModifiedResources(ISet<ResourceName> resources);
        IResource? GetResource(ResourceName name, GetResourceDelegate parent);
    }
}