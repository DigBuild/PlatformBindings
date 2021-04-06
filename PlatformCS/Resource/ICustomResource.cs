using System;
using System.Reflection;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Resource
{
    public interface ICustomResource
    {
        public ResourceName Name { get; }
    }

    internal static class CustomResource<T> where T : class, ICustomResource
    {
        internal delegate T? LoadDelegate(ResourceManager manager, ResourceName name);

        internal static readonly LoadDelegate Load;

        static CustomResource()
        {
            var methodInfo = typeof(T).GetMethod("Load", BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
                throw new ResourceLoaderMissingException(typeof(T));
            Load = (LoadDelegate)Delegate.CreateDelegate(typeof(LoadDelegate), methodInfo);
        }
    }
}