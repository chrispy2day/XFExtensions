using System;
using MetaMediaPlugin.Abstractions;

namespace MetaMediaPlugin
{
    public class MetaMedia
    {
        static Lazy<IMediaService> Implementation = new Lazy<IMediaService>(() => CreateMediaService(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current settings to use
        /// </summary>
        public static IMediaService Current
        {
            get
            {
                var ret = Implementation.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        static IMediaService CreateMediaService()
        {
#if PORTABLE
            return null;
#else
            return new MediaImplementation();
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}
