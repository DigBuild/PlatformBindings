using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
{
    public sealed class FramebufferFormat : IDisposable
    {
        internal readonly NativeHandle Handle;
        public IReadOnlyList<RenderStage> Stages { get; }
        public IReadOnlyList<FramebufferAttachment> Attachments { get; }

        internal FramebufferFormat(NativeHandle handle, IReadOnlyList<RenderStage> stages, IReadOnlyList<FramebufferAttachment> attachments)
        {
            Handle = handle;
            Stages = stages;
            Attachments = attachments;
        }

        public void Dispose() => Handle.Dispose();
    }

    public class FramebufferAttachment
    {
        internal FramebufferFormat Format = null!;
        internal readonly uint Id;
        public readonly Vector4 ClearColor;

        internal FramebufferAttachment(uint id, Vector4 clearColor)
        {
            Id = id;
            ClearColor = clearColor;
        }
    }

    public sealed class FramebufferColorAttachment : FramebufferAttachment
    {
        internal FramebufferColorAttachment(uint id, Vector4 clearColor) : base(id, clearColor)
        {
        }
    }

    public sealed class FramebufferDepthStencilAttachment : FramebufferAttachment
    {
        internal FramebufferDepthStencilAttachment(uint id) : base(id, Vector4.Zero)
        {
        }
    }

    public sealed class RenderStage
    {
        public FramebufferFormat Format;
        internal readonly uint Id;

        internal RenderStage(uint id, FramebufferFormat format = null!)
        {
            Id = id;
            Format = format;
        }
    }

    public readonly ref struct FramebufferFormatBuilder
    {
        private sealed class Data
        {
            internal readonly List<FramebufferAttachment> Attachments = new();
            internal readonly List<RenderStage> Stages = new();

            internal readonly List<AttachmentDescriptor> AttachmentDescriptors = new();
            internal readonly List<RenderStageDescriptor> RenderStageDescriptors = new();
            internal readonly List<uint> RenderStageMembers = new();
            internal readonly List<uint> RenderStageDependencies = new();
        }

        private readonly RenderContext _context;
        private readonly Data _data;

        internal FramebufferFormatBuilder(RenderContext context)
        {
            _context = context;
            _data = new Data();
        }

        public FramebufferFormatBuilder WithColorAttachment(
            out FramebufferColorAttachment attachment,
            TextureFormat format,
            Vector4 clearColor = default
        )
        {
            _data.Attachments.Add(
                attachment = new FramebufferColorAttachment(
                    (uint)_data.Attachments.Count,
                    clearColor
                )
            );
            _data.AttachmentDescriptors.Add(new AttachmentDescriptor(AttachmentType.Color, format.Id));
            return this;
        }

        public FramebufferFormatBuilder WithDepthStencilAttachment(
            out FramebufferDepthStencilAttachment attachment
        )
        {
            _data.Attachments.Add(
                attachment = new FramebufferDepthStencilAttachment(
                    (uint)_data.Attachments.Count
                )
            );
            _data.AttachmentDescriptors.Add(new AttachmentDescriptor(AttachmentType.DepthStencil, 0xFF));
            return this;
        }

        public FramebufferFormatBuilder WithStage(
            out RenderStage stage,
            params FramebufferColorAttachment[] attachments
        )
        {
            if (attachments.Length < 1)
                throw new ArgumentException("Must specify at least one color attachment.", nameof(attachments));

            _data.Stages.Add(stage = new RenderStage((uint) _data.Stages.Count));

            _data.RenderStageDescriptors.Add(new RenderStageDescriptor(
                (uint)_data.RenderStageMembers.Count,
                (uint)attachments.Length,
                0xFFFFFFFF
            ));
            foreach (var attachment in attachments)
                _data.RenderStageMembers.Add(attachment.Id);

            return this;
        }

        public FramebufferFormatBuilder WithStage(
            out RenderStage stage,
            FramebufferDepthStencilAttachment depthStencil,
            params FramebufferColorAttachment[] attachments
        )
        {
            if (attachments.Length < 1)
                throw new ArgumentException("Must specify at least one color attachment.", nameof(attachments));

            _data.Stages.Add(stage = new RenderStage((uint)_data.Stages.Count));

            _data.RenderStageDescriptors.Add(new RenderStageDescriptor(
                (uint)_data.RenderStageMembers.Count,
                (uint)attachments.Length,
                depthStencil.Id
            ));
            foreach (var attachment in attachments)
                _data.RenderStageMembers.Add(attachment.Id);

            return this;
        }

        public FramebufferFormatBuilder WithDependency(
            RenderStage stage,
            params RenderStage[] dependencies
        )
        {
            var descriptor = _data.RenderStageDescriptors[(int)stage.Id];
            _data.RenderStageDescriptors[(int) stage.Id] = new RenderStageDescriptor(
                descriptor,
                (uint)_data.RenderStageDependencies.Count,
                (uint)dependencies.Length
            );
            foreach (var dependency in dependencies)
                _data.RenderStageDependencies.Add(dependency.Id);

            return this;
        }

        public static unsafe implicit operator FramebufferFormat(FramebufferFormatBuilder builder)
        {
            var span1 = new Span<AttachmentDescriptor>(builder._data.AttachmentDescriptors.ToArray());
            var span2 = new Span<RenderStageDescriptor>(builder._data.RenderStageDescriptors.ToArray());
            var span3 = new Span<uint>(builder._data.RenderStageMembers.ToArray());
            var span4 = new Span<uint>(builder._data.RenderStageDependencies.ToArray());

            fixed (AttachmentDescriptor* p1 = &span1.GetPinnableReference())
            fixed (RenderStageDescriptor* p2 = &span2.GetPinnableReference())
            fixed (uint* p3 = &span3.GetPinnableReference())
            fixed (uint* p4 = &span4.GetPinnableReference())
            {
                var format = new FramebufferFormat(
                    new NativeHandle(
                        RenderContext.Bindings.CreateFramebufferFormat(
                            builder._context.Ptr,
                            new IntPtr(p1),
                            (uint)span1.Length,
                            new IntPtr(p2),
                            (uint)span2.Length,
                            new IntPtr(p3),
                            new IntPtr(p4)
                        )
                    ),
                    builder._data.Stages.ToImmutableList(),
                    builder._data.Attachments
                );

                foreach (var attachment in builder._data.Attachments)
                    attachment.Format = format;
                foreach (var stage in builder._data.Stages)
                    stage.Format = format;

                return format;
            }
        }

        internal enum AttachmentType : byte
        {
            Color,
            DepthStencil
        }

        internal readonly struct AttachmentDescriptor
        {
            private readonly AttachmentType _type;
            private readonly byte _format;

            internal AttachmentDescriptor(AttachmentType type, byte format)
            {
                _type = type;
                _format = format;
            }
        }

        internal readonly struct RenderStageDescriptor
        {
            private readonly uint _memberStart;
            private readonly uint _memberCount;
            private readonly uint _depthStencilId;
            private readonly uint _dependencyStart;
            private readonly uint _dependencyCount;

            internal RenderStageDescriptor(
                uint memberStart, uint memberCount,
                uint depthStencilId
            )
            {
                _memberStart = memberStart;
                _memberCount = memberCount;
                _depthStencilId = depthStencilId;
                _dependencyStart = _dependencyCount = 0;
            }

            public RenderStageDescriptor(
                RenderStageDescriptor parent,
                uint dependencyStart, uint dependencyCount
            ) : this(parent._memberStart, parent._memberCount, parent._depthStencilId)
            {
                _dependencyStart = dependencyStart;
                _dependencyCount = dependencyCount;
            }
        }
    }
}