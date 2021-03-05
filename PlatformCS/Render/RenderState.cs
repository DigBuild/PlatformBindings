using System;

namespace DigBuild.Platform.Render
{
    internal readonly struct RenderState
    {
        internal readonly Topology Topology;
        internal readonly RasterMode RasterMode;
        internal readonly bool DiscardRaster;
        internal readonly MaybeDynamic<float> LineWidth;
        internal readonly MaybeDynamic<DepthBias> DepthBias;
        internal readonly MaybeDynamic<DepthTest> DepthTest;
        internal readonly MaybeDynamic<StencilTest> StencilTest;
        internal readonly MaybeDynamic<CullingMode> CullingMode;
        internal readonly MaybeDynamic<FrontFace> FrontFace;

        internal RenderState(
            Topology topology,
            RasterMode rasterMode,
            bool discardRaster,
            MaybeDynamic<float>? lineWidth,
            MaybeDynamic<DepthBias>? depthBias,
            MaybeDynamic<DepthTest>? depthTest,
            MaybeDynamic<StencilTest>? stencilTest,
            MaybeDynamic<CullingMode>? cullingMode,
            MaybeDynamic<FrontFace>? frontFace
        )
        {
            Topology = topology;
            RasterMode = rasterMode;
            DiscardRaster = discardRaster;
            LineWidth = lineWidth ?? 1.0f;
            DepthBias = depthBias ?? Render.DepthBias.Default;
            DepthTest = depthTest ?? Render.DepthTest.Default;
            StencilTest = stencilTest ?? Render.StencilTest.Default;
            CullingMode = cullingMode ?? Render.CullingMode.Back;
            FrontFace = frontFace ?? Render.FrontFace.Clockwise;
        }
    }
    
    public readonly struct MaybeDynamic<T> where T : unmanaged
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

    public enum Topology : byte
    {
        Points,
        Lines, LineStrips,
        Triangles, TriangleStrips, TriangleFans
    }

    public enum RasterMode : byte
    {
        Fill,
        Line,
        Point
    }

    public enum CullingMode : byte
    {
        Front, Back, FrontAndBack, None
    }

    public enum FrontFace : byte
    {
        Clockwise,
        CounterClockwise
    }

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

    public readonly struct DepthBias
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

    public readonly struct DepthTest
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

    public enum CompareOperation : byte
    {
        Never,
        Less, LessOrEqual,
        Equal, NotEqual,
        GreaterOrEqual, Greater,
        Always
    }

    public readonly struct StencilTest
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

    public readonly struct StencilFaceOperation
    {
        public static readonly StencilFaceOperation Default = new(
            StencilOperation.Keep,
            StencilOperation.Keep,
            StencilOperation.Keep,
            CompareOperation.Never,
            0, 0, 0
        );

        public readonly StencilOperation StencilFailOperation;
        public readonly StencilOperation DepthFailOperation;
        public readonly StencilOperation SuccessOperation;
        public readonly CompareOperation CompareOperation;
        public readonly uint CompareMask;
        public readonly uint WriteMask;
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

    public enum BlendOperation : byte
    {
        Add,
        Subtract,
        ReverseSubtract,
        Min,
        Max,
    }

    [Flags]
    public enum ColorComponent : byte
    {
        Red = 1 << 0,
        Green = 1 << 1,
        Blue = 1 << 2,
        Alpha = 1 << 3
    }
}