using System;
using System.IO;

namespace MetaMediaPlugin.Abstractions
{
    public static class StreamHelpers
    {
        public static byte[] ReadFully(System.IO.Stream input)
        {
            byte[] buffer = new byte[16*1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

    }
}

