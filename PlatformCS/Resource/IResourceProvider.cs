﻿using System.Collections.Generic;

namespace DigBuild.Platform.Resource
{
    public delegate IReadOnlySet<ResourceName> GetAndClearModifiedResourcesDelegate();
    public delegate IResource? GetResourceDelegate(ResourceName name);

    public interface IResourceProvider
    {
        IReadOnlySet<ResourceName> GetAndClearModifiedResources(GetAndClearModifiedResourcesDelegate parent);
        IResource? GetResource(ResourceName name, GetResourceDelegate parent);
    }
}