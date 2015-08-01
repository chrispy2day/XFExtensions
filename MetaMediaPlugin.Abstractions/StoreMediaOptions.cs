using System;

namespace MetaMediaPlugin.Abstractions
{
    /// <summary>
    /// Media Options
    /// </summary>
    public class StoreMediaOptions
    {
        /// <summary>
        /// 
        /// </summary>
        protected StoreMediaOptions()
        {
        }

        /// <summary>
        /// Directory name
        /// </summary>
        public string Directory
        {
            get;
            set;
        }

        /// <summary>
        /// File name
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        public bool SaveToCameraRoll
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Camera device
    /// </summary>
    public enum CameraDevice
    {
        /// <summary>
        /// Back of device
        /// </summary>
        Rear,
        /// <summary>
        /// Front facing of device
        /// </summary>
        Front
    }
    /// <summary>
    /// 
    /// </summary>
    public class StoreCameraMediaOptions
      : StoreMediaOptions
    {
        /// <summary>
        /// Default camera
        /// </summary>
        public CameraDevice DefaultCamera
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Video quality
    /// </summary>
    public enum VideoQuality
    {
        /// <summary>
        /// Low
        /// </summary>
        Low = 0,
        /// <summary>
        /// Medium
        /// </summary>
        Medium = 1,
        /// <summary>
        /// High
        /// </summary>
        High = 2,
    }

    /// <summary>
    /// Store Video options
    /// </summary>
    public class StoreVideoOptions
      : StoreCameraMediaOptions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public StoreVideoOptions()
        {
            Quality = VideoQuality.High;
            DesiredLength = TimeSpan.FromMinutes(10);
        }

        /// <summary>
        /// Desired Length
        /// </summary>
        public TimeSpan DesiredLength
        {
            get;
            set;
        }

        /// <summary>
        /// Desired Quality
        /// </summary>
        public VideoQuality Quality
        {
            get;
            set;
        }
    }
}
