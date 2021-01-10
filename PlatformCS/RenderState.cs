using System;

namespace DigBuildPlatformCS
{
    public readonly struct RenderState
    {
        public readonly Topology Topology;
        public readonly RasterMode RasterMode;
        public readonly CullingMode CullingMode;
        public readonly FrontFace FrontFace;
        public readonly DepthBiasFactors? DepthBiasFactors;
        public readonly float LineWidth;
        public readonly Extents2D? Viewport;
        public readonly Extents2D? Scissor;
        public readonly DepthTest? DepthTest;
        public readonly StencilTest? StencilTest;

    }

    public readonly struct DynamicRenderState
    {
        public readonly Extents2D? Scissor;
        public readonly float? LineWidth;
        public readonly DepthBiasFactors? DepthBiasFactors;
        // Depth bounds?
        public readonly uint? CompareMask;
        public readonly uint? WriteMask;
        public readonly uint? Reference;
        public readonly CullingMode? CullingMode;
        public readonly FrontFace? FrontFace;

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

    public readonly struct DepthBiasFactors
    {
        public readonly float Constant;
        public readonly float Slope;

        public DepthBiasFactors(float constant, float slope)
        {
            Constant = constant;
            Slope = slope;
        }
    }

    public readonly struct DepthTest
    {
        public readonly CompareOperation Comparison;
        public readonly bool Write;
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
        public readonly StencilFaceOperation Front;
        public readonly StencilFaceOperation Back;
    }

    public readonly struct StencilFaceOperation
    {
        public readonly StencilOperation StencilFailOperation;
        public readonly StencilOperation DepthFailOperation;
        public readonly StencilOperation SuccessOperation;
        public readonly CompareOperation CompareOperation;
        public readonly uint CompareMask;
        public readonly uint WriteMask;
        public readonly uint Value;
    }

    public enum StencilOperation
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