using System;
using System.Reflection;
using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS.Resource
{
    public interface ICustomResource
    {
        public ResourceName Name { get; }
    }

    public static class CustomResource<T> where T : class, ICustomResource
    {
        public delegate T LoadDelegate(ResourceName name, ResourceManager manager);

        public static readonly LoadDelegate Load;

        static CustomResource()
        {
            var methodInfo = typeof(T).GetMethod("Load", BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
                throw new ResourceLoaderMissingException(typeof(T));
            Load = (LoadDelegate)Delegate.CreateDelegate(typeof(LoadDelegate), methodInfo);
        }
    }
}