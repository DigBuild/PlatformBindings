﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DigBuild.Platform.Resource;
using DigBuild.Platform.Util;

namespace DigBuild.Platform.Render
{
    /// <summary>
    /// A shader.
    /// </summary>
    public abstract class ShaderBase
    {
        internal readonly NativeHandle Handle;
        internal readonly ShaderType ShaderType;

        internal ShaderBase(NativeHandle handle, ShaderType shaderType)
        {
            Handle = handle;
            ShaderType = shaderType;
        }
    }

    /// <summary>
    /// A vertex shader.
    /// </summary>
    public sealed class VertexShader : ShaderBase
    {
        internal VertexShader(NativeHandle handle) : base(handle, ShaderType.Vertex)
        {
        }
    }

    /// <summary>
    /// A fragment shader.
    /// </summary>
    public sealed class FragmentShader : ShaderBase
    {
        internal FragmentShader(NativeHandle handle) : base(handle, ShaderType.Fragment)
        {
        }
    }

    /// <summary>
    /// A shader uniform.
    /// </summary>
    /// <typeparam name="TUniform">The uniform type</typeparam>
    public interface IUniform<TUniform> where TUniform : unmanaged, IUniform<TUniform>
    {
    }

    internal interface IBindingHandle
    {
        internal ShaderBase Shader { set; }
    }

    /// <summary>
    /// A uniform handle.
    /// </summary>
    /// <typeparam name="T">The uniform type</typeparam>
    public sealed class UniformHandle<T> : IBindingHandle where T : unmanaged, IUniform<T>
    {
        internal ShaderBase Shader { get; private set; } = null!;
        internal readonly uint Binding;

        internal UniformHandle(uint binding)
        {
            Binding = binding;
        }

        ShaderBase IBindingHandle.Shader
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

    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct BindingData
    {
        [FieldOffset(0)]
        private readonly Type _type;

        [FieldOffset(sizeof(Type))]
        private readonly Uniform _uniform;
        [FieldOffset(sizeof(Type))]
        private readonly Sampler _sampler;

        private BindingData(Uniform uniform) : this()
        {
            _type = Type.Uniform;
            _uniform = uniform;
        }

        private BindingData(Sampler sampler) : this()
        {
            _type = Type.Sampler;
            _sampler = sampler;
        }

        public static implicit operator BindingData(Uniform uniform) => new(uniform);
        public static implicit operator BindingData(Sampler sampler) => new(sampler);

        internal enum Type : ulong
        {
            Uniform,
            Sampler
        }

        internal readonly struct Uniform
        {
            private readonly uint _memberOffset, _memberCount, _size;

            internal Uniform(uint memberOffset, uint memberCount, uint size)
            {
                _memberOffset = memberOffset;
                _memberCount = memberCount;
                _size = size;
            }
        }

        internal readonly struct Sampler
        {
        }
    }

    /// <summary>
    /// A shader sampler handle.
    /// </summary>
    public sealed class ShaderSamplerHandle : IBindingHandle
    {
        internal ShaderBase Shader { get; private set; } = null!;
        internal readonly uint Binding;

        internal ShaderSamplerHandle(uint binding)
        {
            Binding = binding;
        }

        ShaderBase IBindingHandle.Shader
        {
            set => Shader = value;
        }
    }

    internal enum ShaderType : byte
    {
        Vertex, Fragment
    }

    /// <summary>
    /// A shader builder.
    /// </summary>
    /// <typeparam name="TShader">The shader type</typeparam>
    public readonly ref struct ShaderBuilder<TShader> where TShader : ShaderBase
    {
        private sealed class Data
        {
            internal readonly IResource Resource;
            internal readonly ShaderType Type;
            internal readonly Func<NativeHandle, TShader> Factory;
            
            internal readonly List<BindingData> Bindings = new();
            internal readonly List<IBindingHandle> BindingHandles = new();
            internal readonly List<UniformMember> UniformMembers = new();

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

        /// <summary>
        /// Adds a new uniform to the shader.
        /// </summary>
        /// <typeparam name="TUniform">The uniform type</typeparam>
        /// <param name="handle">The handle</param>
        /// <returns>The builder</returns>
        public ShaderBuilder<TShader> WithUniform<TUniform>(
            out UniformHandle<TUniform> handle
        ) where TUniform : unmanaged, IUniform<TUniform>
        {
            var members = typeof(TUniform).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Select(field => new UniformMember(field))
                .ToList();

            _data.Bindings.Add(new BindingData.Uniform(
                (uint)_data.UniformMembers.Count,
                (uint)members.Count,
                (uint)Marshal.SizeOf<TUniform>()
            ));
            _data.BindingHandles.Add(handle = new UniformHandle<TUniform>((uint)_data.BindingHandles.Count));
            _data.UniformMembers.AddRange(members);
            return this;
        }

        /// <summary>
        /// Adds a new sampler to the shader.
        /// </summary>
        /// <param name="handle">The handle</param>
        /// <returns>The builder</returns>
        public ShaderBuilder<TShader> WithSampler(
            out ShaderSamplerHandle handle
        )
        {
            _data.Bindings.Add(new BindingData.Sampler());
            _data.BindingHandles.Add(handle = new ShaderSamplerHandle((uint)_data.BindingHandles.Count));
            return this;
        }

        public static unsafe implicit operator TShader(ShaderBuilder<TShader> builder)
        {
            var bindings = builder._data.Bindings.ToArray();
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
                foreach (var handle in builder._data.BindingHandles)
                    handle.Shader = shader;
                return shader;
            }
        }
    }
}