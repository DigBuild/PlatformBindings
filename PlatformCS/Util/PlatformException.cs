using System;

namespace DigBuildPlatformCS.Util
{
    public class PlatformException : Exception
    {
        internal PlatformException(string? message, Exception? innerException = null) : base(message, innerException)
        {
        }
    }

    public sealed class InvalidHandleException : PlatformException
    {
        internal InvalidHandleException() : base("Invalid handle.")
        {
        }
    }

    public sealed class ResourceLoaderMissingException : PlatformException
    {
        internal ResourceLoaderMissingException(Type type) :
            base($"No resource loader provided for custom resource type {type.Name}.")
        {
        }
    }

    public sealed class AlreadyRecordingException : PlatformException
    {
        internal AlreadyRecordingException() :
            base("Draw command is already being recorded to.")
        {
        }
    }

    public sealed class RecordingAlreadyCommittedException : PlatformException
    {
        internal RecordingAlreadyCommittedException() :
            base("Draw command recording has already been committed and cannot be written to.")
        {
        }
    }

    public sealed class ShaderBindingAlreadyInUseException : PlatformException
    {
        internal ShaderBindingAlreadyInUseException(uint binding) :
            base($"The specified shader binding is already in use: {binding}")
        {
        }
    }
}