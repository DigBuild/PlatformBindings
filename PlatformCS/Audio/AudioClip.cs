using System;
using DigBuild.Platform.Resource;
using NVorbis;
using OpenAL;

namespace DigBuild.Platform.Audio
{
    public sealed class AudioClip : IDisposable
    {
        internal readonly uint Buffer;

        internal unsafe AudioClip(IResource resource)
        {
            Al.GenBuffer(out Buffer);

            using var reader = new VorbisReader(resource.OpenStream());

            var channels = reader.Channels;
            var sampleRate = reader.SampleRate;
            var seconds = reader.TotalTime.TotalSeconds;
            var samples = (int) Math.Ceiling(seconds * sampleRate * channels);

            var floats = new Span<float>(new float[samples]);
            if (reader.ReadSamples(floats) <= 0)
                throw new Exception("Failed to read OGG stream.");
                
            var shorts = new Span<short>(new short[samples]); // 16 bit
            for (var i = 0; i < floats.Length; i++)
                shorts[i] = (short) (short.MaxValue * floats[i]);

            fixed (void* p = &shorts.GetPinnableReference())
            {
                Al.BufferData(Buffer, channels == 2 ? Al.FormatStereo16 : Al.FormatMono16, p, shorts.Length * sizeof(short), sampleRate);
            }
        }

        ~AudioClip()
        {
            Al.DeleteBuffer(Buffer);
        }

        public void Dispose()
        {
            Al.DeleteBuffer(Buffer);
            GC.SuppressFinalize(this);
        }
    }
}