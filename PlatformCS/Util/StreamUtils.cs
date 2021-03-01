using System.IO;

namespace DigBuild.Platform.Util
{
    public static class StreamUtils
    {
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