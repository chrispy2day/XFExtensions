using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using XFExtensions.Samples.Helpers;
using XFExtensions.Samples.ViewModels;

namespace XFExtensions.Samples.Views
{
    public partial class MediaPage : ContentPage
    {
        private MediaVM _viewModel;

        public MediaPage()
        {
            InitializeComponent();
            _viewModel = new MediaVM();
            BindingContext = _viewModel;
            SetupActionSheet();
        }

        private void SetupActionSheet()
        {
            actionSheet.Title = "Image Source:";
            actionSheet.CancelButton = new ActionSheetButtonInfo { ButtonText = "Cancel" };
            actionSheet.AdditionalButtons.Add(
                new ActionSheetButtonInfo 
                { 
                    ButtonText = "Camera", 
                    ButtonCommand = _viewModel.TakePhotoCommand 
                });
            actionSheet.AdditionalButtons.Add(
                new ActionSheetButtonInfo 
                { 
                    ButtonText = "Existing Photo", 
                    ButtonCommand = _viewModel.ChoosePhotoCommand 
                });
        }
    }
}
