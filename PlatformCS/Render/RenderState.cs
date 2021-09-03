using System;

namespace DigBuild.Platform.Render
{
    internal readonly struct MaybeDynamic<T> where T : unmanaged
    {
        public static readonly MaybeDynamic<T> Dynamic = default;
        
        public readonly bool HasValue;
        public readonly T Value;

        private MaybeDynamic(T value)
        {
            HasValue = true;
            Value = value;
        }

        public static implicit operator MaybeDynamic<T>(T value) => new(value);
    }

    /// <summary>
    /// A geometry topology.
    /// </summary>
    public enum Topology : byte
    {
        Points,
        Lines, LineStrips,
        Triangles, TriangleStrips, TriangleFans
    }

    /// <summary>
    /// A raster mode.
    /// </summary>
    public enum RasterMode : byte
    {
        Fill,
        Line,
        Point
    }

    /// <summary>
    /// A geometry culling mode.
    /// </summary>
    public enum CullingMode : byte
    {
        Front, Back, FrontAndBack, None
    }

    /// <summary>
    /// A geometry front face vertex ordering.
    /// </summary>
    public enum FrontFace : byte
    {
        Clockwise,
        CounterClockwise
    }

    /// <summary>
    /// A set of 2D extents.
    /// </summary>
    public readonly struct Extents2D
    {
        public readonly uint X, Y;
        public readonly uint Width, Height;

        public Extents2D(uint x, uint y, uint width, uint height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    internal readonly struct DepthBias
    {
        public static readonly DepthBias Default = new(false, 0.0f, 0.0f, 0.0f);

        public readonly byte Enabled;
        public readonly float Constant;
        public readonly float Clamp;
        public readonly float Slope;

        public DepthBias(bool enabled, float constant, float clamp, float slope)
        {
            Enabled = (byte) (enabled ? 1 : 0);
            Constant = constant;
            Clamp = clamp;
            Slope = slope;
        }
    }

    internal readonly struct DepthTest
    {
        public static readonly DepthTest Default = new(false, CompareOperation.Always, false);

        public readonly byte Enabled;
        public readonly CompareOperation Comparison;
        public readonly byte Write;

        public DepthTest(bool enabled, CompareOperation comparison, bool write)
        {
            Enabled = (byte) (enabled ? 1 : 0);
            Comparison = comparison;
            Write = (byte) (write ? 1 : 0);
        }
    }

    /// <summary>
    /// A comparison operation.
    /// </summary>
    public enum CompareOperation : byte
    {
        Never,
        Less, LessOrEqual,
        Equal, NotEqual,
        GreaterOrEqual, Greater,
        Always
    }

    internal readonly struct StencilTest
    {
        public static readonly StencilTest Default = new(false, StencilFaceOperation.Default);

        public readonly byte Enabled;
        public readonly StencilFaceOperation Front;
        public readonly StencilFaceOperation Back;

        public StencilTest(bool enabled, StencilFaceOperation front, StencilFaceOperation back)
        {
            Enabled = (byte) (enabled ? 1 : 0);
            Front = front;
            Back = back;
        }

        public StencilTest(bool enabled, StencilFaceOperation operation) :
            this(enabled, operation, operation)
        {
        }
    }

    /// <summary>
    /// A stencil face operation.
    /// </summary>
    public readonly struct StencilFaceOperation
    {
        public static readonly StencilFaceOperation Default = new(
            StencilOperation.Keep,
            StencilOperation.Keep,
            StencilOperation.Keep,
            CompareOperation.Never,
            0, 0, 0
        );

        /// <summary>
        /// The fail operation.
        /// </summary>
        public readonly StencilOperation StencilFailOperation;
        /// <summary>
        /// The depth fail operation.
        /// </summary>
        public readonly StencilOperation DepthFailOperation;
        /// <summary>
        /// The success operation.
        /// </summary>
        public readonly StencilOperation SuccessOperation;
        /// <summary>
        /// The comparison.
        /// </summary>
        public readonly CompareOperation CompareOperation;
        /// <summary>
        /// The comparison mask.
        /// </summary>
        public readonly uint CompareMask;
        /// <summary>
        /// The write mask.
        /// </summary>
        public readonly uint WriteMask;
        /// <summary>
        /// The value.
        /// </summary>
        public readonly uint Value;

        public StencilFaceOperation(
            StencilOperation stencilFailOperation,
            StencilOperation depthFailOperation,
            StencilOperation successOperation,
            CompareOperation compareOperation,
            uint compareMask,
            uint writeMask,
            uint value
        )
        {
            StencilFailOperation = stencilFailOperation;
            DepthFailOperation = depthFailOperation;
            SuccessOperation = successOperation;
            CompareOperation = compareOperation;
            CompareMask = compareMask;
            WriteMask = writeMask;
            Value = value;
        }
    }

    /// <summary>
    /// A stencil operation.
    /// </summary>
    public enum StencilOperation : byte
    {
        Zero,
        Keep,
        Replace,
        Invert,
        IncrementAndClamp,
        DecrementAndClamp,
        IncrementAndWrap,
        DecrementAndWrap
    }
    
    /// <summary>
    /// A blend factor.
    /// </summary>
    public enum BlendFactor : byte
    {
        Zero, One,
        SrcColor, OneMinusSrcColor,
        SrcAlpha, OneMinusSrcAlpha,
        DstColor, OneMinusDstColor,
        DstAlpha, OneMinusDstAlpha,
        CstColor, OneMinusCstColor,
        CstAlpha, OneMinusCstAlpha,
    }

    /// <summary>
    /// A blend operation.
    /// </summary>
    public enum BlendOperation : byte
    {
        Add,
        Subtract,
        ReverseSubtract,
        Min,
        Max,
    }

    /// <summary>
    /// A color component.
    /// </summary>
    [Flags]
    public enum ColorComponent : byte
    {
        Red = 1 << 0,
        Green = 1 << 1,
        Blue = 1 << 2,
        Alpha = 1 << 3
    }
}