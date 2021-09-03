using System;
using System.Reflection;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Resource
{
    /// <summary>
    /// A custom resource. Implementing types <strong>MUST</strong> contain a public static
    /// method with the signature <c>Load(<see cref="ResourceManager"/>, <see cref="ResourceName"/>)</c>.
    /// </summary>
    public interface ICustomResource
    {
        /// <summary>
        /// The name of the resource.
        /// </summary>
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