using System;

namespace DigBuildPlatformCS.Util
{
    public class PlatformException : Exception
    {
        internal PlatformException(string? message, Exception? innerException = null) : base(message, innerException)
        {
        }
    }

    public sealed class InvalidRenderContextException : PlatformException
    {
        internal InvalidRenderContextException() : base("Invalid render context.")
        {
        }
    }
}