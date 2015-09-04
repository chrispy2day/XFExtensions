using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MetaMediaPlugin.Abstractions;

namespace MetaMediaPlugin
{
    internal class MediaPickedEventArgs
        : EventArgs
    {
        public MediaPickedEventArgs(int id, Exception error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            RequestId = id;
            Error = error;
        }

        public MediaPickedEventArgs(int id, bool cancelled, MediaFile media = null)
        {
            RequestId = id;
            Cancelled = cancelled;
            if (!cancelled && media == null)
                throw new ArgumentNullException("media");

            Media = media;
        }

        public int RequestId
        {
            get;
            private set;
        }

        public bool Cancelled
        {
            get;
            private set;
        }

        public bool Failed
        {
            get;
            private set;
        }

        public Exception Error
        {
            get;
            private set;
        }

        public MediaFile Media
        {
            get;
            private set;
        }
    }
}