using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MetaMediaPlugin;
using Xamarin.Forms;

namespace XFExtensions.Samples.ViewModels
{
    public class MediaVM
    {
        public ICommand ChoosePhotoCommand { get; private set; }

        public MediaVM()
        {
            ChoosePhotoCommand = new Command(
                async (_) => await MetaMedia.Current.PickPhotoAsync(), 
                o => MetaMedia.Current.IsPickPhotoSupported);
        }
    }
}
