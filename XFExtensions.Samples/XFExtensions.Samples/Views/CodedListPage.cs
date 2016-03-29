using System;

using Xamarin.Forms;
using XFExtensions.Controls.Abstractions;

namespace XFExtensions.Samples
{
    public class CodedListPage : ContentPage
    {
        public CodedListPage()
        {
            // create the VM and assign the page binding
            var vm = new SimpleListVM();
            this.BindingContext = vm;

            // setup the simple list
            var list = new SimpleList
            {
                HorizontalOptions = LayoutOptions.FillAndExpand, 
                VerticalOptions = LayoutOptions.FillAndExpand, 
                Orientation = StackOrientation.Vertical
            };

            // bind the list to the simple list of strings in the VM
            list.SetBinding<SimpleListVM>(SimpleList.ItemsSourceProperty, m => m.StringList);
            // bind the item selected command
            list.SetBinding<SimpleListVM>(SimpleList.ItemSelectedCommandProperty, m => m.SelectedItemCommand);

            // create the ability to add a new item
            var newItem = new Entry
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Placeholder = "New Item"
            };
            newItem.SetBinding<SimpleListVM>(Entry.TextProperty, m => m.Entry, BindingMode.TwoWay);

            var addBtn = new Button 
            { 
                Text = "  Add  ", // spaces for padding
                HorizontalOptions = LayoutOptions.End
            };
            addBtn.SetBinding<SimpleListVM>(Button.CommandProperty, m => m.AddOrUpdateItemCommand);
            addBtn.Triggers.Add(new DataTrigger(typ

            // create a stack to hold everything
            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                Children = 
                {
                    new ScrollView // Scroll the entire contents
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        Content = 
                        new StackLayout // vertical stack of children
                        {
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            Children =
                            {
                                new Label   // Title
                                { 
                                    Text = "Auto-Binding",  
                                    HorizontalOptions = LayoutOptions.CenterAndExpand
                                },
                                new StackLayout // Add item area container
                                {
                                    Orientation = StackOrientation.Horizontal,
                                    HorizontalOptions = LayoutOptions.FillAndExpand,
                                    Children = 
                                    {
                                        newItem, addBtn
                                    }
                                },
                                list
                            }
                        }
                    }
                }
            };
        }
    }
}