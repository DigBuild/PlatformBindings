﻿using System.IO;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Resource
{
    public interface IResource
    {
        ResourceName Name { get; }

        Stream OpenStream();
        byte[] ReadAllBytes();
    }

    public abstract class ResourceBase : IResource
    {
        public abstract ResourceName Name { get; }

        public abstract Stream OpenStream();

        public byte[] ReadAllBytes()
        {
            using var stream = OpenStream();
            return StreamUtils.ReadAllBytes(stream);
        }
    }
}