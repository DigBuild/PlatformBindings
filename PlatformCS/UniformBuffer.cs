using System;
using DigBuildPlatformCS.Util;

namespace DigBuildPlatformCS
{
    internal static class UniformBufferType<T> where T : class, IUniform<T>
    {
        internal static T Activate(
            IntPtr data
        ) => throw new NotImplementedException();
    }

    public class UniformBuffer<T> where T : class, IUniform<T>
    {
        private NativeBuffer _buffer;

        public T Push() => throw new NotImplementedException();

        public void Upload() => throw new NotImplementedException();

        public T this[uint index] => throw new NotImplementedException();
    }
}