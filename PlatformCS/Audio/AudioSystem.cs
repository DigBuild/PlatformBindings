using System;
using System.Numerics;
using DigBuild.Platform.Resource;
using OpenAL;

namespace DigBuild.Platform.Audio
{
    /// <summary>
    /// A cross-platform audio system.
    /// </summary>
    public sealed class AudioSystem : IDisposable
    {
        private readonly IntPtr _device;
        private readonly IntPtr _context;

        private Vector3 _listenerPosition;
        private Vector3 _listenerVelocity;
        
        /// <summary>
        /// The listener's position in 3D space.
        /// </summary>
        public Vector3 ListenerPosition
        {
            get => _listenerPosition;
            set
            {
                _listenerPosition = value;
                Al.Listener3f(Al.Position, value.X, value.Y, value.Z);
            }
        }

        /// <summary>
        /// The listener's velocity in 3D space.
        /// </summary>
        public Vector3 ListenerVelocity
        {
            get => _listenerVelocity;
            set
            {
                _listenerVelocity = value;
                Al.Listener3f(Al.Velocity, value.X, value.Y, value.Z);
            }
        }

        internal AudioSystem()
        {
            _device = Alc.OpenDevice(null);
            _context = Alc.CreateContext(_device, null);
            Alc.MakeContextCurrent(_context);

            ListenerPosition = Vector3.Zero;
            ListenerVelocity = Vector3.Zero;
        }

        ~AudioSystem()
        {
            Alc.DestroyContext(_context);
            Alc.CloseDevice(_device);
        }

        public void Dispose()
        {
            Alc.DestroyContext(_context);
            Alc.CloseDevice(_device);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Loads a resource as an audio clip.
        /// </summary>
        /// <param name="resource">The resource to be loaded</param>
        /// <returns>The audio clip</returns>
        public AudioClip Load(IResource resource) => new(resource);

        /// <summary>
        /// Creates a new audio player.
        /// </summary>
        /// <returns>The audio player</returns>
        public AudioPlayer CreatePlayer() => new();
    }
}