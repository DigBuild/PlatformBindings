using System;
using System.Numerics;
using DigBuild.Platform.Resource;
using OpenAL;

namespace DigBuild.Platform.Audio
{
    public sealed class AudioSystem : IDisposable
    {
        private readonly IntPtr _device;
        private readonly IntPtr _context;

        private Vector3 _listenerPosition;
        private Vector3 _listenerVelocity;
        
        public Vector3 ListenerPosition
        {
            get => _listenerPosition;
            set
            {
                _listenerPosition = value;
                Al.Listener3f(Al.Position, value.X, value.Y, value.Z);
            }
        }

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

        public AudioClip Load(IResource resource) => new(resource);

        public AudioPlayer CreatePlayer() => new();
    }
}