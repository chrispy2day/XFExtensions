using System.Collections.Generic;
using XFExtensions.Samples.Models;
using XFExtensions.Samples.Views;

namespace XFExtensions.Samples.ViewModels
{
    public class MenuVM
    {
        public List<SamplesMenuGroup> Menu { get; private set; }
        
        public MenuVM()
        {
            // create the menu
            Menu = new List<SamplesMenuGroup>
            {
                new SamplesMenuGroup("Simple List")
                {
                    new SamplesMenuItem
                    {
                        Name = "Coded Simple List",
                        Description = "Basic setup in code",
                        TypeOfView = typeof(CodedListPage)
                    },
                    new SamplesMenuItem
                    {
                        Name = "XAML Simple List",
                        Description = "Basic setup in XAML",
                        TypeOfView = typeof(SimpleListsPage)
                    }
                },
                new SamplesMenuGroup("Zoom Image")
                {
                    new SamplesMenuItem
                    {
                        Name = "Zoom Example",
                        Description = "Setup & use ZoomImage",
                        TypeOfView = typeof(ZoomPage)
                    }
                },
                new SamplesMenuGroup("MetaMedia Plugin")
                {
                    new SamplesMenuItem
                    {
                        Name = "MetaMedia Example",
                        Description = "Get & take photos",
                        TypeOfView = typeof(MediaPage)
                    }
                }
            };
        }
    }
}
