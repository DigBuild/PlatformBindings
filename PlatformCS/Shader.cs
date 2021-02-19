using DigBuildPlatformCS.Resource;
using DigBuildPlatformCS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DigBuildPlatformCS
{
    public abstract class Shader
    {
        internal readonly NativeHandle Handle;

        internal Shader(NativeHandle handle)
        {
            Handle = handle;
        }
    }

    public sealed class VertexShader : Shader
    {
        internal VertexShader(NativeHandle handle) : base(handle)
        {
        }
    }

    public sealed class VertexShader<TUniform> : Shader where TUniform : IUniform<TUniform>
    {
        internal VertexShader(NativeHandle handle) : base(handle)
        {
        }
    }

    public sealed class FragmentShader : Shader
    {
        internal FragmentShader(NativeHandle handle) : base(handle)
        {
        }
    }

    public sealed class FragmentShader<TUniform> : Shader where TUniform : IUniform<TUniform>
    {
        internal FragmentShader(NativeHandle handle) : base(handle)
        {
        }
    }

    public interface IUniform<TUniform> where TUniform : IUniform<TUniform>
    {
    }

    internal interface IUniformHandle
    {
        internal IRenderPipeline Pipeline { get; set; }
        internal ShaderType ShaderType { get; }
    }

    public sealed class UniformHandle<TUniform> : IUniformHandle where TUniform : IUniform<TUniform>
    {
        private readonly ShaderType _shaderType;

        IRenderPipeline IUniformHandle.Pipeline { get; set; } = null!;
        ShaderType IUniformHandle.ShaderType => _shaderType;

        internal UniformHandle(ShaderType shaderType)
        {
            _shaderType = shaderType;
        }
    }

    public sealed class UniformAttribute : Attribute
    {
        internal readonly uint Binding;
        internal readonly uint Order;

        public UniformAttribute(uint binding, [CallerLineNumber] int order = 0)
        {
            Binding = binding;
            Order = (uint) order;
        }
    }

    internal readonly struct UniformProperty
    {
        internal readonly NumericType Type;

        internal UniformProperty(PropertyInfo property)
        {
            Type = NumericTypeHelper.GetType(property.PropertyType);
        }
    }

    internal sealed class UniformDescriptor
    {
        internal readonly IReadOnlyDictionary<uint, UniformProperty[]> Bindings;
        internal readonly int TotalPropertyCount;

        internal UniformDescriptor(IReadOnlyDictionary<uint, UniformProperty[]> bindings)
        {
            Bindings = bindings;
            TotalPropertyCount = bindings.Values.Sum(properties => properties.Length);
        }
    }

    internal static class UniformDescriptor<TUniform> where TUniform : IUniform<TUniform>
    {
        internal static readonly UniformDescriptor Instance;

        static UniformDescriptor()
        {
            var type = typeof(TUniform);
            if (!type.IsInterface)
                throw new UniformNotInterfaceException(type);

            Dictionary<uint, SortedSet<(PropertyInfo, UniformAttribute)>> bindings = new();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<UniformAttribute>();
                if (attribute == null)
                    throw new UntaggedUniformPropertyException(type, property.Name);

                if (!bindings.TryGetValue(attribute.Binding, out var elements))
                    elements = bindings[attribute.Binding]
                        = new SortedSet<(PropertyInfo, UniformAttribute)>(new Comparer());

                elements.Add((property, attribute));
            }

            Dictionary<uint, UniformProperty[]> finalBindings = new();
            foreach (var (binding, elements) in bindings)
            {
                var finalElements = new UniformProperty[elements.Count];
                var i = 0;
                foreach (var (property, _) in elements)
                    finalElements[i++] = new UniformProperty(property);
                finalBindings[binding] = finalElements;
            }

            Instance = new UniformDescriptor(finalBindings);
        }

        private sealed class Comparer : IComparer<(PropertyInfo, UniformAttribute)>
        {
            public int Compare((PropertyInfo, UniformAttribute) a, (PropertyInfo, UniformAttribute) b)
            {
                return a.Item2.Order.CompareTo(b.Item2.Order);
            }
        }
    }

    internal enum ShaderType : byte
    {
        Vertex, Fragment
    }

    public readonly ref struct ShaderBuilder<TShader> where TShader : Shader
    {
        private readonly RenderContext _ctx;
        private readonly IResource _resource;
        private readonly ShaderType _type;
        private readonly UniformDescriptor? _uniformDescriptor;
        private readonly Func<NativeHandle, TShader> _factory;

        internal ShaderBuilder(RenderContext ctx, IResource resource, ShaderType type,
            UniformDescriptor? uniformDescriptor, Func<NativeHandle, TShader> factory)
        {
            _ctx = ctx;
            _resource = resource;
            _type = type;
            _uniformDescriptor = uniformDescriptor;
            _factory = factory;
        }

        public static unsafe implicit operator TShader(ShaderBuilder<TShader> builder)
        {
            var properties = new UniformProperty[builder._uniformDescriptor?.TotalPropertyCount ?? 0];
            var offset = 0u;
            List<BindingData> bindings = new();
            if (builder._uniformDescriptor != null)
            {
                foreach (var (binding, props) in builder._uniformDescriptor.Bindings)
                {
                    props.CopyTo(properties, offset);
                    bindings.Add(new BindingData(binding, offset, (uint) properties.Length));
                    offset += (uint) props.Length;
                }
            }

            var bytes = builder._resource.ReadAllBytes();
            var span1 = new Span<byte>(bytes);
            var span2 = new Span<BindingData>(bindings.ToArray());
            var span3 = new Span<UniformProperty>(properties);

            fixed (byte* p1 = &span1.GetPinnableReference())
            fixed (BindingData* p2 = &span2.GetPinnableReference())
            fixed (UniformProperty* p3 = &span3.GetPinnableReference())
            {
                return builder._factory(
                    new NativeHandle(
                        RenderContext.Bindings.CreateShader(
                            builder._ctx.Ptr,
                            builder._type,
                            new IntPtr(p1),
                            span1.Length,
                            new IntPtr(p2),
                            span2.Length,
                            new IntPtr(p3)
                        )
                    )
                );
            }
        }

        private struct BindingData
        {
            public uint Id, Offset, PropertyCount;

            internal BindingData(uint id, uint offset, uint propertyCount)
            {
                Id = id;
                Offset = offset;
                PropertyCount = propertyCount;
            }
        }
    }
}