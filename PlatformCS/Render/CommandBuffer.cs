using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AdvancedDLSupport;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
{
    [NativeSymbols("dbp_command_buffer_", SymbolTransformationMethod.Underscore)]
    internal interface ICommandBufferBindings
    {
        void Commit(IntPtr instance, IntPtr context, IntPtr format, IntPtr commands, uint commandCount);
    }

    /// <summary>
    /// A collection of GPU commands.
    /// </summary>
    public sealed class CommandBuffer
    {
        internal static readonly ICommandBufferBindings Bindings = NativeLib.Get<ICommandBufferBindings>();

        internal readonly NativeHandle Handle;
        internal bool Recording;

        internal CommandBuffer(NativeHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Begins recording a new set of commands for the buffer.
        /// </summary>
        /// <param name="context">The render context</param>
        /// <param name="format">The target framebuffer format</param>
        /// <param name="bufferPool">A buffer pool</param>
        /// <returns>The recorder</returns>
        public CommandBufferRecorder Record(RenderContext context, FramebufferFormat format, NativeBufferPool bufferPool)
        {
            if (Handle == null)
                throw new InvalidOperationException("Not initialized.");
            if (Recording)
                throw new AlreadyRecordingException();
            Recording = true;
            return new CommandBufferRecorder(this, format, context, bufferPool);
        }
    }

    /// <summary>
    /// A command buffer command recorder.
    /// </summary>
    public sealed class CommandBufferRecorder : IDisposable
    {
        private readonly CommandBuffer _parent;
        private readonly FramebufferFormat _format;
        private readonly IntPtr _contextPtr;
        private readonly PooledNativeBuffer<CommandBufferCmd> _commands;
        private readonly Dictionary<IBindingHandle, (IUniformBinding, uint)> _uniformBindings = new();
        private readonly Dictionary<ShaderSamplerHandle, TextureBinding> _textureBindings = new();
        private bool _committed;

        internal CommandBufferRecorder(
            CommandBuffer parent,
            FramebufferFormat format,
            RenderContext context,
            NativeBufferPool bufferPool
        )
        {
            _parent = parent;
            _format = format;
            _contextPtr = context.Ptr;
            _commands = bufferPool.Request<CommandBufferCmd>();
        }

        /// <summary>
        /// Updates the viewport and scissor to match the specified render target.
        /// </summary>
        /// <param name="renderTarget">The render target</param>
        public void SetViewportAndScissor(IRenderTarget renderTarget)
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _commands.Add(new CommandBufferCmd.SetViewportScissor(renderTarget.Handle));
        }

        /// <summary>
        /// Updates the viewport and scissor to the specified extents.
        /// </summary>
        /// <param name="extents">The extents</param>
        public void SetViewportAndScissor(Extents2D extents)
        {
            SetViewport(extents);
            SetScissor(extents);
        }

        /// <summary>
        /// Updates the viewport to the specified extents.
        /// </summary>
        /// <param name="extents">The extents</param>
        public void SetViewport(Extents2D extents)
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _commands.Add(new CommandBufferCmd.SetViewport(extents));
        }

        /// <summary>
        /// Updates the scissor to the specified extents.
        /// </summary>
        /// <param name="extents">The extents</param>
        public void SetScissor(Extents2D extents)
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _commands.Add(new CommandBufferCmd.SetScissor(extents));
        }

        /// <summary>
        /// Sets the active index for a uniform binding.
        /// </summary>
        /// <typeparam name="TUniform">The uniform type</typeparam>
        /// <param name="pipeline">The target pipeline</param>
        /// <param name="uniformBinding">The uniform binding</param>
        /// <param name="index">The index</param>
        public void Using<TUniform>(
            IRenderPipeline pipeline,
            UniformBinding<TUniform> uniformBinding,
            uint index
        ) where TUniform : unmanaged, IUniform<TUniform>
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();

            if (_uniformBindings.TryGetValue(uniformBinding.UniformHandle, out var current) && current.Item1 == uniformBinding && current.Item2 == index)
                return;
            _uniformBindings[uniformBinding.UniformHandle] = (uniformBinding, index);

            _commands.Add(new CommandBufferCmd.BindUniform(pipeline.Handle, uniformBinding.Handle, index));
        }

        /// <summary>
        /// Sets the active binding for a texture.
        /// </summary>
        /// <param name="pipeline">The pipline</param>
        /// <param name="binding">The binding</param>
        public void Using(
            IRenderPipeline pipeline,
            TextureBinding binding
        )
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();

            if (_textureBindings.TryGetValue(binding.SamplerHandle, out var currentBinding) && currentBinding == binding)
                return;
            _textureBindings[binding.SamplerHandle] = binding;

            _commands.Add(new CommandBufferCmd.BindTexture(
                pipeline.Handle,
                binding.Handle
            ));
        }

        /// <summary>
        /// Draws the geometry in the vertex buffer using the pipline.
        /// </summary>
        /// <typeparam name="TVertex">The vertex type</typeparam>
        /// <param name="pipeline">The pipeline</param>
        /// <param name="vertexBuffer">The vertex buffer</param>
        public void Draw<TVertex>(
            RenderPipeline<TVertex> pipeline,
            VertexBuffer<TVertex> vertexBuffer
        ) where TVertex : unmanaged
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _commands.Add(new CommandBufferCmd.Draw(pipeline.Handle, vertexBuffer.Handle, IntPtr.Zero));
        }

        /// <summary>
        /// Draws the instanced geometry in the vertex buffers using the pipeline.
        /// </summary>
        /// <typeparam name="TVertex">The vertex type</typeparam>
        /// <typeparam name="TInstance">The instance type</typeparam>
        /// <param name="pipeline">The pipeline</param>
        /// <param name="vertexBuffer">The vertex buffer</param>
        /// <param name="instanceBuffer">The instance buffer</param>
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
        
        /// <summary>
        /// Commits the commands to the GPU.
        /// </summary>
        public void Commit()
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            _committed = true;
            _parent.Recording = false;

            var unpooled = _commands.Unpooled;
            CommandBuffer.Bindings.Commit(_parent.Handle!, _contextPtr, _format.Handle, ((INativeBuffer<CommandBufferCmd>)unpooled).Ptr, unpooled.Count);
            _commands.Dispose();
        }

        void IDisposable.Dispose() => Commit();
    }

    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct CommandBufferCmd
    {
        [FieldOffset(0)] private readonly Type _type;

        [FieldOffset(sizeof(Type))] private readonly SetViewportScissor _setViewportScissor;
        [FieldOffset(sizeof(Type))] private readonly SetViewport _setViewport;
        [FieldOffset(sizeof(Type))] private readonly SetScissor _setScissor;
        [FieldOffset(sizeof(Type))] private readonly BindUniform _bindUniform;
        [FieldOffset(sizeof(Type))] private readonly BindTexture _bindTexture;
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

        private CommandBufferCmd(BindTexture bindTexture) : this()
        {
            _type = Type.BindTexture;
            _bindTexture = bindTexture;
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
        public static implicit operator CommandBufferCmd(BindTexture cmd) => new(cmd);
        public static implicit operator CommandBufferCmd(Draw cmd) => new(cmd);

        internal enum Type : ulong
        {
            SetViewportScissor,
            SetViewport,
            SetScissor,
            BindUniform,
            BindTexture,
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
            private readonly IntPtr _uniformBinding;
            private readonly uint _binding;

            internal BindUniform(IntPtr pipeline, IntPtr uniformBinding, uint binding)
            {
                _pipeline = pipeline;
                _uniformBinding = uniformBinding;
                _binding = binding;
            }
        }

        internal readonly struct BindTexture
        {
            private readonly IntPtr _pipeline;
            private readonly IntPtr _binding;

            internal BindTexture(IntPtr pipeline, IntPtr binding)
            {
                _pipeline = pipeline;
                _binding = binding;
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

    /// <summary>
    /// A command buffer builder.
    /// </summary>
    public readonly ref struct CommandBufferBuilder
    {
        private readonly RenderContext _context;

        internal CommandBufferBuilder(RenderContext context) : this()
        {
            _context = context;
        }

        public static implicit operator CommandBuffer(CommandBufferBuilder builder)
        {
            return new(
                new NativeHandle(
                    RenderContext.Bindings.CreateCommandBuffer(builder._context.Ptr)
                )
            );
        }
    }
}