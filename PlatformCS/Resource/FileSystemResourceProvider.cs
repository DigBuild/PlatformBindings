using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

namespace DigBuild.Platform.Resource
{
    /// <summary>
    /// <see cref="IResourceProvider"/> implementation for local filesystem content roots. Supports file modification monitoring.
    /// </summary>
    public sealed class FileSystemResourceProvider : IResourceProvider, IDisposable
    {
        private readonly IReadOnlyDictionary<string, string> _contentRoots;
        private readonly ImmutableList<FileSystemWatcher> _watchers;
        private readonly HashSet<ResourceName> _modifiedResources = new();

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
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.Attributes,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };
                watchers.Add(watcher);

                void OnEvent(object _, FileSystemEventArgs args)
                {
                    if (args.Name == null || args.Name.Contains('~')) return;
                    if (Directory.Exists(args.FullPath)) return;
                    _modifiedResources.Add(new ResourceName(domain, args.Name.Replace('\\', '/')));
                }
                void OnRenamed(object _, RenamedEventArgs args)
                {
                    if (args.OldName == null || args.Name == null) return;
                    if (!args.OldName.Contains('~'))
                        _modifiedResources.Add(new ResourceName(domain, args.OldName.Replace('\\', '/')));
                    if (!args.Name.Contains('~'))
                        _modifiedResources.Add(new ResourceName(domain, args.Name.Replace('\\', '/')));
                }

                watcher.Created += OnEvent;
                watcher.Changed += OnEvent;
                watcher.Renamed += OnRenamed;
                watcher.Deleted += OnEvent;
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

        public void AddAndClearModifiedResources(ISet<ResourceName> resources)
        {
            foreach (var resource in _modifiedResources)
                resources.Add(resource);
            _modifiedResources.Clear();
        }

        public IResource? GetResource(ResourceName name, GetResourceDelegate parent)
        {
            if (!_contentRoots.TryGetValue(name.Domain, out var rootDir))
                return parent(name);

            var fullPath = Path.GetFullPath(
                Path.TrimEndingDirectorySeparator(rootDir) +
                Path.DirectorySeparatorChar +
                name.Path
            );
            if (File.Exists(fullPath))
                return new Resource(fullPath, name, File.GetLastWriteTime(fullPath));

            return parent(name);
        }

        private sealed class Resource : ResourceBase
        {
            private readonly string _path;
            public override ResourceName Name { get; }
            public override DateTime LastEdited { get; }

            public Resource(string path, ResourceName name, DateTime lastEdited)
            {
                _path = path;
                Name = name;
                LastEdited = lastEdited;
            }

            public override string? FileSystemPath => _path;

            public override Stream OpenStream()
            {
                return File.OpenRead(_path);
            }
        }
    }
}