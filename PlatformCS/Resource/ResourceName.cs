using System;

namespace DigBuild.Platform.Resource
{
    /// <summary>
    /// A resource name.
    /// </summary>
    public readonly struct ResourceName : IEquatable<ResourceName>
    {
        private readonly string _domain, _path;

        /// <summary>
        /// The domain.
        /// </summary>
        public string Domain => _domain;
        /// <summary>
        /// The path.
        /// </summary>
        public string Path => _path;

        public ResourceName(string domain, string path)
        {
            _domain = domain;
            _path = path;
        }

        public ResourceName GetSibling(string name)
        {
            var parentDir = System.IO.Path.GetDirectoryName(_path)?.Replace('\\', '/') ?? string.Empty;
            var newPath = parentDir.Length == 0 ? name : $"{parentDir}/{name}";
            return new ResourceName(_domain, newPath);
        }

        public bool Equals(ResourceName other)
        {
            return _domain == other._domain && _path == other._path;
        }

        public override bool Equals(object? obj)
        {
            return obj is ResourceName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_domain, _path);
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
            return $"{_domain}:{_path}";
        }

        /// <summary>
        /// Parses a string representation of the resource name, or null if invalid.
        /// </summary>
        /// <param name="str">The string</param>
        /// <returns>The resource name, or null</returns>
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