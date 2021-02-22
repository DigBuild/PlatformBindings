using DigBuildPlatformCS.Resource;
using DigBuildPlatformCS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DigBuildPlatformCS
{
    public abstract class Shader
    {
        internal readonly NativeHandle Handle;
        internal readonly ShaderType ShaderType;

        internal Shader(NativeHandle handle, ShaderType shaderType)
        {
            Handle = handle;
            ShaderType = shaderType;
        }
    }

    public sealed class VertexShader : Shader
    {
        internal VertexShader(NativeHandle handle) : base(handle, ShaderType.Vertex)
        {
        }
    }

    public sealed class FragmentShader : Shader
    {
        internal FragmentShader(NativeHandle handle) : base(handle, ShaderType.Fragment)
        {
        }
    }
    
    public interface IUniform<TUniform> where TUniform : unmanaged, IUniform<TUniform>
    {
    }

    internal interface IUniformHandle
    {
        internal Shader Shader { set; }
    }

    public sealed class UniformHandle<T> : IUniformHandle where T : unmanaged, IUniform<T>
    {
        internal Shader Shader { get; private set; } = null!;
        internal readonly uint Binding;

        internal UniformHandle(uint binding)
        {
            Binding = binding;
        }

        Shader IUniformHandle.Shader
        {
            set => Shader = value;
        }
    }

    internal readonly struct UniformMember
    {
        internal readonly NumericType Type;

        internal UniformMember(FieldInfo field)
        {
            Type = NumericTypeHelper.GetType(field.FieldType);
        }
    }

    internal readonly struct BindingData
    {
        private readonly uint _memberOffset, _memberCount, _size;

        internal BindingData(uint memberOffset, uint memberCount, uint size)
        {
            _memberOffset = memberOffset;
            _memberCount = memberCount;
            _size = size;
        }
    }

    internal enum ShaderType : byte
    {
        Vertex, Fragment
    }

    public readonly ref struct ShaderBuilder<TShader> where TShader : Shader
    {
        private sealed class Data
        {
            internal readonly IResource Resource;
            internal readonly ShaderType Type;
            internal readonly Func<NativeHandle, TShader> Factory;

            internal readonly List<BindingData> UniformBindings = new();
            internal readonly List<UniformMember> UniformMembers = new();
            internal readonly List<IUniformHandle> UniformHandles = new();

            public Data(IResource resource, ShaderType type, Func<NativeHandle, TShader> factory)
            {
                Resource = resource;
                Type = type;
                Factory = factory;
            }
        }

        private readonly RenderContext _ctx;
        private readonly Data _data;

        internal ShaderBuilder(RenderContext ctx, IResource resource, ShaderType type, Func<NativeHandle, TShader> factory)
        {
            _ctx = ctx;
            _data = new Data(resource, type, factory);
        }

        public ShaderBuilder<TShader> WithUniform<TUniform>(
            out UniformHandle<TUniform> handle
        ) where TUniform : unmanaged, IUniform<TUniform>
        {
            var members = typeof(TUniform).GetFields(BindingFlags.NonPublic)
                .Select(field => new UniformMember(field))
                .ToList();
            _data.UniformBindings.Add(new BindingData(
                (uint)_data.UniformMembers.Count,
                (uint)members.Count,
                (uint)Marshal.SizeOf<TUniform>()
            ));
            _data.UniformMembers.AddRange(members);
            _data.UniformHandles.Add(handle = new UniformHandle<TUniform>((uint) _data.UniformHandles.Count));
            return this;
        }

        public static unsafe implicit operator TShader(ShaderBuilder<TShader> builder)
        {
            var bindings = builder._data.UniformBindings.ToArray();
            var members = builder._data.UniformMembers.ToArray();

            var bytes = builder._data.Resource.ReadAllBytes();
            var span1 = new Span<byte>(bytes);
            var span2 = new Span<BindingData>(bindings);
            var span3 = new Span<UniformMember>(members);

            fixed (byte* p1 = &span1.GetPinnableReference())
            fixed (BindingData* p2 = &span2.GetPinnableReference())
            fixed (UniformMember* p3 = &span3.GetPinnableReference())
            {
                var shader = builder._data.Factory(
                    new NativeHandle(
                        RenderContext.Bindings.CreateShader(
                            builder._ctx.Ptr,
                            builder._data.Type,
                            new IntPtr(p1),
                            span1.Length,
                            new IntPtr(p2),
                            span2.Length,
                            new IntPtr(p3)
                        )
                    )
                );
                foreach (var handle in builder._data.UniformHandles)
                    handle.Shader = shader;
                return shader;
            }
        }
    }
}