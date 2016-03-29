using Xamarin.Forms;
using XFExtensions.Samples.Views;

namespace XFExtensions.Samples
{
    public class App : Application
    {
        public App()
        {
            MainPage = new CodedListPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
