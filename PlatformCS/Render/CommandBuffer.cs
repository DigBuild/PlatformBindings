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

    public sealed class CommandBuffer
    {
        internal static readonly ICommandBufferBindings Bindings = NativeLib.Get<ICommandBufferBindings>();

        internal readonly NativeHandle Handle;
        internal bool Recording;

        internal CommandBuffer(NativeHandle handle)
        {
            Handle = handle;
        }

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

    public sealed class CommandBufferRecorder : IDisposable
    {
        private readonly CommandBuffer _parent;
        private readonly FramebufferFormat _format;
        private readonly IntPtr _contextPtr;
        private readonly PooledNativeBuffer<CommandBufferCmd> _commands;
        private readonly Dictionary<IBindingHandle, (IUniformBuffer, uint)> _uniformBindings = new();
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

        public void Bind<TUniform>(
            IRenderPipeline pipeline,
            UniformHandle<TUniform> handle,
            UniformBuffer<TUniform> buffer
        ) where TUniform : unmanaged, IUniform<TUniform>
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();

            // if (_uniformBindings.TryGetValue(uniformBuffer.UniformHandle, out var current) && current.Item1 == uniformBuffer && current.Item2 == index)
            //     return;
            // _uniformBindings[uniformBuffer.UniformHandle] = (uniformBuffer, index);

            _commands.Add(new CommandBufferCmd.BindUniform(pipeline.Handle, buffer.Handle, handle.Shader.Handle, handle.Binding));
        }

        public void Bind(
            IRenderPipeline pipeline,
            ShaderSamplerHandle handle,
            TextureSampler sampler,
            Texture texture
        )
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            
            // if (_textureBindings.TryGetValue(binding.SamplerHandle, out var currentBinding) && currentBinding == binding)
            //     return;
            // _textureBindings[binding.SamplerHandle] = binding;

            _commands.Add(new CommandBufferCmd.BindTexture(pipeline.Handle, sampler.Handle, texture.Handle, handle.Shader.Handle, handle.Binding));
        }

        public void Using<TUniform>(
            IRenderPipeline pipeline,
            UniformHandle<TUniform> handle,
            uint index
        ) where TUniform : unmanaged, IUniform<TUniform>
        {
            if (_committed)
                throw new RecordingAlreadyCommittedException();
            
            _commands.Add(new CommandBufferCmd.UseUniform(pipeline.Handle, handle.Shader.Handle, handle.Binding, index));
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
        [FieldOffset(sizeof(Type))] private readonly UseUniform _useUniform;
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

        private CommandBufferCmd(UseUniform useUniform) : this()
        {
            _type = Type.UseUniform;
            _useUniform = useUniform;
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
        public static implicit operator CommandBufferCmd(UseUniform cmd) => new(cmd);
        public static implicit operator CommandBufferCmd(Draw cmd) => new(cmd);

        internal enum Type : ulong
        {
            SetViewportScissor,
            SetViewport,
            SetScissor,
            BindUniform,
            BindTexture,
            UseUniform,
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
            private readonly IntPtr _shader;
            private readonly uint _binding;

            internal BindUniform(IntPtr pipeline, IntPtr uniformBuffer, IntPtr shader, uint binding)
            {
                _pipeline = pipeline;
                _uniformBuffer = uniformBuffer;
                _shader = shader;
                _binding = binding;
            }
        }

        internal readonly struct BindTexture
        {
            private readonly IntPtr _pipeline;
            private readonly IntPtr _sampler;
            private readonly IntPtr _texture;
            private readonly IntPtr _shader;
            private readonly uint _binding;

            internal BindTexture(IntPtr pipeline, IntPtr sampler, IntPtr texture, IntPtr shader, uint binding)
            {
                _pipeline = pipeline;
                _sampler = sampler;
                _texture = texture;
                _shader = shader;
                _binding = binding;
            }
        }

        internal readonly struct UseUniform
        {
            private readonly IntPtr _pipeline;
            private readonly IntPtr _shader;
            private readonly uint _binding;
            private readonly uint _index;

            internal UseUniform(IntPtr pipeline, IntPtr shader, uint binding, uint index)
            {
                _pipeline = pipeline;
                _shader = shader;
                _binding = binding;
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