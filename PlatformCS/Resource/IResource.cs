using System;
using System.IO;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Resource
{
    /// <summary>
    /// A streamable resource with a name.
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// The name of the resource.
        /// </summary>
        ResourceName Name { get; }

        /// <summary>
        /// The last time it was edited, or <see cref="DateTime.MinValue"/> if unavailable.
        /// </summary>
        DateTime LastEdited { get; }

        /// <summary>
        /// The filesystem path of the resource, or null if unavailable.
        /// </summary>
        string? FileSystemPath { get; }

        /// <summary>
        /// Opens a read stream for the resource.
        /// </summary>
        /// <returns>The stream</returns>
        Stream OpenStream();
        /// <summary>
        /// Reads all the bytes of the resource.
        /// </summary>
        /// <returns>The bytes</returns>
        byte[] ReadAllBytes();
    }

    /// <summary>
    /// A base implementation of <see cref="IResource"/>.
    /// </summary>
    public abstract class ResourceBase : IResource
    {
        public abstract ResourceName Name { get; }

        public abstract DateTime LastEdited { get; }

        public virtual string? FileSystemPath => null;

        public abstract Stream OpenStream();

        public byte[] ReadAllBytes()
        {
            using var stream = OpenStream();
            return StreamUtils.ReadAllBytes(stream);
        }
    }
}