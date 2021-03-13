using System;

namespace DigBuild.Platform.Resource
{
    public readonly struct ResourceName : IEquatable<ResourceName>
    {
        public readonly string Domain, Path;

        public ResourceName(string domain, string path)
        {
            Domain = domain;
            Path = path;
        }

        public bool Equals(ResourceName other)
        {
            return Domain == other.Domain && Path == other.Path;
        }

        public override bool Equals(object? obj)
        {
            return obj is ResourceName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Domain, Path);
        }

        public static bool operator ==(ResourceName left, ResourceName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ResourceName left, ResourceName right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{Domain}:{Path}";
        }
    }
}