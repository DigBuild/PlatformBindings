﻿using System;
using System.Numerics;
using OpenAL;

namespace DigBuild.Platform.Audio
{
    /// <summary>
    /// An audio player.
    /// </summary>
    public sealed class AudioPlayer : IDisposable
    {
        private readonly uint _source;

        private float _gain;
        private float _pitch;
        private float _maxDistance;
        private bool _listenerRelative;
        private Vector3 _position;
        private Vector3 _velocity;
        private AudioClip? _clip;

        /// <summary>
        /// The gain (or volume) of the sound being played.
        /// </summary>
        public float Gain
        {
            get => _gain;
            set
            {
                _gain = value;
                Al.Sourcef(_source, Al.Gain, value);
            }
        }

        /// <summary>
        /// The pitch of the sound being played.
        /// </summary>
        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                Al.Sourcef(_source, Al.Pitch, value);
            }
        }

        /// <summary>
        /// The maximum distance at which the sound will be audible.
        /// </summary>
        public float MaxDistance
        {
            get => _maxDistance;
            set
            {
                _maxDistance = value;
                Al.Sourcef(_source, Al.MaxDistance, value);
            }
        }

        /// <summary>
        /// Whether the player's position is relative to the listener or to the world.
        /// </summary>
        /// <see cref="Position"/>
        public bool ListenerRelative
        {
            get => _listenerRelative;
            set
            {
                _listenerRelative = value;
                Al.Sourcei(_source, Al.SourceRelative, value ? 1 : 0);
            }
        }

        /// <summary>
        /// The position of the player.
        /// </summary>
        /// <see cref="ListenerRelative"/>
        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                Al.Source3f(_source, Al.Position, value.X, value.Y, value.Z);
            }
        }

        /// <summary>
        /// The velocity of the player.
        /// </summary>
        public Vector3 Velocity
        {
            get => _velocity;
            set
            {
                _velocity = value;
                Al.Source3f(_source, Al.Velocity, value.X, value.Y, value.Z);
            }
        }

        /// <summary>
        /// The current playback status.
        /// </summary>
        public PlayStatus Status
        {
            get
            {
                Al.GetSourcei(_source, Al.SourceState, out var state);
                return state switch
                {
                    Al.Playing => PlayStatus.Playing,
                    Al.Paused => PlayStatus.Paused,
                    _ => PlayStatus.Stopped
                };
            }
        }

        /// <summary>
        /// The current audio clip.
        /// </summary>
        public AudioClip? Clip
        {
            get => _clip;
            set
            {
                _clip = value;
                Al.Sourcei(_source, Al.Buffer, (int) (value?.Buffer ?? 0));
            }
        }

        internal AudioPlayer()
        {
            Al.GenSource(out _source);

            Gain = 1;
            Pitch = 1;
            MaxDistance = float.PositiveInfinity;
            ListenerRelative = false;
            Position = Vector3.Zero;
            Velocity = Vector3.Zero;
        }

        ~AudioPlayer()
        {
            Al.DeleteSource(_source);
        }

        public void Dispose()
        {
            Al.DeleteSource(_source);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Begins playback of an audio clip.
        /// </summary>
        /// <param name="clip">The clip to play</param>
        /// <param name="looping">Whether it should loop when it ends</param>
        public void Play(AudioClip clip, bool looping = false)
        {
            Clip = clip;
            Play(looping);
        }

        /// <summary>
        /// Begins playback of the currently loaded audio clip.
        /// </summary>
        /// <param name="looping">Whether it should loop when it ends</param>
        public void Play(bool looping = false)
        {
            Al.Sourcei(_source, Al.Looping, looping ? 1 : 0);
            Al.SourcePlay(_source);
        }

        /// <summary>
        /// Pauses playback.
        /// </summary>
        public void Pause()
        {
            Al.SourcePause(_source);
        }

        /// <summary>
        /// Stops playback.
        /// </summary>
        public void Stop()
        {
            Al.SourceStop(_source);
        }

        /// <summary>
        /// Playback status.
        /// </summary>
        public enum PlayStatus
        {
            Stopped,
            Playing,
            Paused
        }
    }
}