using System.IO;

namespace DigBuild.Platform.Util
{
    /// <summary>
    /// Utilities to deal with streams.
    /// </summary>
    public static class StreamUtils
    {
        /// <summary>
        /// Collects the contents of a stream into a byte array.
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <returns>The byte array</returns>
        public static byte[] ReadAllBytes(Stream stream)
        {
            var index = 0;
            var count = (int)stream!.Length;
            byte[] bytes = new byte[count];
            while (count > 0)
            {
                var n = stream.Read(bytes, index, count);
                if (n == 0)
                    throw new EndOfStreamException();
                index += n;
                count -= n;
            }
            return bytes;
        }
    }
}