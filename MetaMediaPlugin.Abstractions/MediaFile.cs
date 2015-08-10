using System;
using System.IO;

namespace MetaMediaPlugin.Abstractions
{
    public class MediaFile
    {
        public byte[] MediaBytes { get; set; }
        public string FileName { get; set; }
    }
}
