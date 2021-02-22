using AdvancedDLSupport;
using DigBuildPlatformCS.Util;
using System;
using System.Runtime.InteropServices;

namespace DigBuildPlatformCS
{
    [NativeSymbols("dbp_command_buffer_", SymbolTransformationMethod.Underscore)]
    internal interface ICommandBufferBindings
    {
        void Commit(IntPtr instance, IntPtr context, IntPtr format, IntPtr commands, uint commandCount);
    }

    public sealed class CommandBuffer
    {
        internal static readonly ICommandBufferBindings Bindings = NativeLib.Get<ICommandBufferBindings>();

        internal readonly NativeHandle Handle;

        internal CommandBuffer(NativeHandle handle)
        {
            Handle = handle;
        }
    }

    public sealed class CommandBufferWriter
    {
        internal NativeHandle? Handle;
        internal bool Recording;

        public CommandBufferRecorder BeginRecording(FramebufferFormat format, NativeBufferPool bufferPool)
        {
            if (Handle == null)
                throw new InvalidOperationException("Not initialized.");
            if (Recording)
                throw new AlreadyRecordingException();
            Recording = true;
            return new CommandBufferRecorder(this, format, bufferPool);
        }
    }

    public sealed class CommandBufferRecorder
    {
        private readonly CommandBufferWriter _writer;
        private readonly FramebufferFormat _format;
        private readonly PooledNativeBuffer<CommandBufferCmd> _commands;
        private bool _committed;

        internal CommandBufferRecorder(
            CommandBufferWriter writer,
            FramebufferFormat format,
            NativeBufferPool bufferPool
        )
        {
            _writer = writer;
            _format = format;
            _commands = bufferPool.Request<CommandBufferCmd>();
        }

        public void SetViewportAndScissor(IRenderTarget renderTarget)
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _commands.Add(new CommandBufferCmd.SetViewportScissor(renderTarget.Handle));
        }

        public void SetViewportAndScissor(Extents2D extents)
        {
            SetViewport(extents);
            SetScissor(extents);
        }

        public void SetViewport(Extents2D extents)
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _commands.Add(new CommandBufferCmd.SetViewport(extents));
        }

        public void SetScissor(Extents2D extents)
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _commands.Add(new CommandBufferCmd.SetScissor(extents));
        }
        
        public void Using<TUniform>(
            IRenderPipeline pipeline,
            UniformBuffer<TUniform> uniformBuffer,
            uint index
        ) where TUniform : unmanaged, IUniform<TUniform>
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _commands.Add(new CommandBufferCmd.BindUniform(pipeline.Handle, uniformBuffer.Handle, index));
        }

        public void Draw<TVertex>(
            RenderPipeline<TVertex> pipeline,
            VertexBuffer<TVertex> vertexBuffer
        ) where TVertex : unmanaged
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _commands.Add(new CommandBufferCmd.Draw(pipeline.Handle, vertexBuffer.Handle, IntPtr.Zero));
        }

        public void Draw<TVertex, TInstance>(
            RenderPipeline<TVertex, TInstance> pipeline,
            VertexBuffer<TVertex> vertexBuffer,
            VertexBuffer<TInstance> instanceBuffer
        ) where TVertex : unmanaged
            where TInstance : unmanaged
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _commands.Add(new CommandBufferCmd.Draw(pipeline.Handle, vertexBuffer.Handle, instanceBuffer.Handle));
        }
        
        public void Commit(RenderContext context)
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _committed = true;
            _writer.Recording = false;

            var unpooled = _commands.Unpooled;
            CommandBuffer.Bindings.Commit(_writer.Handle!, context.Ptr, _format.Handle, unpooled.Ptr, unpooled.Count);
            _commands.Dispose();
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct CommandBufferCmd
    {
        [FieldOffset(0)] private readonly Type _type;

        [FieldOffset(sizeof(Type))] private readonly SetViewportScissor _setViewportScissor;
        [FieldOffset(sizeof(Type))] private readonly SetViewport _setViewport;
        [FieldOffset(sizeof(Type))] private readonly SetScissor _setScissor;
        [FieldOffset(sizeof(Type))] private readonly BindUniform _bindUniform;
        [FieldOffset(sizeof(Type))] private readonly Draw _draw;

        private CommandBufferCmd(SetViewportScissor setViewportScissor) : this()
        {
            _type = Type.SetViewportScissor;
            _setViewportScissor = setViewportScissor;
        }

        private CommandBufferCmd(SetViewport setViewport) : this()
        {
            _type = Type.SetViewport;
            _setViewport = setViewport;
        }

        private CommandBufferCmd(SetScissor setScissor) : this()
        {
            _type = Type.SetScissor;
            _setScissor = setScissor;
        }

        private CommandBufferCmd(BindUniform bindUniform) : this()
        {
            _type = Type.BindUniform;
            _bindUniform = bindUniform;
        }

        private CommandBufferCmd(Draw draw) : this()
        {
            _type = Type.Draw;
            _draw = draw;
        }

        public static implicit operator CommandBufferCmd(SetViewportScissor cmd) => new(cmd);
        public static implicit operator CommandBufferCmd(SetViewport cmd) => new(cmd);
        public static implicit operator CommandBufferCmd(SetScissor cmd) => new(cmd);
        public static implicit operator CommandBufferCmd(BindUniform cmd) => new(cmd);
        public static implicit operator CommandBufferCmd(Draw cmd) => new(cmd);

        internal enum Type : ulong
        {
            SetViewportScissor,
            SetViewport,
            SetScissor,
            BindUniform,
            Draw
        }

        internal readonly struct SetViewportScissor
        {
            private readonly IntPtr _target;

            internal SetViewportScissor(IntPtr target)
            {
                _target = target;
            }
        }

        internal readonly struct SetViewport
        {
            private readonly Extents2D _extents;

            internal SetViewport(Extents2D extents)
            {
                _extents = extents;
            }
        }

        internal readonly struct SetScissor
        {
            private readonly Extents2D _extents;

            internal SetScissor(Extents2D extents)
            {
                _extents = extents;
            }
        }

        internal readonly struct BindUniform
        {
            private readonly IntPtr _pipeline;
            private readonly IntPtr _uniformBuffer;
            private readonly uint _index;

            internal BindUniform(IntPtr pipeline, IntPtr uniformBuffer, uint index)
            {
                _pipeline = pipeline;
                _uniformBuffer = uniformBuffer;
                _index = index;
            }
        }

        internal readonly struct Draw
        {
            private readonly IntPtr _pipeline;
            private readonly IntPtr _vertexBuffer;
            private readonly IntPtr _instanceBuffer;

            internal Draw(IntPtr pipeline, IntPtr vertexBuffer, IntPtr instanceBuffer)
            {
                _pipeline = pipeline;
                _vertexBuffer = vertexBuffer;
                _instanceBuffer = instanceBuffer;
            }
        }

    }

    public readonly ref struct CommandBufferBuilder
    {
        private readonly RenderContext _context;
        private readonly CommandBufferWriter? _writer;

        internal CommandBufferBuilder(RenderContext context) : this()
        {
            _context = context;
        }

        internal CommandBufferBuilder(RenderContext context, out CommandBufferWriter writer)
        {
            _context = context;
            _writer = writer = new CommandBufferWriter();
        }

        public static implicit operator CommandBuffer(CommandBufferBuilder builder)
        {
            var handle = new NativeHandle(RenderContext.Bindings.CreateCommandBuffer(builder._context.Ptr));
            if (builder._writer != null)
                builder._writer.Handle = handle;
            return new CommandBuffer(handle);
        }
    }
}