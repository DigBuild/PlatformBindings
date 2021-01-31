using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace DigBuildPlatformCS.Resource
{
    public sealed class FileSystemResourceProvider : IResourceProvider, IDisposable
    {
        private readonly IReadOnlyDictionary<string, string> _contentRoots;
        private readonly ImmutableList<FileSystemWatcher> _watchers;
        private HashSet<ResourceName> _modifiedResources = new();

        public FileSystemResourceProvider(IReadOnlyDictionary<string, string> contentRoots, bool watch = false)
        {
            _contentRoots = contentRoots;

            if (!watch)
            {
                _watchers = ImmutableList<FileSystemWatcher>.Empty;
                return;
            }

            List<FileSystemWatcher> watchers = new();
            foreach (var (domain, rootDir) in contentRoots)
            {
                var watcher = new FileSystemWatcher(rootDir)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.Attributes,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };
                watchers.Add(watcher);
                watcher.Changed += (_, args) =>
                {
                    if (args.Name == null) return;
                    _modifiedResources.Add(new ResourceName(domain, args.Name));
                };
            }
            _watchers = watchers.ToImmutableList();
        }

        ~FileSystemResourceProvider()
        {
            Dispose();
        }

        public void Dispose()
        {
            _watchers.ForEach(w => w.Dispose());
            GC.SuppressFinalize(this);
        }

        public IReadOnlySet<ResourceName> GetAndClearModifiedResources(GetAndClearModifiedResourcesDelegate parent)
        {
            HashSet<ResourceName> names;
            (names, _modifiedResources) = (_modifiedResources, new HashSet<ResourceName>());
            names.UnionWith(parent());
            return names.ToImmutableHashSet();
        }

        public IResource? GetResource(ResourceName name, GetResourceDelegate parent)
        {
            if (_contentRoots.TryGetValue(name.Domain, out var rootDir))
            {
                var fullPath = Path.GetFullPath(
                    Path.TrimEndingDirectorySeparator(rootDir) +
                    Path.DirectorySeparatorChar +
                    name.Path
                );
                if (File.Exists(fullPath))
                    return new Resource(fullPath, name);
            }

            return parent(name);
        }

        private sealed class Resource : ResourceBase
        {
            private readonly string _path;
            public override ResourceName Name { get; }

            public Resource(string path, ResourceName name)
            {
                _path = path;
                Name = name;
            }

            public override Stream OpenStream()
            {
                return File.OpenRead(_path);
            }
        }
    }
}