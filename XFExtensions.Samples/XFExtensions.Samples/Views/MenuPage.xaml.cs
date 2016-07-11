using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using XFExtensions.Samples.Models;
using XFExtensions.Samples.ViewModels;

namespace XFExtensions.Samples.Views
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
            BindingContext = new MenuVM();
            pageList.ItemSelected += OnItemSelected;
        }

        void OnItemSelected (object sender, SelectedItemChangedEventArgs e)
        {
            var masterDetail = (MasterDetailPage)Application.Current.MainPage;
            var item = (SamplesMenuItem)e.SelectedItem;
            if (item == null)
                return;
                
            var detailPage = (Page)Activator.CreateInstance (item.TypeOfView);
            masterDetail.Detail = new NavigationPage(detailPage);
            pageList.SelectedItem = null;
            masterDetail.IsPresented = false;
        }
   }
}
