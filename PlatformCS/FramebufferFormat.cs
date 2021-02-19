using DigBuildPlatformCS.Util;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DigBuildPlatformCS
{
    public sealed class FramebufferFormat : IDisposable
    {
        internal readonly NativeHandle Handle;
        internal readonly uint StageCount;
        public readonly IReadOnlyList<FramebufferAttachment> Attachments;

        internal FramebufferFormat(NativeHandle handle, uint stageCount, IReadOnlyList<FramebufferAttachment> attachments)
        {
            Handle = handle;
            StageCount = stageCount;
            Attachments = attachments;
        }

        public void Dispose() => Handle.Dispose();
    }

    public sealed class FramebufferAttachment
    {
        internal readonly uint Id;
        public readonly Vector4 ClearColor;

        internal FramebufferAttachment(uint id, Vector4 clearColor)
        {
            Id = id;
            ClearColor = clearColor;
        }
    }

    public sealed class RenderStage
    {
        internal readonly uint Id;
        internal readonly FramebufferFormat Format;

        internal RenderStage(uint id, FramebufferFormat format)
        {
            Id = id;
            Format = format;
        }
    }

    public readonly ref struct FramebufferFormatBuilder
    {
        private sealed class Data
        {

        }

        private readonly RenderContext _ctx;
        private readonly Data _data;

        internal FramebufferFormatBuilder(RenderContext ctx)
        {
            _ctx = ctx;
            _data = new Data();
        }

        public FramebufferFormatBuilder WithColorAttachment<T>(
            out FramebufferAttachment attachment,
            TextureFormat<T> format,
            T clearValue = default
        ) where T : unmanaged
        {
            attachment = new FramebufferAttachment(/* _data.Attachments.Count */ 0, format.ToVector4(clearValue));

            return this;
        }

        public FramebufferFormatBuilder WithDepthStencilAttachment(
            out FramebufferAttachment attachment
        ) => throw new NotImplementedException();

        public FramebufferFormatBuilder WithStage(
            out RenderStage stage,
            params FramebufferAttachment[] attachments
        ) => throw new NotImplementedException();

        public FramebufferFormatBuilder WithDependency(
            RenderStage stage,
            params RenderStage[] dependencies
        ) => throw new NotImplementedException();

        public static implicit operator FramebufferFormat(FramebufferFormatBuilder builder)
            => throw new NotImplementedException();
    }
}