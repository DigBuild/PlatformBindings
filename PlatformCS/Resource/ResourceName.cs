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

        public static ResourceName? Parse(string str)
        {
            var firstColon = str.IndexOf(':');
            if (firstColon == -1) return null;
            var lastColon = str.LastIndexOf(':');
            if (lastColon != firstColon) return null;

            return new ResourceName(str[..firstColon], str[(firstColon + 1)..]);
        }
    }
}