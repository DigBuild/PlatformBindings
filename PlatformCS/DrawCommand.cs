using System;

namespace DigBuildPlatformCS
{
    public sealed class DrawCommand
    {
        
    }

    public readonly ref struct DrawCommandBuilder
    {

        public static implicit operator DrawCommand(DrawCommandBuilder builder) => throw new NotImplementedException();
    }
}