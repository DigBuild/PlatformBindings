using System;

namespace DigBuild.Platform.Util
{
    /// <summary>
    /// A base class for all platform exceptions.
    /// </summary>
    public class PlatformException : Exception
    {
        internal PlatformException(string? message, Exception? innerException = null) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Fired when a handle to a native resource is invalid.
    /// </summary>
    public sealed class InvalidHandleException : PlatformException
    {
        internal InvalidHandleException() : base("Invalid handle.")
        {
        }
    }

    /// <summary>
    /// Fired when a custom resource type has no loader.
    /// </summary>
    public sealed class ResourceLoaderMissingException : PlatformException
    {
        internal ResourceLoaderMissingException(Type type) :
            base($"No resource loader provided for custom resource type {type.Name}.")
        {
        }
    }

    /// <summary>
    /// Fired when a draw command begins recording while it is already recording.
    /// </summary>
    public sealed class AlreadyRecordingException : PlatformException
    {
        internal AlreadyRecordingException() :
            base("Draw command is already being recorded to.")
        {
        }
    }

    /// <summary>
    /// Fired when an action is recorded to a committed draw command.
    /// </summary>
    public sealed class RecordingAlreadyCommittedException : PlatformException
    {
        internal RecordingAlreadyCommittedException() :
            base("Draw command recording has already been committed and cannot be written to.")
        {
        }
    }

    /// <summary>
    /// Fired when a shader binding is added multiple times.
    /// </summary>
    public sealed class ShaderBindingAlreadyInUseException : PlatformException
    {
        internal ShaderBindingAlreadyInUseException(uint binding) :
            base($"The specified shader binding is already in use: {binding}")
        {
        }
    }
}